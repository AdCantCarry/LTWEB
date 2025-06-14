using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using TechNova.Helpers; // ✅ dùng để mã hóa mật khẩu

namespace TechNova.Models
{
    public static class Seeddata
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new StoreDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<StoreDbContext>>()))
            {
                // ✅ Seed User Admin nếu chưa có
                if (!context.Users.Any(u => u.Email == "admin@example.com"))
                {
                    context.Users.Add(new User
                    {
                        Username = "Admin",
                        Email = "admin@example.com",
                        Password = PasswordHelper.Hash("123456"), // ✅ Mật khẩu được mã hóa
                        Role = "Admin"
                    });
                }

                // ✅ Seed sản phẩm nếu chưa có
                if (!context.Products.Any())
                {
                    context.Products.AddRange(
                        new Product
                        {
                            Name = "MacBook Air M2",
                            Price = 30000000,
                            DiscountPercent = 50,
                            ImageUrl = "/images/macbook-air.jpg"
                        },
                        new Product
                        {
                            Name = "Samsung Galaxy S24",
                            Price = 22000000,
                            DiscountPercent = 10,
                            ImageUrl = "/images/galaxy.jpg"
                        },
                        new Product
                        {
                            Name = "Logitech MX Master 3",
                            Price = 2500000,
                            DiscountPercent = null,
                            ImageUrl = "/images/mouse.jpg"
                        },
                        new Product
                        {
                            Name = "Iphone 16 Pro Max",
                            Price = 2500000,
                            DiscountPercent = null,
                            ImageUrl = "/images/mouse.jpg"
                        },
                        new Product
                        {
                            Name = "Samsung galaxy S21 Ultra",
                            Price = 9100000,
                            DiscountPercent = null,
                            ImageUrl = "/images/mouse.jpg"
                        }
                    );
                }

                context.SaveChanges();
            }
        }
    }
}
