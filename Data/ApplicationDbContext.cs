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

<<<<<<< HEAD
        // Khai báo các DbSet tương ứng với các thực thể trong hệ thống LMS
=======
        // Khai báo các bảng dữ liệu của hệ thống LMS
>>>>>>> 30bd107d68c9fadbaaef7c3a2540b921e7e78ec6
        public DbSet<Classroom> Classrooms => Set<Classroom>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<Submission> Submissions => Set<Submission>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
<<<<<<< HEAD
            // Bắt buộc gọi base.OnModelCreating để nạp cấu hình bảng Identity (AspNetUsers, AspNetRoles,...)
            base.OnModelCreating(builder);

            // 1. Ràng buộc an ninh cấp CSDL: Chống một sinh viên ghi danh trùng lặp vào cùng một lớp
=======
            // Bắt buộc gọi base.OnModelCreating để cấu hình các bảng Identity (AspNetUsers, AspNetRoles,...)
            base.OnModelCreating(builder);

            // 1. Ràng buộc an ninh: Một sinh viên không thể đăng ký trùng vào cùng một lớp học
>>>>>>> 30bd107d68c9fadbaaef7c3a2540b921e7e78ec6
            builder.Entity<Enrollment>()
                .HasIndex(e => new { e.ClassroomId, e.StudentId })
                .IsUnique();

<<<<<<< HEAD
            // 2. Ràng buộc an ninh: Mỗi sinh viên chỉ có tối đa một bản nộp bài trên mỗi bài tập
=======
            // 2. Ràng buộc an ninh: Mỗi sinh viên chỉ có tối đa một bản nộp bài cho mỗi bài tập
>>>>>>> 30bd107d68c9fadbaaef7c3a2540b921e7e78ec6
            builder.Entity<Submission>()
                .HasIndex(s => new { s.AssignmentId, s.StudentId })
                .IsUnique();

<<<<<<< HEAD
            // 3. Tránh lỗi khóa ngoại vòng lặp (Cascade Delete Conflicts) trên SQL Server
=======
            // 3. Cấu hình quan hệ để tránh lỗi chu kỳ xóa (Cascade Delete Cycles) trên SQL Server
>>>>>>> 30bd107d68c9fadbaaef7c3a2540b921e7e78ec6
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