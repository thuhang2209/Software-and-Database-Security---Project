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

        public string? AttachmentFileName { get; set; } // Tên file hiển thị (vd: Slide_Bai1.pptx)
        public string? AttachmentFilePath { get; set; } // Tên file lưu trên ổ đĩa (vd: 3f8a-...pptx)

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }

    public class Assignment
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;

        // Phân loại: "Homework" (BTVN) hoặc "Quiz" (Bài kiểm tra)
        public string AssignmentType { get; set; } = "Homework";

        public string? AssignmentFileName { get; set; } // Tên gốc (vd: DeThi_KiemTra15P.pdf)
        public string? AssignmentFilePath { get; set; } // Tên mã hóa GUID lưu trên ổ đĩa

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

        public string? SubmissionFileName { get; set; } // Tên tệp gốc sinh viên gửi (ví dụ: BaiTap1.zip)
        public string? SubmissionFilePath { get; set; } // Tên GUID lưu an toàn trên ổ đĩa

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