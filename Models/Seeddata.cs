using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using TechNova.Helpers;

namespace TechNova.Models
{
    public static class Seeddata
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new StoreDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<StoreDbContext>>()))
            {
                // Tạo Category nếu chưa có
                if (!context.Categories.Any())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Laptop", MainImageUrl = "/images/AcerSwiftX.jpg" },
                        new Category { Name = "Điện thoại", MainImageUrl = "/images/ip15.jpg" },
                        new Category { Name = "Phụ kiện", MainImageUrl = "/images/SonyWH-1000XM5.jpg" }
                    );
                    context.SaveChanges();
                }


                // Map tên -> ID để dễ dùng
                var categories = context.Categories.ToDictionary(c => c.Name, c => c.CategoryId);

                // Tạo user Admin nếu chưa có
                if (!context.Users.Any(u => u.Email == "admin@example.com"))
                {
                    context.Users.Add(new User
                    {
                        Username = "Admin",
                        Email = "admin@example.com",
                        Password = PasswordHelper.Hash("123456"),
                        Role = "Admin"
                    });
                }

                // Tạo sản phẩm
                if (!context.Products.Any())
                {
                    var products = new List<Product>
                    {
                        // Laptops
                        Create("Acer Swift X", 18990000, "Laptop", categories),
                        Create("Dell XPS 13 Plus", 29990000, "Laptop", categories),
                        Create("MSI Creator Z17", 35990000, "Laptop", categories),
                        Create("ASUS ROG Strix G16", 32990000, "Laptop", categories),
                        Create("MacBook Air M1", 19990000, "Laptop", categories),
                        Create("MacBook Air M2", 25990000, "Laptop", categories),
                        Create("MacBook Air M3", 28990000, "Laptop", categories),
                        Create("MacBook Pro M3 Pro", 49990000, "Laptop", categories),
                        Create("MacBook Pro M3 Max", 69990000, "Laptop", categories),

                        // Điện thoại
                        Create("iPhone X", 7990000, "Điện thoại", categories),
                        Create("iPhone 11", 10990000, "Điện thoại", categories),
                        Create("iPhone 12", 12990000, "Điện thoại", categories),
                        Create("iPhone 13", 15490000, "Điện thoại", categories),
                        Create("iPhone 14", 18990000, "Điện thoại", categories),
                        Create("Samsung Galaxy A74", 7990000, "Điện thoại", categories),
                        Create("Samsung Galaxy S24 Ultra", 31990000, "Điện thoại", categories),
                        Create("Samsung Galaxy Z Flip5", 23990000, "Điện thoại", categories),
                        Create("Samsung Galaxy Z Fold5", 38990000, "Điện thoại", categories),

                        // Phụ kiện - Đồng hồ
                        Create("Apple Watch Series 9", 10990000, "Phụ kiện", categories),
                        Create("Apple Watch Ultra 2", 19990000, "Phụ kiện", categories),
                        Create("Garmin Venu 3", 8990000, "Phụ kiện", categories),
                        Create("Huawei Watch 4 Pro", 8990000, "Phụ kiện", categories),
                        Create("Samsung Galaxy Watch 6 Classic", 9490000, "Phụ kiện", categories),

                        // Phụ kiện - Bàn phím
                        Create("Akko 3068B Plus", 1690000, "Phụ kiện", categories),
                        Create("Keychron K6", 1890000, "Phụ kiện", categories),
                        Create("Leopold FC750R", 2590000, "Phụ kiện", categories),
                        Create("Logitech MX Keys S", 2390000, "Phụ kiện", categories),
                        Create("Razer Huntsman V2 Analog", 4290000, "Phụ kiện", categories),

                        // Phụ kiện - Chuột
                        Create("Apple Magic Mouse 2", 2290000, "Phụ kiện", categories),
                        Create("Fuhlen G90 Pro", 590000, "Phụ kiện", categories),
                        Create("Logitech G304", 790000, "Phụ kiện", categories),
                        Create("Logitech MX Master 3S", 2390000, "Phụ kiện", categories),
                        Create("Razer DeathAdder V2", 1290000, "Phụ kiện", categories),

                        // Phụ kiện - Tai nghe
                        Create("Apple AirPods Pro 2", 5290000, "Phụ kiện", categories),
                        Create("Logitech G435", 1390000, "Phụ kiện", categories),
                        Create("Samsung Galaxy Buds2 Pro", 3990000, "Phụ kiện", categories),
                        Create("Sony WH-1000XM5", 7990000, "Phụ kiện", categories),
                        Create("SoundPEATS Air3 Deluxe HS", 890000, "Phụ kiện", categories),
                    };

                    context.Products.AddRange(products);
                }

                context.SaveChanges();
            }
        }

        private static Product Create(string name, decimal price, string categoryName, Dictionary<string, int> categoryMap)
        {
            var baseName = name.ToLower()
                .Replace(" ", "_")
                .Replace("+", "")
                .Replace(".", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("-", "_");

            return new Product
            {
                Name = name,
                Description = $"Sản phẩm {name} chất lượng cao, chính hãng.",
                Price = price,
                DiscountPercent = 0,
                MainImageUrl = "/images/" + baseName + ".jpg",
                SubImage1Url = "/images/" + baseName + "_1.jpg",
                SubImage2Url = "/images/" + baseName + "_2.jpg",
                SubImage3Url = "/images/" + baseName + "_3.jpg",
                Color = "Đen", // hoặc bạn có thể random hoặc thiết lập tùy loại
                Storage = "128GB", // tương tự
                CategoryId = categoryMap[categoryName],
                CreatedAt = DateTime.Now
            };
        }

    }
}
