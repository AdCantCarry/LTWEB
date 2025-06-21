using Microsoft.AspNetCore.Mvc;
using TechNova.Models;
using System.Linq;

namespace TechNova.Controllers
{

    public class AdminBrandsController : Controller
    {
        private readonly StoreDbContext _context;

        public AdminBrandsController(StoreDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var brands = _context.Brands.ToList();
            return View("~/Views/Admin/AdminBrands/Index.cshtml", brands);
        }

        public IActionResult Create()
        {
            return View("~/Views/Admin/AdminBrands/Create.cshtml");
        }

        [HttpPost]
        public IActionResult Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
                _context.Brands.Add(brand);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("~/Views/Admin/AdminBrands/Create.cshtml", brand);
        }

        public IActionResult Edit(int id)
        {
            var brand = _context.Brands.Find(id);
            if (brand == null) return NotFound();

            return View("~/Views/Admin/AdminBrands/Edit.cshtml", brand);
        }

        [HttpPost]
        public IActionResult Edit(Brand brand)
        {
            if (ModelState.IsValid)
            {
                var existing = _context.Brands.Find(brand.BrandId);
                if (existing == null) return NotFound();

                existing.Name = brand.Name;
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View("~/Views/Admin/AdminBrands/Edit.cshtml", brand);
        }
    }
}
