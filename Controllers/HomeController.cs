using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using TechNova.Models;

namespace TechNova.Controllers
{
    public class HomeController : Controller
    {
        private readonly StoreDbContext _context;

        public HomeController(StoreDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            var saleProducts = _context.Products
                        .Where(p => p.DiscountPercent > 0)
                        .OrderByDescending(p => p.DiscountPercent)
                        .Take(15) // giới hạn 15 sản phẩm
                        .ToList();
            var appleProducts = _context.Products
                .Where(p => p.BrandId == 1)
                .Include(p => p.Brand)
                .Take(5)
                .ToList();

            var samsungProducts = _context.Products
                .Where(p => p.Brand.Name.Contains("Samsung") && p.IsActive)
                .Include(p => p.Brand)
                .Take(5)
                .ToList();

            ViewBag.SaleProducts = saleProducts;
            ViewBag.AppleProducts = appleProducts;
            ViewBag.SamsungProducts = samsungProducts;

            return View();
        }
        public IActionResult Store()
        {
            var products = _context.Products.Include(p => p.Category).ToList();
            ViewBag.Categories = _context.Categories.ToList();
            return View(products);
        }
        public IActionResult Products(string search, int? categoryId, string sort, int? minPrice, int? maxPrice, List<int> brands)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (brands != null && brands.Any())
                query = query.Where(p => p.BrandId.HasValue && brands.Contains(p.BrandId.Value));

            if (minPrice.HasValue)
            {
                query = query.Where(p =>
                    (p.DiscountPercent.HasValue
                        ? p.Price * (1 - p.DiscountPercent.Value / 100m)
                        : p.Price) >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p =>
                    (p.DiscountPercent.HasValue
                        ? p.Price * (1 - p.DiscountPercent.Value / 100m)
                        : p.Price) <= maxPrice.Value);
            }

            query = sort switch
            {
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "price_asc" => query.OrderBy(p =>
                    (p.DiscountPercent.HasValue
                        ? p.Price * (1 - p.DiscountPercent.Value / 100m)
                        : p.Price)),
                "price_desc" => query.OrderByDescending(p =>
                    (p.DiscountPercent.HasValue
                        ? p.Price * (1 - p.DiscountPercent.Value / 100m)
                        : p.Price)),
                _ => query
            };

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Brands = _context.Brands.ToList();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedSort = sort;
            ViewBag.SearchQuery = search;
            ViewBag.SelectedBrands = brands ?? new List<int>();
            ViewBag.MinPrice = minPrice ?? 0;
            ViewBag.MaxPrice = maxPrice ?? 100000000;

            return View(query.ToList());
        }

        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            return View(product);
        }

    }
}
