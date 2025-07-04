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

            var currentStatus = order.Status;
            var validTransitions = new Dictionary<string, string>
    {
        { "Chờ xác nhận", "Đang xử lý" },
        { "Đang xử lý", "Đang giao hàng" },
        { "Đang giao hàng", "Hoàn tất" },
    };

            if (!validTransitions.ContainsKey(currentStatus) || validTransitions[currentStatus] != status)
            {
                TempData["Error"] = $"❌ Không thể chuyển từ trạng thái '{currentStatus}' sang '{status}'!";
                return RedirectToAction("Details", new { id = orderId });
            }

            order.Status = status;
            _context.SaveChanges();

            TempData["Success"] = "✅ Cập nhật trạng thái thành công!";
            return RedirectToAction("Index"); // ➤ Trở về danh sách đơn hàng
        }

    }
}
