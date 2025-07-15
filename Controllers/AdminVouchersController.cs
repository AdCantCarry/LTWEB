using Microsoft.AspNetCore.Mvc;
using TechNova.Models.Core;
using TechNova.Models.Data;
using System.Linq;

namespace TechNova.Controllers
{
    public class AdminVouchersController : Controller
    {
        private readonly StoreDbContext _context;

        public AdminVouchersController(StoreDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var now = DateTime.Now;
            var vouchers = _context.Vouchers.ToList();

            foreach (var v in vouchers)
            {
                bool shouldBeInactive = v.Quantity <= 0 || v.EndDate < now;
                if (v.IsActive && shouldBeInactive)
                {
                    v.IsActive = false;
                }
                else if (!v.IsActive && v.Quantity > 0 && v.EndDate >= now)
                {
                    v.IsActive = true;
                }
            }

            _context.SaveChanges();

            return View("~/Views/Admin/AdminVouchers/Index.cshtml", vouchers.OrderByDescending(v => v.VoucherId));
        }


        public IActionResult Create()
        {
            return View("~/Views/Admin/AdminVouchers/Create.cshtml");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Voucher voucher)
        {
            if (_context.Vouchers.Any(v => v.Code == voucher.Code))
            {
                TempData["ErrorMessage"] = "Mã giảm giá đã tồn tại. Vui lòng chọn mã khác.";
                return View("~/Views/Admin/AdminVouchers/Create.cshtml", voucher);
            }

            if (voucher.DiscountPercent < 0 || voucher.DiscountPercent > 100)
            {
                TempData["ErrorMessage"] = "Phần trăm giảm giá chỉ nằm trong khoảng 0 đến 100%.";
                return View("~/Views/Admin/AdminVouchers/Create.cshtml", voucher);
            }

            if (voucher.DiscountAmount < 0 || voucher.MinimumOrderAmount < 0)
            {
                TempData["ErrorMessage"] = "Số tiền giảm hoặc đơn hàng tối thiểu không được âm.";
                return View("~/Views/Admin/AdminVouchers/Create.cshtml", voucher);
            }

            if (voucher.EndDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Ngày hết hạn phải sau ngày hiện tại.";
                return View("~/Views/Admin/AdminVouchers/Create.cshtml", voucher);
            }

            if (ModelState.IsValid)
            {
                _context.Vouchers.Add(voucher);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Tạo voucher thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            return View("~/Views/Admin/AdminVouchers/Create.cshtml", voucher);
        }



        public IActionResult Edit(int id)
        {
            var voucher = _context.Vouchers.Find(id);
            if (voucher == null) return NotFound();
            return View("~/Views/Admin/AdminVouchers/Edit.cshtml", voucher);
        }


        [HttpPost]
        public IActionResult Edit(Voucher voucher)
        {
            if (_context.Vouchers.Any(v => v.Code == voucher.Code && v.VoucherId != voucher.VoucherId))
            {
                TempData["ErrorMessage"] = "Mã giảm giá đã tồn tại. Vui lòng chọn mã khác.";
                return View("~/Views/Admin/AdminVouchers/Edit.cshtml", voucher);
            }
            if (voucher.DiscountPercent < 0 || voucher.DiscountPercent > 100)
            {
                TempData["ErrorMessage"] = "Phần trăm giảm giá chỉ nằm trong khoảng 0 đến 100%.";
                return View("~/Views/Admin/AdminVouchers/Create.cshtml", voucher);
            }

            if (voucher.DiscountAmount < 0 || voucher.MinimumOrderAmount < 0)
            {
                TempData["ErrorMessage"] = "Số tiền giảm hoặc đơn hàng tối thiểu không được âm.";
                return View("~/Views/Admin/AdminVouchers/Create.cshtml", voucher);
            }

            if (voucher.EndDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Ngày hết hạn phải sau ngày hiện tại.";
                return View("~/Views/Admin/AdminVouchers/Create.cshtml", voucher);
            }

            if (ModelState.IsValid)
            {
                _context.Vouchers.Update(voucher);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Tạo voucher thành công!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            return View("~/Views/Admin/AdminVouchers/Create.cshtml", voucher);
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            var voucher = _context.Vouchers.Find(id);
            if (voucher == null) return NotFound();

            _context.Vouchers.Remove(voucher);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
