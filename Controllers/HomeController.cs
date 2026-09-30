using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Software_and_Database_Security___Project.Data;
using Software_and_Database_Security___Project.Models;

namespace Software_and_Database_Security___Project.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            List<Classroom> classrooms = new List<Classroom>();

            if (User.IsInRole("Admin"))
            {
                // Admin xem được tất cả các lớp trong trường
                classrooms = await _context.Classrooms
                    .Include(c => c.Teacher)
                    .Include(c => c.Enrollments)
                    .ToListAsync();
            }
            else if (User.IsInRole("Teacher"))
            {
                // Giảng viên chỉ xem danh sách lớp do mình giảng dạy (Yêu cầu 4 & 6)
                classrooms = await _context.Classrooms
                    .Where(c => c.TeacherId == userId)
                    .Include(c => c.Teacher)
                    .Include(c => c.Enrollments)
                    .ToListAsync();
            }
            else if (User.IsInRole("Student"))
            {
                // Sinh viên chỉ xem các lớp mình đã được ghi danh
                classrooms = await _context.Enrollments
                    .Where(e => e.StudentId == userId)
                    .Include(e => e.Classroom!)
                        .ThenInclude(c => c.Teacher)
                    .Include(e => e.Classroom!)
                        .ThenInclude(c => c.Enrollments)
                    .Select(e => e.Classroom!)
                    .ToListAsync();
            }

            // Đảm bảo luôn trả về List (không bao giờ null)
            return View(classrooms ?? new List<Classroom>());
        }
    }
}