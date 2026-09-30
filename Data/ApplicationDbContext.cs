using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Software_and_Database_Security___Project.Models;

namespace Software_and_Database_Security___Project.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các DbSet tương ứng với các thực thể trong hệ thống LMS
        public DbSet<Classroom> Classrooms => Set<Classroom>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<Submission> Submissions => Set<Submission>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Bắt buộc gọi base.OnModelCreating để nạp cấu hình bảng Identity (AspNetUsers, AspNetRoles,...)
            base.OnModelCreating(builder);

            // 1. Ràng buộc an ninh cấp CSDL: Chống một sinh viên ghi danh trùng lặp vào cùng một lớp
            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.ClassroomId, e.StudentId })
                .IsUnique();

            // 2. Ràng buộc an ninh: Mỗi sinh viên chỉ có tối đa một bản nộp bài trên mỗi bài tập
            builder.Entity<Submission>()
                .HasIndex(s => new { s.AssignmentId, s.StudentId })
                .IsUnique();

            // 3. Tránh lỗi khóa ngoại vòng lặp (Cascade Delete Conflicts) trên SQL Server
            builder.Entity<Classroom>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Submission>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}