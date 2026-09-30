using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Software_and_Database_Security___Project.Data;
using Software_and_Database_Security___Project.Models;

namespace Software_and_Database_Security___Project.Controllers
{
    [Authorize]
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LessonsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Danh sách bài học của một lớp
        public async Task<IActionResult> Index(int classId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var classroom = await _context.Classrooms
                .Include(c => c.Lessons)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classroom == null) return NotFound();

            // Kiểm soát quyền truy cập
            if (User.IsInRole("Teacher") && classroom.TeacherId != currentUserId)
                return Forbid();

            if (User.IsInRole("Student"))
            {
                var isEnrolled = classroom.Enrollments.Any(e => e.StudentId == currentUserId);
                if (!isEnrolled) return Forbid();
            }

            ViewBag.Classroom = classroom;
            return View(classroom.Lessons.ToList());
        }

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(int classId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == classId && c.TeacherId == currentUserId);
            if (classroom == null) return Forbid();

            ViewBag.ClassId = classId;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(int classId, string title, string content)
        {
            var currentUserId = _userManager.GetUserId(User);
            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == classId && c.TeacherId == currentUserId);
            if (classroom == null) return Forbid();

            var lesson = new Lesson
            {
                ClassroomId = classId,
                Title = title,
                Content = content,
                IsPublished = false // Mặc định là Draft
            };
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { classId });
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Publish(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var lesson = await _context.Lessons
                .Include(l => l.Classroom)
                .FirstOrDefaultAsync(l => l.Id == id && l.Classroom!.TeacherId == currentUserId);

            if (lesson == null) return Forbid();

            lesson.IsPublished = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { classId = lesson.ClassroomId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var lesson = await _context.Lessons
                .Include(l => l.Classroom)
                .Include(l => l.Assignments)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) return NotFound();

            // Chặn Teacher khác
            if (User.IsInRole("Teacher") && lesson.Classroom!.TeacherId != currentUserId)
                return Forbid();

            // Chặn Student nếu chưa ghi danh hoặc bài chưa xuất bản
            if (User.IsInRole("Student"))
            {
                var isEnrolled = await _context.Enrollments.AnyAsync(e => e.ClassroomId == lesson.ClassroomId && e.StudentId == currentUserId);
                if (!isEnrolled || !lesson.IsPublished)
                    return Forbid();
            }

            return View(lesson);
        }
    }
}