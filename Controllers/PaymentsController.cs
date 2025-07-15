using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.Models.Core;
using TechNova.Models.Auth;
using TechNova.Models.Data;
using TechNova.Helpers;
using System.Text;
using PaymentModel = TechNova.Models.Core.Payment;

namespace TechNova.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly IConfiguration _config;

        public PaymentsController(StoreDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public IActionResult Index()
        {
            var payments = _context.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o.User)
                .ToList();
            return View(payments);
        }

        [HttpPost]
        public IActionResult ConfirmOrder(IFormCollection form, string PaymentMethod)
        {
            var email = HttpContext.Session.GetString("Email");
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");

            if (user == null || cart == null || !cart.Any())
                return RedirectToAction("Index", "Cart");

            // Lưu địa chỉ giao hàng
            Address shippingAddress;
            if (!string.IsNullOrEmpty(form["SelectedAddressId"]) && int.TryParse(form["SelectedAddressId"], out int selectedId))
            {
                shippingAddress = _context.Addresses.FirstOrDefault(a => a.AddressId == selectedId);
            }
            else
            {
                shippingAddress = new Address
                {
                    UserId = user.UserId,
                    FullName = form["FullName"],
                    Phone = form["Phone"],
                    Street = form["Street"],
                    Ward = form["Ward"],
                    District = form["District"],
                    City = form["City"],
                    IsDefault = true
                };

                // Huỷ mặc định cũ
                var existing = _context.Addresses.Where(a => a.UserId == user.UserId);
                foreach (var addr in existing) addr.IsDefault = false;

                _context.Addresses.Add(shippingAddress);
                _context.SaveChanges();
            }

            // Tính tổng đơn
            var cartTotal = cart.Sum(i => i.TotalPrice);
            var shippingFee = cart.FirstOrDefault()?.ShippingFee ?? 0;
            decimal totalBeforeDiscount = cartTotal + shippingFee;

            // Áp dụng voucher nếu có
            var voucherCode = HttpContext.Session.GetString("VoucherCode");
            decimal discount = 0;
            Voucher? voucher = null;

            if (!string.IsNullOrEmpty(voucherCode))
            {
                (discount, voucher) = VoucherHelper.Validate(_context, voucherCode, totalBeforeDiscount);
            }

            decimal total = totalBeforeDiscount - discount;


            // Tạo đơn hàng
            var order = new Order
            {
                UserId = user.UserId,
                AddressId = shippingAddress.AddressId,
                CreatedAt = DateTime.Now,
                TotalAmount = total,
                Status = PaymentMethod == "COD" ? "Chờ xác nhận" : "Chờ thanh toán",
                OrderItems = cart.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Color = i.Color,
                    Storage = i.Storage
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Tạo thanh toán
            var payment = new PaymentModel
            {
                OrderId = order.OrderId,
                Method = PaymentMethod,
                Amount = total,
                CreatedAt = DateTime.Now,
                Status = PaymentMethod == "COD" ? "Chờ xác nhận" : "Chờ thanh toán",
                IsPaid = false
            };

            _context.Payments.Add(payment);

            // Giảm số lượng voucher nếu hợp lệ
            if (voucher != null)
            {
                voucher.Quantity--;
            }

            _context.SaveChanges();

            // Xử lý phương thức thanh toán
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("VoucherDiscount");
            HttpContext.Session.Remove("VoucherCode");

            if (PaymentMethod == "VNPay")
                return Redirect(GenerateVNPayUrl(order));

            if (PaymentMethod == "Momo")
                return RedirectToAction("PayWithMomo", new { orderId = order.OrderId });

            return RedirectToAction("Success", new { orderId = order.OrderId });
        }
        private (decimal discount, Voucher? voucher) ValidateAndApplyVoucher(string code, decimal total)
        {
            var voucher = _context.Vouchers.FirstOrDefault(v => v.Code == code);
            if (voucher == null || !voucher.IsActive || voucher.Quantity <= 0)
                return (0, null);

            if (DateTime.Now < voucher.StartDate || DateTime.Now > voucher.EndDate)
                return (0, null);

            if (total < voucher.MinimumOrderAmount)
                return (0, null);

            decimal discount = 0;

            if (voucher.DiscountPercent.HasValue)
            {
                var rawPercentDiscount = total * (decimal)voucher.DiscountPercent.Value / 100;
                // ✅ Giới hạn tối đa theo DiscountAmount (nếu có)
                discount = voucher.DiscountAmount > 0 ? Math.Min(rawPercentDiscount, voucher.DiscountAmount) : rawPercentDiscount;
            }
            else
            {
                discount = voucher.DiscountAmount;
            }

            return (discount, voucher);
        }



        [HttpPost]
        public IActionResult PayWithCOD(int addressId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            decimal total = cart.Sum(i => i.Price * i.Quantity);

            var order = new Order
            {
                UserId = userId,
                AddressId = addressId,
                CreatedAt = DateTime.Now,
                TotalAmount = total,
                Status = "Chờ xác nhận",
                OrderItems = cart.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Color = i.Color,
                    Storage = i.Storage
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            var payment = new PaymentModel
            {
                OrderId = order.OrderId,
                Method = "COD",
                Amount = total,
                Status = "Chờ xác nhận",
                IsPaid = false,
                CreatedAt = DateTime.Now
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Success", new { orderId = order.OrderId });
        }

        public IActionResult PayWithMomo(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            var payment = _context.Payments.FirstOrDefault(p => p.OrderId == orderId);

            if (order == null || payment == null)
                return RedirectToAction("Index", "Cart");

            order.Status = "Đã thanh toán";
            payment.Status = "Đã thanh toán";
            payment.IsPaid = true;

            _context.SaveChanges();
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Success", new { orderId = order.OrderId });
        }

        [HttpGet]
        public IActionResult VNPayCallback()
        {
            var query = HttpContext.Request.Query;
            var responseCode = query["vnp_ResponseCode"];
            var txnRef = query["vnp_TxnRef"];

            if (responseCode == "00")
            {
                var order = _context.Orders
                    .Include(o => o.Payment)
                    .AsEnumerable()
                    .FirstOrDefault(o => o.CreatedAt.Ticks.ToString() == txnRef);

                if (order != null)
                {
                    order.Status = "Đã thanh toán";

                    var payment = _context.Payments.FirstOrDefault(p => p.OrderId == order.OrderId);
                    if (payment != null)
                    {
                        payment.Status = "Đã thanh toán";
                        payment.IsPaid = true;
                    }

                    _context.SaveChanges();
                    HttpContext.Session.Remove("Cart");

                    return RedirectToAction("Success", new { orderId = order.OrderId });
                }
            }

            TempData["OrderError"] = "Thanh toán VNPay thất bại hoặc bị hủy.";
            return RedirectToAction("Profile", "Account");
        }

        public IActionResult Success(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return RedirectToAction("Index", "Home");

            return View("~/Views/Cart/Success.cshtml", order);
        }

        private string GenerateVNPayUrl(Order order)
        {
            var vnp = new VNPayLibrary();
            string tmnCode = _config["VNPay:TmnCode"];
            string hashSecret = _config["VNPay:HashSecret"];
            string returnUrl = _config["VNPay:ReturnUrl"];
            string vnpUrl = _config["VNPay:VnpUrl"];

            string txnRef = order.CreatedAt.Ticks.ToString();

            vnp.AddRequestData("vnp_Version", "2.1.0");
            vnp.AddRequestData("vnp_Command", "pay");
            vnp.AddRequestData("vnp_TmnCode", tmnCode);
            vnp.AddRequestData("vnp_Amount", ((long)(order.TotalAmount * 100)).ToString());
            vnp.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnp.AddRequestData("vnp_CurrCode", "VND");
            vnp.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString());
            vnp.AddRequestData("vnp_Locale", "vn");
            vnp.AddRequestData("vnp_OrderInfo", $"Thanh toán đơn hàng #{order.OrderId}");
            vnp.AddRequestData("vnp_OrderType", "other");
            vnp.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnp.AddRequestData("vnp_TxnRef", txnRef);

            return vnp.CreateRequestUrl(vnpUrl, hashSecret);
        }
    }
}
