using System;
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
    public class SubmissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public SubmissionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: /Submissions/Index?assignmentId=5 (Giảng viên xem danh sách nộp)
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Index(int assignmentId)
        {
            var userId = _userManager.GetUserId(User);
            var assignment = await _context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l!.Classroom)
                        .ThenInclude(c => c!.Enrollments)
                            .ThenInclude(e => e.Student)
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Student)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null || assignment.Lesson?.Classroom?.TeacherId != userId)
            {
                return Forbid();
            }

            return View(assignment);
        }

        // GET: /Submissions/Submit?assignmentId=5 (Sinh viên vào giao diện nộp bài)
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(int assignmentId)
        {
            var userId = _userManager.GetUserId(User);
            var assignment = await _context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l!.Classroom)
                        .ThenInclude(c => c!.Enrollments)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) return NotFound();
            if (!assignment.Lesson!.IsPublished) return Forbid();
            if (!assignment.Lesson.Classroom!.Enrollments.Any(e => e.StudentId == userId)) return Forbid();

            var existingSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == userId);

            ViewBag.Assignment = assignment;
            return View(existingSubmission);
        }

        // POST: /Submissions/Submit (Xử lý văn bản + Tệp bài làm)
        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int assignmentId, string? content, IFormFile? submissionFile)
        {
            var userId = _userManager.GetUserId(User);
            var assignment = await _context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l!.Classroom)
                        .ThenInclude(c => c!.Enrollments)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null || !assignment.Lesson!.IsPublished) return Forbid();
            if (!assignment.Lesson.Classroom!.Enrollments.Any(e => e.StudentId == userId)) return Forbid();

            // Kiểm tra hạn nộp
            if (assignment.Deadline.HasValue && DateTime.UtcNow > assignment.Deadline.Value)
            {
                ModelState.AddModelError("", "Đã quá hạn nộp bài.");
                ViewBag.Assignment = assignment;
                var currSub = await _context.Submissions.FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == userId);
                return View(currSub);
            }

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == userId);

            if (submission != null && submission.Grade.HasValue)
            {
                return Forbid(); // Đã chấm điểm thì không sửa đổi
            }

            string? savedFileName = submission?.SubmissionFilePath;
            string? originalFileName = submission?.SubmissionFileName;

            // Xử lý tệp tải lên nếu sinh viên đính kèm
            if (submissionFile != null && submissionFile.Length > 0)
            {
                if (submissionFile.Length > 20 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Tệp không được vượt quá 20MB.");
                    ViewBag.Assignment = assignment;
                    return View(submission);
                }

                var allowedExts = new[] { ".pdf", ".docx", ".doc", ".zip", ".rar", ".txt", ".cs", ".cpp", ".sql", ".py" };
                var ext = Path.GetExtension(submissionFile.FileName).ToLowerInvariant();
                if (!allowedExts.Contains(ext))
                {
                    ModelState.AddModelError("", "Định dạng tệp không được hỗ trợ. Chấp nhận: .pdf, .docx, .zip, .rar, .txt, .cs, .sql, v.v.");
                    ViewBag.Assignment = assignment;
                    return View(submission);
                }

                var uploadDir = Path.Combine(_env.ContentRootPath, "App_Data", "Submissions");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                savedFileName = $"{Guid.NewGuid()}{ext}";
                originalFileName = Path.GetFileName(submissionFile.FileName);
                var fullPath = Path.Combine(uploadDir, savedFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await submissionFile.CopyToAsync(stream);
                }
            }

            if (submission != null)
            {
                submission.Content = content ?? string.Empty;
                submission.SubmissionFileName = originalFileName;
                submission.SubmissionFilePath = savedFileName;
                submission.SubmittedAt = DateTime.UtcNow;
            }
            else
            {
                submission = new Submission
                {
                    AssignmentId = assignmentId,
                    StudentId = userId,
                    Content = content ?? string.Empty,
                    SubmissionFileName = originalFileName,
                    SubmissionFilePath = savedFileName,
                    SubmittedAt = DateTime.UtcNow
                };
                _context.Submissions.Add(submission);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Submit), new { assignmentId });
        }

        // GET: Tải tệp bài nộp an toàn (Kiểm soát quyền giữa Giảng viên và Sinh viên)
        [HttpGet]
        public async Task<IActionResult> DownloadSubmissionFile(int submissionId)
        {
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a!.Lesson)
                        .ThenInclude(l => l!.Classroom)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null || string.IsNullOrEmpty(submission.SubmissionFilePath)) return NotFound();

            var userId = _userManager.GetUserId(User);
            bool isTeacher = User.IsInRole("Teacher") && submission.Assignment?.Lesson?.Classroom?.TeacherId == userId;
            bool isStudentOwner = User.IsInRole("Student") && submission.StudentId == userId;

            if (!isTeacher && !isStudentOwner) return Forbid();

            var filePath = Path.Combine(_env.ContentRootPath, "App_Data", "Submissions", submission.SubmissionFilePath);
            if (!System.IO.File.Exists(filePath)) return NotFound("Tệp bài làm không tồn tại trên hệ thống.");

            return PhysicalFile(filePath, "application/octet-stream", submission.SubmissionFileName ?? "bailam");
        }

        // POST: /Submissions/Grade
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int submissionId, double grade, string feedback)
        {
            var userId = _userManager.GetUserId(User);
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a!.Lesson)
                        .ThenInclude(l => l!.Classroom)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null || submission.Assignment?.Lesson?.Classroom?.TeacherId != userId)
            {
                return Forbid();
            }

            submission.Grade = Math.Clamp(grade, 0, 10);
            submission.Feedback = feedback;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { assignmentId = submission.AssignmentId });
        }
    }
}