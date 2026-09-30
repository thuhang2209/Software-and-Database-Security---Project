using Microsoft.AspNetCore.Identity;

namespace Software_and_Database_Security___Project.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }

    public class Classroom
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TeacherId { get; set; } = string.Empty;
        public ApplicationUser? Teacher { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }

    public class Enrollment
    {
        public int Id { get; set; }
        public int ClassroomId { get; set; }
        public Classroom? Classroom { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser? Student { get; set; }
    }

    public class Lesson
    {
        public int Id { get; set; }
        public int ClassroomId { get; set; }
        public Classroom? Classroom { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = false;
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }

    public class Assignment
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }

    public class Submission
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public Assignment? Assignment { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser? Student { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public double? Grade { get; set; }
        public string? Feedback { get; set; }
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsSuccess { get; set; }
    }
}