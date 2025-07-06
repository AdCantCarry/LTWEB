using Microsoft.AspNetCore.Mvc;
using TechNova.Models;
using TechNova.Models.Core;
using TechNova.Models.Data;

public class AdminNewsController : Controller
{
    private readonly StoreDbContext _context;

    public AdminNewsController(StoreDbContext context)
    {
        _context = context;
    }

    // Hiển thị danh sách tin tức
    public IActionResult Index()
    {
        var newsList = _context.News.OrderByDescending(n => n.CreatedAt).ToList();
        return View("~/Views/Admin/AdminNews/Index.cshtml", newsList);
    }

    // Hiển thị form tạo mới
    public IActionResult Create()
    {
        return View("~/Views/Admin/AdminNews/Create.cshtml");
    }

    [HttpPost]
    public IActionResult Create(News news)
    {
        if (ModelState.IsValid)
        {
            news.CreatedAt = DateTime.Now;
            _context.News.Add(news);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Cập nhật tin tức thành công!";
            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu tin tức!";
            return RedirectToAction("Index");
        }
        return View("~/Views/Admin/AdminNews/Create.cshtml", news);
    }

    // Hiển thị form chỉnh sửa
    public IActionResult Edit(int id)
    {
        var news = _context.News.Find(id);
        if (news == null) return NotFound();
        return View("~/Views/Admin/AdminNews/Edit.cshtml", news);
    }

    [HttpPost]
    public IActionResult Edit(News news)
    {
        if (ModelState.IsValid)
        {
            _context.News.Update(news);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Cập nhật tin tức thành công!";
            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lưu tin tức!";
            return RedirectToAction("Index");
        }
        return View("~/Views/Admin/AdminNews/Edit.cshtml", news);
    }

    // Xóa tin tức
    public IActionResult Delete(int id)
    {
        var news = _context.News.Find(id);
        if (news == null) return NotFound();
        _context.News.Remove(news);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}