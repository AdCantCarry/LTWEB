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
        public IActionResult SummaryStats()
        {
            try
            {
                var completedStatus = "Đã thanh toán";

                var totalRevenue = _context.Orders
                    .Where(o => o.Status.ToLower().Trim() == completedStatus.ToLower())
                    .Sum(o => o.TotalAmount);

                var totalSold = _context.OrderItems
                    .Where(oi => oi.Order.Status.ToLower().Trim() == completedStatus.ToLower())
                    .Sum(oi => oi.Quantity);

                var totalStock = _context.Products.Sum(p => p.StockQuantity);
                var productCount = _context.Products.Count();

                return Json(new
                {
                    totalRevenue,
                    totalSold,
                    totalStock,
                    productCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }


        [HttpGet]
        public IActionResult RevenueData()
        {
            var completedStatus = "Đã thanh toán";

            var revenueData = _context.Orders
                .Where(o => o.Status.Trim().ToLower() == completedStatus.ToLower())
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .AsEnumerable() // 👉 chuyển sang LINQ in-memory để dùng string.Format
                .Select(g => new
                {
                    Month = string.Format("{0}-{1:D2}", g.Key.Year, g.Key.Month),
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToList();

            return Json(revenueData);
        }


        [HttpGet]
        public IActionResult OrderCountData()
        {
            var completedStatus = "Đã thanh toán";

            var orderCounts = _context.Orders
                .Where(o => o.Status.Trim().ToLower() == completedStatus.ToLower())
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .AsEnumerable()
                .Select(g => new
                {
                    Month = string.Format("{0}-{1:D2}", g.Key.Year, g.Key.Month),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            return Json(orderCounts);
        }



        [HttpGet]
        public IActionResult RevenueByBrand()
        {
            var completed = "đã thanh toán";

            var data = _context.OrderItems
                .Where(oi => oi.Order.Status.ToLower() == completed)
                .GroupBy(oi => oi.Product.Brand!.Name)
                .Select(g => new {
                    Brand = g.Key ?? "Không rõ",
                    Revenue = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            return Json(data);
        }


        public IActionResult Index()
        {
            return View("~/Views/Admin/AdminDashboard/Index.cshtml");
        }
    }

}
