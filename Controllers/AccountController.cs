using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Software_and_Database_Security___Project.Data;
using Software_and_Database_Security___Project.Models;

namespace Software_and_Database_Security___Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // lockoutOnFailure: true để kích hoạt cơ chế đếm số lần sai và khóa tài khoản
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            if (result.Succeeded)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Email = model.Email,
                    Action = "Login Success",
                    IpAddress = ip,
                    IsSuccess = true
                });
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    Email = model.Email,
                    Action = "Account Locked Out",
                    IpAddress = ip,
                    IsSuccess = false
                });
                await _context.SaveChangesAsync();
                ModelState.AddModelError(string.Empty, "Tài khoản bị tạm khóa 15 phút do nhập sai quá 5 lần!");
                return View(model);
            }

            // Ghi nhận lần thử đăng nhập thất bại
            _context.AuditLogs.Add(new AuditLog
            {
                Email = model.Email,
                Action = "Login Failed",
                IpAddress = ip,
                IsSuccess = false
            });
            await _context.SaveChangesAsync();

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}