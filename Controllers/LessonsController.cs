using System.IO;
using System.Linq;
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
        private readonly IWebHostEnvironment _env;

        public LessonsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: /Lessons/Index?classId=2
        public async Task<IActionResult> Index(int classId)
        {
            var userId = _userManager.GetUserId(User);
            var cls = await _context.Classrooms
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Assignments)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (cls == null) return NotFound();

            if (User.IsInRole("Teacher") && cls.TeacherId != userId) return Forbid();
            if (User.IsInRole("Student") && !cls.Enrollments.Any(e => e.StudentId == userId)) return Forbid();

            // Nếu là Admin: Lấy thêm danh sách sinh viên chưa được ghi danh để hiển thị trong form dropdown
            if (User.IsInRole("Admin"))
            {
                var allStudents = await _userManager.GetUsersInRoleAsync("Student");
                var enrolledStudentIds = cls.Enrollments.Select(e => e.StudentId).ToHashSet();
                ViewBag.AvailableStudents = allStudents.Where(s => !enrolledStudentIds.Contains(s.Id)).ToList();
            }

            ViewBag.Classroom = cls;
            return View(cls.Lessons.ToList());
        }

        // POST: /Lessons/EnrollStudent (Admin thêm học viên trực tiếp tại trang lớp học)
        [HttpPost]
        [Authorize(Roles = "Admin")]
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

            return RedirectToAction(nameof(Index), new { classId });
        }

        // GET: /Lessons/Create?classId=2
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(int classId)
        {
            var userId = _userManager.GetUserId(User);
            var cls = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == classId && c.TeacherId == userId);
            if (cls == null) return Forbid();

            ViewBag.ClassId = classId;
            return View();
        }

        // POST: /Lessons/Create
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int classId, string title, string content, IFormFile? attachment)
        {
            var userId = _userManager.GetUserId(User);
            var cls = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == classId && c.TeacherId == userId);
            if (cls == null) return Forbid();

            string? savedFileName = null;
            string? originalFileName = null;

            if (attachment != null && attachment.Length > 0)
            {
                if (attachment.Length > 15 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Kích thước tệp tin không được vượt quá 15MB.");
                    ViewBag.ClassId = classId;
                    return View();
                }

                var allowedExts = new[] { ".ppt", ".pptx", ".doc", ".docx", ".pdf", ".md", ".txt" };
                var ext = Path.GetExtension(attachment.FileName).ToLowerInvariant();
                if (!allowedExts.Contains(ext))
                {
                    ModelState.AddModelError("", "Định dạng tệp không được hỗ trợ. Chỉ chấp nhận .ppt, .pptx, .doc, .docx, .pdf, .md, .txt.");
                    ViewBag.ClassId = classId;
                    return View();
                }

                var uploadDir = Path.Combine(_env.ContentRootPath, "App_Data", "Uploads");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                savedFileName = $"{Guid.NewGuid()}{ext}";
                originalFileName = Path.GetFileName(attachment.FileName);
                var fullPath = Path.Combine(uploadDir, savedFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await attachment.CopyToAsync(stream);
                }
            }

            var lesson = new Lesson
            {
                ClassroomId = classId,
                Title = title,
                Content = content,
                IsPublished = false,
                AttachmentFileName = originalFileName,
                AttachmentFilePath = savedFileName
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { classId });
        }

        // GET: /Lessons/DownloadFile?lessonId=5
        [HttpGet]
        public async Task<IActionResult> DownloadFile(int lessonId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Classroom)
                    .ThenInclude(c => c!.Enrollments)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null || string.IsNullOrEmpty(lesson.AttachmentFilePath)) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (User.IsInRole("Teacher") && lesson.Classroom?.TeacherId != userId) return Forbid();
            if (User.IsInRole("Student"))
            {
                if (!lesson.IsPublished) return Forbid();
                if (!lesson.Classroom!.Enrollments.Any(e => e.StudentId == userId)) return Forbid();
            }

            var filePath = Path.Combine(_env.ContentRootPath, "App_Data", "Uploads", lesson.AttachmentFilePath);
            if (!System.IO.File.Exists(filePath)) return NotFound("Tệp tài liệu không tồn tại trên hệ thống.");

            return PhysicalFile(filePath, "application/octet-stream", lesson.AttachmentFileName ?? "document");
        }

        // POST: /Lessons/Publish/5
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var userId = _userManager.GetUserId(User);
            var lesson = await _context.Lessons.Include(l => l.Classroom).FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null || lesson.Classroom?.TeacherId != userId) return Forbid();

            lesson.IsPublished = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { classId = lesson.ClassroomId });
        }

        // GET: /Lessons/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var lesson = await _context.Lessons
                .Include(l => l.Classroom)
                    .ThenInclude(c => c!.Enrollments)
                .Include(l => l.Assignments)
                    .ThenInclude(a => a.Submissions)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) return NotFound();

            if (User.IsInRole("Teacher") && lesson.Classroom?.TeacherId != userId) return Forbid();
            if (User.IsInRole("Student"))
            {
                if (!lesson.IsPublished) return Forbid();
                if (!lesson.Classroom!.Enrollments.Any(e => e.StudentId == userId)) return Forbid();
            }

            return View(lesson);
        }

        // POST: /Lessons/CreateAssignment (Tạo BTVN / Bài kiểm tra có kèm file đề bài)
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(
            int lessonId,
            string title,
            string instructions,
            string assignmentType,
            DateTime? deadline,
            IFormFile? assignmentFile)
        {
            var userId = _userManager.GetUserId(User);
            var lesson = await _context.Lessons
                .AsNoTracking()
                .Include(l => l.Classroom)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null || lesson.Classroom?.TeacherId != userId) return Forbid();

            string? savedFileName = null;
            string? originalFileName = null;

            if (assignmentFile != null && assignmentFile.Length > 0)
            {
                if (assignmentFile.Length > 20 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Tệp đề bài không được vượt quá 20MB.");
                    return RedirectToAction(nameof(Details), new { id = lessonId });
                }

                var allowedExts = new[] { ".pdf", ".docx", ".doc", ".ppt", ".pptx", ".zip", ".rar", ".txt", ".md" };
                var ext = Path.GetExtension(assignmentFile.FileName).ToLowerInvariant();
                if (!allowedExts.Contains(ext))
                {
                    ModelState.AddModelError("", "Định dạng tệp không hợp lệ.");
                    return RedirectToAction(nameof(Details), new { id = lessonId });
                }

                var uploadDir = Path.Combine(_env.ContentRootPath, "App_Data", "Assignments");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                savedFileName = $"{Guid.NewGuid()}{ext}";
                originalFileName = Path.GetFileName(assignmentFile.FileName);
                var fullPath = Path.Combine(uploadDir, savedFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await assignmentFile.CopyToAsync(stream);
                }
            }

            var assignment = new Assignment
            {
                LessonId = lessonId,
                Title = title,
                Instructions = instructions,
                AssignmentType = assignmentType,
                Deadline = deadline,
                AssignmentFileName = originalFileName,
                AssignmentFilePath = savedFileName
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = lessonId });
        }

        // GET: /Lessons/DownloadAssignmentFile?assignmentId=5 (Tải tệp đề bài)
        [HttpGet]
        public async Task<IActionResult> DownloadAssignmentFile(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l!.Classroom)
                        .ThenInclude(c => c!.Enrollments)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null || string.IsNullOrEmpty(assignment.AssignmentFilePath)) return NotFound();

            var userId = _userManager.GetUserId(User);

            // Kiểm tra quyền hạn xem bài: Giảng viên của lớp hoặc Sinh viên đã ghi danh
            if (User.IsInRole("Teacher") && assignment.Lesson?.Classroom?.TeacherId != userId) return Forbid();
            if (User.IsInRole("Student"))
            {
                if (!assignment.Lesson!.IsPublished) return Forbid();
                if (!assignment.Lesson.Classroom!.Enrollments.Any(e => e.StudentId == userId)) return Forbid();
            }

            var filePath = Path.Combine(_env.ContentRootPath, "App_Data", "Assignments", assignment.AssignmentFilePath);
            if (!System.IO.File.Exists(filePath)) return NotFound("Tệp đề bài không tồn tại.");

            return PhysicalFile(filePath, "application/octet-stream", assignment.AssignmentFileName ?? "DeBai");
        }
    }
}