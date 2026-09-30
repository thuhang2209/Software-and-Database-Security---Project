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

        public SubmissionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Danh sách bài nộp cho Giáo viên xem và chấm điểm
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Index(int assignmentId)
        {
            var teacherId = _userManager.GetUserId(User);
            var assignment = await _context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l!.Classroom)
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Student)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) return NotFound();

            // Yêu cầu 6: Chặn giáo viên lớp khác
            if (assignment.Lesson!.Classroom!.TeacherId != teacherId)
                return Forbid();

            return View(assignment);
        }

        // Học sinh mở trang nộp bài
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(int assignmentId)
        {
            var studentId = _userManager.GetUserId(User)!;
            var assignment = await _context.Assignments
                .Include(a => a.Lesson)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null || !assignment.Lesson!.IsPublished) return Forbid();

            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.ClassroomId == assignment.Lesson.ClassroomId && e.StudentId == studentId);
            if (!isEnrolled) return Forbid();

            var currentSubmission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

            ViewBag.Assignment = assignment;
            return View(currentSubmission);
        }

        // Xử lý nộp bài
        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(int assignmentId, string content)
        {
            var studentId = _userManager.GetUserId(User)!;
            var assignment = await _context.Assignments
                .Include(a => a.Lesson)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null || !assignment.Lesson!.IsPublished) return Forbid();

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

            if (submission != null && submission.Grade.HasValue)
            {
                ModelState.AddModelError("", "Bài làm đã được chấm điểm, không thể chỉnh sửa.");
                ViewBag.Assignment = assignment;
                return View(submission);
            }

            if (submission == null)
            {
                submission = new Submission
                {
                    AssignmentId = assignmentId,
                    StudentId = studentId,
                    Content = content,
                    SubmittedAt = DateTime.UtcNow
                };
                _context.Submissions.Add(submission);
            }
            else
            {
                submission.Content = content;
                submission.SubmittedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Lessons", new { id = assignment.LessonId });
        }

        // Giáo viên vào chấm điểm
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Grade(int submissionId, double grade, string feedback)
        {
            var teacherId = _userManager.GetUserId(User);
            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a!.Lesson)
                        .ThenInclude(l => l!.Classroom)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission == null) return NotFound();

            // Yêu cầu 6: Xác thực quyền sở hữu lớp trước khi chấm
            if (submission.Assignment!.Lesson!.Classroom!.TeacherId != teacherId)
                return Forbid();

            submission.Grade = grade;
            submission.Feedback = feedback;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { assignmentId = submission.AssignmentId });
        }
    }
}