using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Models.Data;

namespace TechNova.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly StoreDbContext _context;

        public AdminDashboardController(StoreDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult RevenueData()
        {
            var completedStatus = "Hoàn tất";

            var revenueData = _context.Orders
                .Where(o => o.Status.Trim().ToLower() == completedStatus.ToLower())
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .AsEnumerable() // chuyển sang LINQ in-memory
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month.ToString("D2")}",
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToList(); // ✅ dùng ToList thay vì ToListAsync

            return Json(revenueData);
        }


        [HttpGet]
        public IActionResult OrderCountData()
        {
            var completedStatus = "Hoàn tất";

            var orderCounts = _context.Orders
                .Where(o => o.Status.Trim().ToLower() == completedStatus.ToLower())
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .AsEnumerable()
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month.ToString("D2")}",
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            return Json(orderCounts);
        }


        public IActionResult Index()
        {
            return View("~/Views/Admin/AdminDashboard/Index.cshtml");
        }
    }

}
