using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechNova.middleware;
using TechNova.Models;
using System.Linq;

[AdminAuthorize]
public class AdminUsersController : Controller
{
    private readonly StoreDbContext _context;

    public AdminUsersController(StoreDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var users = _context.Users.ToList();
        return View("~/Views/Admin/AdminUsers/Index.cshtml", users);
    }

    public IActionResult Create()
    {
        return View("~/Views/Admin/AdminUsers/Create.cshtml");
    }

    [HttpPost]
    public IActionResult Create(User user)
    {
        if (ModelState.IsValid)
        {
            // Mã hóa mật khẩu nếu cần
            user.Password = TechNova.Helpers.PasswordHelper.Hash(user.Password);

            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View("~/Views/Admin/AdminUsers/Create.cshtml", user);
    }

    public IActionResult Edit(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();
        return View("~/Views/Admin/AdminUsers/Edit.cshtml", user);
    }

    [HttpPost]
    public IActionResult Edit(User user)
    {
        if (ModelState.IsValid)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);
            if (existingUser == null) return NotFound();

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            // Nếu có chỉnh sửa mật khẩu thì mã hóa lại
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existingUser.Password = TechNova.Helpers.PasswordHelper.Hash(user.Password);
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View("~/Views/Admin/AdminUsers/Edit.cshtml", user);
    }

    public IActionResult Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}
