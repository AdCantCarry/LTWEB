using Microsoft.AspNetCore.Mvc;
using TechNova.Models;
using Microsoft.EntityFrameworkCore;

namespace TechNova.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly StoreDbContext _context;

        public PaymentsController(StoreDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var payments = _context.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o.User)
                .ToList();
            return View(payments);
        }

        public IActionResult PayWithMomo(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            var payment = _context.Payments.FirstOrDefault(p => p.OrderId == orderId);

            if (order == null || payment == null)
                return RedirectToAction("Index", "Cart");

            // Redirect giả lập (không tích hợp API thật)
            TempData["OrderSuccess"] = "Giả lập thanh toán Momo thành công!";
            order.Status = "Đã thanh toán";
            payment.Status = "Paid";
            _context.SaveChanges();

            return RedirectToAction("Profile", "Account");
        }

        [HttpPost]
        public IActionResult ConfirmOrder(int SelectedAddressId, string PaymentMethod)
        {
            var email = HttpContext.Session.GetString("Email");
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");

            if (user == null || cart == null || !cart.Any())
                return RedirectToAction("Index", "Cart");

            var order = new Order
            {
                UserId = user.UserId,
                AddressId = SelectedAddressId,
                CreatedAt = DateTime.Now,
                TotalAmount = cart.Sum(i => i.TotalPrice),
                Status = PaymentMethod == "Momo" ? "Chờ thanh toán" : "Pending",
                OrderItems = cart.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            var payment = new Payment
            {
                OrderId = order.OrderId,
                Method = PaymentMethod,
                Amount = order.TotalAmount,
                Status = PaymentMethod == "Momo" ? "Pending" : "Paid"
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            if (PaymentMethod == "Momo")
            {
                // Chuyển qua hành động gọi Momo
                return RedirectToAction("PayWithMomo", "Payment", new { orderId = order.OrderId });
            }

            HttpContext.Session.Remove("Cart");
            TempData["OrderSuccess"] = "Đặt hàng thành công!";
            return RedirectToAction("Profile", "Account");
        }

    }

}
