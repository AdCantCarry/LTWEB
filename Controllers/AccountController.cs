using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TechNova.Models;
using TechNova.Helpers;
using TechNova.ViewModels;

namespace TechNova.Controllers
{
    public class AccountController : Controller
    {
        private readonly StoreDbContext _context;
        private readonly EmailService _emailService;

        public AccountController(StoreDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ĐĂNG KÝ
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
                ModelState.AddModelError("Email", "Email đã được sử dụng.");

            if (!model.AgreeTerms)
                ModelState.AddModelError("AgreeTerms", "Bạn phải đồng ý với điều khoản.");

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = model.FirstName + " " + model.LastName,
                    Email = model.Email,
                    Password = PasswordHelper.Hash(model.Password),
                    Role = "User",
                    AvatarUrl = "/images/default.jpg",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // ĐĂNG NHẬP
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password, bool rememberMe)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null && PasswordHelper.Verify(password, user.Password))
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("Role", user.Role ?? "User");
                HttpContext.Session.SetString("AvatarUrl", user.AvatarUrl ?? "/images/default-avatar.png");

                if (rememberMe)
                {
                    var options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7),
                        IsEssential = true,
                        HttpOnly = true
                    };
                    Response.Cookies.Append("RememberMe_Email", email, options);
                }

                return user.Role == "Admin"
                    ? RedirectToAction("Index", "AdminProducts", new { area = "Admin" })
                    : RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Sai email hoặc mật khẩu";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // QUÊN MẬT KHẨU
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email không tồn tại.");
                return View(model);
            }

            // Tạo mã 6 chữ số
            var code = new Random().Next(100000, 999999).ToString();
            user.ResetCode = code;
            user.ResetCodeExpiry = DateTime.Now.AddMinutes(10);
            _context.SaveChanges();

            // Gửi email
            await _emailService.SendEmailAsync(user.Email,
                "Mã xác minh đặt lại mật khẩu - TechNova",
                $"<p>Mã xác minh của bạn là: <strong>{code}</strong></p><p>Mã có hiệu lực trong 10 phút.</p>"
            );

            TempData["Email"] = model.Email; // để giữ lại email sau POST
            return RedirectToAction("VerifyCode");
        }
        [HttpGet]
        public IActionResult VerifyCode()
        {
            var email = TempData["Email"] as string;
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            return View(new VerifyCodeViewModel { Email = email });
        }

        [HttpPost]
        public IActionResult VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null || user.ResetCode != model.Code || user.ResetCodeExpiry < DateTime.Now)
            {
                ModelState.AddModelError("", "Mã không hợp lệ hoặc đã hết hạn.");
                return View(model);
            }

            TempData["VerifiedEmail"] = model.Email;
            return RedirectToAction("ResetPassword");
        }


        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = TempData["VerifiedEmail"] as string;
            if (string.IsNullOrEmpty(email)) return RedirectToAction("ForgotPassword");

            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null) return RedirectToAction("ForgotPassword");

            user.Password = PasswordHelper.Hash(model.NewPassword);
            user.ResetCode = null;
            user.ResetCodeExpiry = null;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công. Mời đăng nhập.";
            return RedirectToAction("Login");
        }


        // TRANG PROFILE
        public IActionResult Profile()
        {
            var user = GetLoggedInUser();
            if (user == null) return RedirectToAction("Login");

            var addresses = _context.Addresses.Where(a => a.UserId == user.UserId).ToList();
            var orders = _context.Orders
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.OrderId,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    PaymentMethod = o.Payment.Method,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(i => new OrderItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        Price = i.Price,
                        Quantity = i.Quantity,
                        Image = i.Product.MainImageUrl
                    }).ToList()
                }).ToList();

            var viewModel = new UserProfileViewModel
            {
                User = user,
                Addresses = addresses,
                Orders = orders
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(IFormCollection form, IFormFile avatarInput)
        {
            var user = GetLoggedInUser();
            if (user == null) return RedirectToAction("Login");

            user.Username = form["Username"];
            user.PhoneNumber = form["PhoneNumber"];
            if (DateTime.TryParse(form["BirthDate"], out var birthDate))
                user.BirthDate = birthDate;

            if (avatarInput != null && avatarInput.Length > 0)
            {
                var folderPath = Path.Combine("wwwroot", "images", "avatars");
                Directory.CreateDirectory(folderPath);

                if (!string.IsNullOrEmpty(user.AvatarUrl) && !user.AvatarUrl.Contains("default"))
                {
                    var oldPath = Path.Combine("wwwroot", user.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(avatarInput.FileName);
                var filePath = Path.Combine(folderPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await avatarInput.CopyToAsync(stream);

                user.AvatarUrl = "/images/avatars/" + fileName;
                HttpContext.Session.SetString("AvatarUrl", user.AvatarUrl);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            TempData["AvatarUpdated"] = true;

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatar)
        {
            var user = GetLoggedInUser();
            if (user == null) return RedirectToAction("Login");

            if (avatar != null && avatar.Length > 0)
            {
                var folderPath = Path.Combine("wwwroot", "images", "avatars");
                Directory.CreateDirectory(folderPath);

                if (!string.IsNullOrEmpty(user.AvatarUrl) && !user.AvatarUrl.Contains("default"))
                {
                    var oldPath = Path.Combine("wwwroot", user.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(avatar.FileName);
                var filePath = Path.Combine(folderPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await avatar.CopyToAsync(stream);

                user.AvatarUrl = "/images/avatars/" + fileName;
                HttpContext.Session.SetString("AvatarUrl", user.AvatarUrl);
                TempData["AvatarUpdated"] = true;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var user = GetLoggedInUser();
            if (user == null) return RedirectToAction("Login");

            if (!PasswordHelper.Verify(CurrentPassword, user.Password))
            {
                TempData["PasswordError"] = "Mật khẩu hiện tại không đúng.";
                return RedirectToAction("Profile");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["PasswordError"] = "Mật khẩu mới không khớp.";
                return RedirectToAction("Profile");
            }

            user.Password = PasswordHelper.Hash(NewPassword);
            _context.Users.Update(user);
            _context.SaveChanges();

            TempData["PasswordSuccess"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult AddAddress(Address address)
        {
            var user = GetLoggedInUser();
            if (user == null)
                return Json(new { success = false });

            address.UserId = user.UserId;

            if (address.IsDefault)
            {
                var others = _context.Addresses.Where(a => a.UserId == user.UserId && a.IsDefault);
                foreach (var a in others) a.IsDefault = false;
            }

            _context.Addresses.Add(address);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                address = new
                {
                    fullName = address.FullName,
                    phone = address.Phone,
                    street = address.Street,
                    ward = address.Ward,
                    district = address.District,
                    city = address.City,
                    isDefault = address.IsDefault
                }
            });
        }

        [HttpPost]
        public IActionResult DeleteAddress(int id)
        {
            var user = GetLoggedInUser();
            if (user == null) return RedirectToAction("Login");

            var addr = _context.Addresses.FirstOrDefault(a => a.AddressId == id && a.UserId == user.UserId);
            if (addr != null)
            {
                _context.Addresses.Remove(addr);
                _context.SaveChanges();
            }

            return RedirectToAction("Profile");
        }

        private User GetLoggedInUser()
        {
            var username = HttpContext.Session.GetString("Username");
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }
    }
}
