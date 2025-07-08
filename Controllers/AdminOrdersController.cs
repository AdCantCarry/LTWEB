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
            var order = _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null) return NotFound();

            var currentStatus = order.Status;

            // Chuyển trạng thái hợp lệ
            var validTransitions = new Dictionary<string, List<string>>
    {
        { "Chờ xác nhận", new List<string> { "Đang xử lý" } },
        { "Đang xử lý", new List<string> { "Đang giao hàng", "Đã thanh toán" } },
        { "Đang giao hàng", new List<string> { "Đã giao hàng" } }
    };

            // Kiểm tra trạng thái hợp lệ
            if (!validTransitions.ContainsKey(currentStatus) || !validTransitions[currentStatus].Contains(status))
            {
                TempData["Error"] = $"❌ Không thể chuyển từ trạng thái '{currentStatus}' sang '{status}'!";
                return RedirectToAction("Details", new { id = orderId });
            }

            // Đảm bảo "Đã thanh toán" chỉ áp dụng cho VNPay
            if (status == "Đã thanh toán" && order.Payment?.Method != "VNPay")
            {
                TempData["Error"] = "❌ Trạng thái 'Đã thanh toán' chỉ áp dụng cho đơn thanh toán bằng VNPay!";
                return RedirectToAction("Details", new { id = orderId });
            }

            // Cập nhật trạng thái đơn hàng
            order.Status = status;

            // Nếu trạng thái là Đã thanh toán → cập nhật trạng thái thanh toán
            if (status == "Đã thanh toán")
            {
                if (order.Payment != null)
                {
                    order.Payment.Status = "Đã thanh toán";
                    order.Payment.IsPaid = true;
                }
            }

            _context.SaveChanges();

            TempData["Success"] = "✅ Cập nhật trạng thái thành công!";
            return RedirectToAction("Index");
        }




    }
}
