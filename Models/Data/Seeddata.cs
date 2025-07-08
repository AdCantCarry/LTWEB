using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using TechNova.Models.Core;
using TechNova.Models.Auth;

namespace TechNova.Models.Data
{
    public static class Seeddata
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new StoreDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<StoreDbContext>>()))
            {
                // Danh mục
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Laptop", MainImageUrl = "/images/ảnh thư mục/Laptop.png" },
                        new Category { Name = "Điện thoại", MainImageUrl = "/images/ảnh thư mục/ss.png" },
                        new Category { Name = "Tai nghe", MainImageUrl = "/images/ảnh thư mục/tainghe.png" },
                        new Category { Name = "Đồng hồ", MainImageUrl = "/images/ảnh thư mục/dongho.png" },
                        new Category { Name = "Bàn phím", MainImageUrl = "/images/ảnh thư mục/banphim.jpg" },
                        new Category { Name = "Chuột", MainImageUrl = "/images/ảnh thư mục/Chuot.png" }
                    );
                    context.SaveChanges();
                }

                // Tài khoản Admin mặc định
                if (!context.Users.Any(u => u.Email == "admin@example.com"))
                {
                    context.Users.Add(new User
                    {
                        Username = "Admin",
                        Email = "admin@example.com",
                        Password = PasswordHelper.Hash("123456"),
                        Role = "Admin"
                    });
                    context.SaveChanges();
                }
            }
        }
    }
}
