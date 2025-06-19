using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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
            return View(products);
        }
        public IActionResult Store()
        {
            var products = _context.Products.Include(p => p.Category).ToList();
            ViewBag.Categories = _context.Categories.ToList();
            return View(products);
        }
        public IActionResult iPhone()
        {
            var products = _context.Products.Include(p => p.Category).ToList();
            return View(products);
        }
        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            return View(product);
        }

    }
}
