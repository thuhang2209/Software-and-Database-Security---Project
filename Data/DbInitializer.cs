using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Software_and_Database_Security___Project.Data;
using Software_and_Database_Security___Project.Models;

namespace Software_and_Database_Security___Project.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Tự động khởi tạo Database và bảng nếu chưa tồn tại
            await context.Database.EnsureCreatedAsync();

            // 2. Khởi tạo 3 vai trò (Roles) theo yêu cầu hệ thống
            string[] roles = { "Admin", "Teacher", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 3. Hàm nội bộ tạo tài khoản kiểm thử nếu chưa có
            async Task CreateUserIfNotExists(string email, string name, string role)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = name,
                        EmailConfirmed = true
                    };

                    // Framework tự động hash mật khẩu qua PBKDF2/HMAC-SHA256
                    var result = await userManager.CreateAsync(user, "SecurePass@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }
            }

            // 4. Tạo các tài khoản phục vụ kịch bản kiểm thử (Acceptance Tests)
            await CreateUserIfNotExists("admin@lms.com", "System Admin", "Admin");
            await CreateUserIfNotExists("teacher1@lms.com", "Giang Vien 1", "Teacher");
            await CreateUserIfNotExists("teacher2@lms.com", "Giang Vien 2", "Teacher");
            await CreateUserIfNotExists("student1@lms.com", "Sinh Vien 1", "Student");
            await CreateUserIfNotExists("student2@lms.com", "Sinh Vien 2", "Student");
        }
    }
}