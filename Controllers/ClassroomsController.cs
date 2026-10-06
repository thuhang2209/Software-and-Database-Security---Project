using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Software_and_Database_Security___Project.Data;
using Software_and_Database_Security___Project.Models;

namespace Software_and_Database_Security___Project.Controllers
{
    // Bắt buộc vai trò Administrator theo đúng ma trận phân quyền (Authorization Matrix)
    [Authorize(Roles = "Admin")]
    public class ClassroomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClassroomsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Classrooms hoặc /Classrooms/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var classrooms = await _context.Classrooms
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .ToListAsync();

            return View(classrooms);
        }

        // GET: /Classrooms/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");
            return View();
        }

        // POST: /Classrooms/Create (Chống CSRF)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string teacherId)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(teacherId))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập tên lớp và chỉ định giảng viên.");
                var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
                ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");
                return View();
            }

            var classroom = new Classroom
            {
                Name = name,
                TeacherId = teacherId
            };

            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Classrooms/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null) return NotFound();

            // Lấy danh sách học viên chưa tham gia lớp này
            var allStudents = await _userManager.GetUsersInRoleAsync("Student");
            var enrolledStudentIds = classroom.Enrollments.Select(e => e.StudentId).ToHashSet();
            ViewBag.AvailableStudents = allStudents.Where(s => !enrolledStudentIds.Contains(s.Id)).ToList();

            return View(classroom);
        }

        // POST: /Classrooms/EnrollStudent (Chống duplicate enrollments)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudent(int classId, string studentId)
        {
            // Kiểm tra chống trùng lặp theo quy chuẩn an toàn CSDL
            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.ClassroomId == classId && e.StudentId == studentId);

            if (!alreadyEnrolled)
            {
                _context.Enrollments.Add(new Enrollment
                {
                    ClassroomId = classId,
                    StudentId = studentId
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = classId });
        }
    }
}