using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TechNova.Helpers;
using TechNova.Models.Auth;
using TechNova.Models.Core;
using TechNova.Models.Data;
using TechNova.Models.ViewModels;

namespace TechNova.Controllers
{
    public class VouchersController : Controller
    {
        private readonly StoreDbContext _context;

        public VouchersController(StoreDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Validate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { success = false, message = "Vui lòng nhập mã." });

            var voucher = _context.Vouchers.FirstOrDefault(v =>
                v.Code.ToLower() == code.ToLower() &&
                v.IsActive &&
                v.Quantity > 0 &&
                v.StartDate <= DateTime.Now &&
                v.EndDate >= DateTime.Now
            );

            if (voucher == null)
                return Json(new { success = false, message = "Mã không hợp lệ hoặc đã hết hạn." });

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
                return Json(new { success = false, message = "Giỏ hàng trống." });

            var cartTotal = cart.Sum(i => i.TotalPrice);

            if (cartTotal < voucher.MinimumOrderAmount)
            {
                return Json(new
                {
                    success = false,
                    message = $"Đơn hàng phải từ {voucher.MinimumOrderAmount:N0}₫ mới dùng được mã này."
                });
            }

            decimal discount = 0;
            if (voucher.DiscountPercent.HasValue && voucher.DiscountPercent > 0)
            {
                discount = cartTotal * ((decimal)voucher.DiscountPercent.Value / 100);
            }
            else
            {
                discount = voucher.DiscountAmount;
            }

            // Lưu mã voucher vào session (áp dụng cho bước thanh toán)
            HttpContext.Session.SetString("AppliedVoucherCode", voucher.Code);
            HttpContext.Session.SetString("AppliedVoucherDiscount", discount.ToString());

            var shippingFee = HttpContext.Session.GetObjectFromJson<decimal?>("ShippingFee") ?? 0m;
            var newTotal = cartTotal + shippingFee - discount;

            return Json(new
            {
                success = true,
                discountAmount = (int)discount,
                newTotal = (int)newTotal
            });
        }
    }
}
