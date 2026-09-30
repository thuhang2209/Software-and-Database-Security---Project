using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Software_and_Database_Security___Project.Data;
using Software_and_Database_Security___Project.Models;

namespace Software_and_Database_Security___Project.Controllers
{
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

        public async Task<IActionResult> Index()
        {
            var classrooms = await _context.Classrooms
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .ToListAsync();
            return View(classrooms);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string teacherId)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(teacherId))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ tên lớp và chọn giảng viên.");
                return await Create();
            }

            var classroom = new Classroom { Name = name, TeacherId = teacherId };
            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null) return NotFound();

            var students = await _userManager.GetUsersInRoleAsync("Student");
            var enrolledStudentIds = classroom.Enrollments.Select(e => e.StudentId).ToList();
            ViewBag.AvailableStudents = students.Where(s => !enrolledStudentIds.Contains(s.Id)).ToList();

            return View(classroom);
        }

        [HttpPost]
        public async Task<IActionResult> EnrollStudent(int classId, string studentId)
        {
            var exists = await _context.Enrollments.AnyAsync(e => e.ClassroomId == classId && e.StudentId == studentId);
            if (!exists)
            {
                _context.Enrollments.Add(new Enrollment { ClassroomId = classId, StudentId = studentId });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = classId });
        }
    }
}