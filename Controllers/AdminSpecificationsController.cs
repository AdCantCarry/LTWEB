using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TechNova.Models.Core;
using TechNova.Models.Data;

namespace TechNova.Controllers
{
    public class AdminSpecificationsController : Controller
    {
        private readonly StoreDbContext _context;

        public AdminSpecificationsController(StoreDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách các nhóm theo danh mục
        public IActionResult Index(int? categoryId)
        {
            var categories = _context.Categories.ToList();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;

            if (categoryId.HasValue)
            {
                var groups = _context.SpecificationGroups
                    .Include(g => g.Items)
                    .Where(g => g.CategoryId == categoryId)
                    .ToList();

                return View("~/Views/Admin/AdminSpecifications/Index.cshtml", groups);
            }

            // Trường hợp chưa chọn danh mục, trả về view rỗng hoặc danh sách rỗng
            var emptyGroups = new List<SpecificationGroup>();
            return View("~/Views/Admin/AdminSpecifications/Index.cshtml", emptyGroups);
        }

        public IActionResult EditProductSpecs(int id)
        {
            var product = _context.Products
                .Include(p => p.Specifications)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null) return NotFound();

            var categoryId = product.CategoryId;
            var groups = _context.SpecificationGroups
                .Include(g => g.Items)
                .Where(g => g.CategoryId == categoryId)
                .ToList();

            ViewBag.Groups = groups;
            return View("Edit", product);
        }

        [HttpPost]
        public IActionResult EditProductSpecs(int productId, IFormCollection form)
        {
            var existingSpecs = _context.ProductSpecifications
                .Where(s => s.ProductId == productId).ToList();

            _context.ProductSpecifications.RemoveRange(existingSpecs);

            var newSpecs = new List<ProductSpecification>();
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("spec_"))
                {
                    var itemIdStr = key.Replace("spec_", "");
                    if (int.TryParse(itemIdStr, out int itemId))
                    {
                        var value = form[key];
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            newSpecs.Add(new ProductSpecification
                            {
                                ProductId = productId,
                                SpecificationItemId = itemId,
                                Value = value
                            });
                        }
                    }
                }
            }

            _context.ProductSpecifications.AddRange(newSpecs);
            _context.SaveChanges();

            return RedirectToAction("Index", "AdminProducts");
        }
        // GET: AdminSpecifications/CreateGroup
        public IActionResult CreateGroup(int? categoryId)
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.SelectedCategoryId = categoryId;

            // Điều chỉnh đường dẫn nếu view không nằm trong thư mục mặc định
            return View("~/Views/Admin/AdminSpecifications/Create.cshtml");
        }


        // POST: AdminSpecifications/CreateGroup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateGroup(SpecificationGroup group)
        {
            if (ModelState.IsValid)
            {
                // Gắn lại reference tránh lỗi EF
                foreach (var item in group.Items)
                {
                    item.Group = group;
                }

                _context.SpecificationGroups.Add(group);
                _context.SaveChanges();
                return RedirectToAction("Index", new { categoryId = group.CategoryId });
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.SelectedCategoryId = group.CategoryId;
            return View("~/Views/Admin/AdminSpecifications/Create.cshtml", group);
        }


    }

}
