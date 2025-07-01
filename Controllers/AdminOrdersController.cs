using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.middleware;
using TechNova.Models.Data;

namespace TechNova.Controllers
{
    [AdminAuthorize]
    public class AdminOrdersController : Controller
    {
        private readonly StoreDbContext _context;

        public AdminOrdersController(StoreDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .ToList();
            return View("~/Views/Admin/AdminOrders/Index.cshtml", orders);
        }

        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.Address) // 👈 THÊM DÒNG NÀY
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
                return NotFound();

           return View("~/Views/Admin/AdminOrders/Details.cshtml", order);
        }


        [HttpPost]
        public IActionResult UpdateStatus(int orderId, string status)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = orderId });
        }
    }
}
