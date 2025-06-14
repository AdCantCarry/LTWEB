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
        public async Task<IActionResult> Store()
        {
            var products = await _context.Products.ToListAsync();
            return View(products); // trả về Views/Home/Store.cshtml
        }

    }
}
