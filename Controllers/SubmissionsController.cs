using System;
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

        public SubmissionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Submissions/Index?assignmentId=5 (Giảng viên xem danh sách nộp bài)
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

        // POST: /Submissions/Submit
        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int assignmentId, string content)
        {
            var userId = _userManager.GetUserId(User);
            var assignment = await _context.Assignments
                .Include(a => a.Lesson)
                    .ThenInclude(l => l!.Classroom)
                        .ThenInclude(c => c!.Enrollments)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null || !assignment.Lesson!.IsPublished) return Forbid();
            if (!assignment.Lesson.Classroom!.Enrollments.Any(e => e.StudentId == userId)) return Forbid();

            if (assignment.Deadline.HasValue && DateTime.UtcNow > assignment.Deadline.Value)
            {
                ModelState.AddModelError("", "Đã quá hạn nộp bài.");
                ViewBag.Assignment = assignment;
                return View();
            }

            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == userId);

            if (submission != null)
            {
                if (submission.Grade.HasValue)
                {
                    return Forbid();
                }
                submission.Content = content;
                submission.SubmittedAt = DateTime.UtcNow;
            }
            else
            {
                submission = new Submission
                {
                    AssignmentId = assignmentId,
                    StudentId = userId,
                    Content = content,
                    SubmittedAt = DateTime.UtcNow
                };
                _context.Submissions.Add(submission);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Submit), new { assignmentId });
        }

        // POST: /Submissions/Grade (Giảng viên đánh giá & phản hồi)
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