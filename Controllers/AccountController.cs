using Microsoft.AspNetCore.Mvc;
using TechNova.Models;
using TechNova.Helpers; // dùng để mã hóa
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace TechNova.Controllers
{
    public class AccountController : Controller
    {
        private readonly StoreDbContext _context;

        public AccountController(StoreDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                user.Password = PasswordHelper.Hash(user.Password);
                user.Role ??= "User"; // Gán role mặc định là User nếu không được set

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: /Account/Login
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            string hashedPassword = PasswordHelper.Hash(password);

            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == hashedPassword);
            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role ?? "User");

                if (user.Role == "Admin")
                    return RedirectToAction("Index", "AdminProducts", new { area = "Admin" });

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Sai email hoặc mật khẩu";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
