using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TechNova.Helpers;
using TechNova.Models;
using TechNova.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace TechNova.Controllers
{
    public class CartController : Controller
    {
        private readonly StoreDbContext _context;

        public CartController(StoreDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, string color, string storage, int quantity = 1)
        {
            var user = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(user))
                return RedirectToAction("Login", "Account");

            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null) return NotFound();

            double distanceKm = 6; // Giả lập khoảng cách
            var (shippingMethod, shippingFee) = CalculateShipping(product, distanceKm);

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(x => x.ProductId == productId && x.Color == color && x.Storage == storage);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    ImageUrl = product.MainImageUrl,
                    Color = color,
                    Storage = storage,
                    Quantity = quantity,
                    Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price,
                    ShippingMethod = shippingMethod,
                    ShippingFee = shippingFee
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult AddToCartAjax(int productId, string color = "", string storage = "", int quantity = 1)
        {
            var user = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(user))
                return Json(new { success = false, message = "Bạn chưa đăng nhập" });

            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

            double distanceKm = 6;
            var (shippingMethod, shippingFee) = CalculateShipping(product, distanceKm);

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(x => x.ProductId == productId && x.Color == color && x.Storage == storage);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    ImageUrl = product.MainImageUrl,
                    Color = color,
                    Storage = storage,
                    Quantity = quantity,
                    Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price,
                    ShippingMethod = shippingMethod,
                    ShippingFee = shippingFee
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Json(new { success = true, cartCount = cart.Sum(x => x.Quantity) });
        }

        [HttpPost]
        public IActionResult Remove(int productId, string color, string storage)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            cart.RemoveAll(x =>
                x.ProductId == productId &&
                (x.Color ?? "") == (color ?? "") &&
                (x.Storage ?? "") == (storage ?? "")
            );

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }


        private (string method, decimal fee) CalculateShipping(Product product, double distanceKm)
        {
            if (product.Price >= 1_000_000)
            {
                return ("Miễn phí vận chuyển (giá > 1 triệu)", 0);
            }
            else if (distanceKm < 5)
            {
                return ("Miễn phí vận chuyển (< 5km)", 0);
            }
            else if (distanceKm <= 10)
            {
                return ("Phí vận chuyển (5-10km)", 30000);
            }
            else
            {
                return ("Không hỗ trợ giao hàng xa hơn 10km", -1);
            }
        }

        private decimal CalculateShippingFee(List<CartItem> cart, Address addr)
        {
            var total = cart.Sum(i => i.TotalPrice);
            if (total > 1_000_000) return 0;
            double distance = 6; // giả lập
            if (distance < 5) return 0;
            else if (distance <= 10) return 30000;
            return 50000;
        }

        public IActionResult Checkout()
        {
            var email = HttpContext.Session.GetString("Email");
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return RedirectToAction("Login", "Account");

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new();

            var addresses = _context.Addresses.Where(a => a.UserId == user.UserId).ToList();
            var defaultAddr = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();

            var shippingFee = CalculateShippingFee(cart, defaultAddr);

            var viewModel = new Checkout
            {
                CartItems = cart,
                Addresses = addresses,
                ShippingFee = shippingFee,
                Total = cart.Sum(i => i.TotalPrice) + shippingFee
            };

            ViewBag.DefaultAddress = defaultAddr; // 👈 TRUYỀN VÀO ĐÂY

            return View(viewModel);
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
                Status = "Pending",
                OrderItems = cart.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Tạo Payment tương ứng với đơn hàng (COD)
            var payment = new Payment
            {
                OrderId = order.OrderId,
                Method = PaymentMethod,
                Amount = order.TotalAmount,
                Status = "Chưa thanh toán", // Vì COD
                CreatedAt = DateTime.Now
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Success", new { orderId = order.OrderId });
        }
        public IActionResult Success(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
                return RedirectToAction("Index", "Home");

            return View(order);
        }

    }
}
