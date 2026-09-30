using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Software_and_Database_Security___Project.Data;
using Software_and_Database_Security___Project.Models;

public static class DbInitializer
{
	public static async Task SeedAsync(IServiceProvider serviceProvider)
	{
		var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
		var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
		var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

		// 1. Tự động tạo Database và cấu trúc bảng nếu chưa tồn tại
		await context.Database.EnsureCreatedAsync();

		// 2. Tạo 3 Role cơ bản
		string[] roles = { "Admin", "Teacher", "Student" };
		foreach (var role in roles)
		{
			if (!await roleManager.RoleExistsAsync(role))
				await roleManager.CreateAsync(new IdentityRole(role));
		}

		// 3. Hàm phụ trợ tạo tài khoản mẫu an toàn
		async Task CreateUserIfNotExists(string email, string name, string role)
		{
			if (await userManager.FindByEmailAsync(email) == null)
			{
				var user = new ApplicationUser { UserName = email, Email = email, FullName = name };
				var result = await userManager.CreateAsync(user, "SecurePass@123");
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(user, role);
				}
			}
		}

		// Tạo đủ các tài khoản mẫu kiểm thử theo kịch bản:
		await CreateUserIfNotExists("admin@lms.com", "System Admin", "Admin");
		await CreateUserIfNotExists("teacher1@lms.com", "Giang Vien 1", "Teacher");
		await CreateUserIfNotExists("teacher2@lms.com", "Giang Vien 2", "Teacher");
		await CreateUserIfNotExists("student1@lms.com", "Sinh Vien 1", "Student");
		await CreateUserIfNotExists("student2@lms.com", "Sinh Vien 2", "Student");
	}
}