using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.middleware;
using TechNova.Models.Core;
using TechNova.Models.Data;

namespace TechNova.Controllers
{
    [AdminAuthorize]
    public class AdminCategoriesController : Controller
    {
        private readonly StoreDbContext _context;
        public AdminCategoriesController(StoreDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.
                Include(c => c.Products)
                .ToList();
            return View("~/Views/Admin/AdminCategories/Index.cshtml", categories);
        }

        public IActionResult Create()
        {
            return View("~/Views/Admin/AdminCategories/Create.cshtml");
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("~/Views/Admin/AdminCategories/Create.cshtml", category);
        }

        public IActionResult Edit(int id)
        {
            var cat = _context.Categories.Find(id);
            return View("~/Views/Admin/AdminCategories/Edit.cshtml", cat);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("~/Views/Admin/AdminCategories/Edit.cshtml", category);
        }

        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);
            if (category == null)
                return Json(new { success = false });

            category.IsActive = !category.IsActive;
            category.CreatedAt = DateTime.Now;
            _context.SaveChanges();

            return Json(new { success = true, newStatus = category.IsActive });
        }

    }

}
