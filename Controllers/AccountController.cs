// Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TechNova.Models;
using TechNova.Helpers;

namespace TechNova.Controllers
{
    public class AccountController : Controller
    {
        private readonly StoreDbContext _context;

        public AccountController(StoreDbContext context)
        {
            _context = context;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
            }

            if (!model.AgreeTerms)
            {
                ModelState.AddModelError("AgreeTerms", "Bạn phải đồng ý với điều khoản.");
            }

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
                HttpContext.Session.SetString("AvatarUrl", string.IsNullOrEmpty(user.AvatarUrl) ? "/images/default-avatar.png" : user.AvatarUrl);

                // Ghi nhớ đăng nhập qua cookie
                if (rememberMe)
                {
                    CookieOptions options = new CookieOptions
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

        public IActionResult Profile()
        {
            var user = GetLoggedInUser();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var addresses = _context.Addresses
                .Where(a => a.UserId == user.UserId)
                .ToList();

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
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
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
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
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
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return RedirectToAction("Login");

            if (avatar != null && avatar.Length > 0)
            {
                var folderPath = Path.Combine("wwwroot", "images", "avatars");
                Directory.CreateDirectory(folderPath);

                if (!string.IsNullOrEmpty(user.AvatarUrl) && !user.AvatarUrl.Contains("default"))
                {
                    var oldPath = Path.Combine("wwwroot", user.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
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
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
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
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
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
            var username = HttpContext.Session.GetString("Username");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
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
