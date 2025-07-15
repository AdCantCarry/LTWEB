using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TechNova.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using TechNova.Models.Core;
using TechNova.Models.Auth;
using TechNova.Models.Data;

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
        public IActionResult AddToCart(int productId, string? color, string? storage, int quantity = 1)
        {
            var user = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(user))
                return RedirectToAction("Login", "Account");

            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null) return NotFound();

            double distanceKm = 6; // Giả lập khoảng cách
            var (shippingMethod, shippingFee) = CalculateShipping(product, distanceKm);

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(x =>
                x.ProductId == productId &&
                string.Equals(x.Color ?? "", color ?? "", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Storage ?? "", storage ?? "", StringComparison.OrdinalIgnoreCase));

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
                    Color = string.IsNullOrWhiteSpace(color) ? null : color,
                    Storage = string.IsNullOrWhiteSpace(storage) ? null : storage,
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
        [HttpPost]
        public IActionResult ApplyVoucher(string voucherCode)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new();
            var voucher = _context.Vouchers.FirstOrDefault(v =>
                v.Code == voucherCode &&
                v.IsActive &&
                v.StartDate <= DateTime.Now &&
                v.EndDate >= DateTime.Now &&
                v.Quantity > 0);

            if (voucher == null)
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ" });

            var total = cart.Sum(x => x.TotalPrice);
            if (total < voucher.MinimumOrderAmount)
                return Json(new { success = false, message = $"Đơn hàng phải trên {voucher.MinimumOrderAmount:N0}₫ để áp dụng mã này" });

            decimal discountAmount = voucher.DiscountAmount;
            if (voucher.DiscountPercent.HasValue)
                discountAmount += total * ((decimal)voucher.DiscountPercent.Value / 100);

            HttpContext.Session.SetObjectAsJson("VoucherDiscount", discountAmount);
            HttpContext.Session.SetString("VoucherCode", voucher.Code);

            return Json(new { success = true, discountAmount = discountAmount.ToString("N0") + "₫" });
        }
        public IActionResult Checkout(int? productId)
        {
            var email = HttpContext.Session.GetString("Email");
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return RedirectToAction("Login", "Account");

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new();

            if (productId.HasValue)
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == productId.Value);
                if (product != null)
                {
                    var existing = cart.FirstOrDefault(c => c.ProductId == product.ProductId && c.Color == "" && c.Storage == "");
                    if (existing != null)
                        existing.Quantity += 1;
                    else
                    {
                        var (method, fee) = CalculateShipping(product, 6); // giả lập khoảng cách
                        cart.Add(new CartItem
                        {
                            ProductId = product.ProductId,
                            Name = product.Name,
                            ImageUrl = product.MainImageUrl,
                            Color = "",
                            Storage = "",
                            Quantity = 1,
                            Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price,
                            ShippingMethod = method,
                            ShippingFee = fee
                        });
                    }

                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                }
            }

            var addresses = _context.Addresses.Where(a => a.UserId == user.UserId).ToList();
            var defaultAddr = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();

            var shippingFee = CalculateShippingFee(cart, defaultAddr);
            decimal subtotal = cart.Sum(i => i.TotalPrice) + shippingFee;

            // Áp dụng voucher chính xác
            var voucherCode = HttpContext.Session.GetString("VoucherCode");
            decimal discount = 0;
            if (!string.IsNullOrEmpty(voucherCode))
            {
                (discount, _) = VoucherHelper.Validate(_context, voucherCode, subtotal);
            }

            var viewModel = new Checkout
            {
                CartItems = cart,
                Addresses = addresses,
                ShippingFee = shippingFee,
                Total = subtotal - discount
            };

            ViewBag.DefaultAddress = defaultAddr;
            ViewBag.VoucherCode = voucherCode;
            ViewBag.VoucherDiscount = discount;

            return View(viewModel);
        }
    }
}
