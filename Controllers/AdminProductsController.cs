using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechNova.middleware;
using TechNova.Models;
using System.Linq;

[AdminAuthorize]
public class AdminProductsController : Controller
{
    private readonly StoreDbContext _context;

    public AdminProductsController(StoreDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View("~/Views/Admin/AdminProducts/Index.cshtml", _context.Products.ToList());
    }

    public IActionResult Create()
    {
        ViewBag.CategoryList = new SelectList(_context.Categories.Where(c => c.IsActive), "CategoryId", "Name");
        return View("~/Views/Admin/AdminProducts/Create.cshtml");
    }

    [HttpPost]
    public IActionResult Create(Product product)
    {
        if (ModelState.IsValid)
        {
            NormalizeImagePaths(product);
            product.CreatedAt = DateTime.Now;

            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryId", "Name");
        return View("~/Views/Admin/AdminProducts/Create.cshtml", product);
    }

    public IActionResult Edit(int id)
    {
        var product = _context.Products.Find(id);
        if (product == null) return NotFound();

        ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
        return View("~/Views/Admin/AdminProducts/Edit.cshtml", product);
    }

    [HttpPost]
    public IActionResult Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            NormalizeImagePaths(product);

            var existing = _context.Products.Find(product.ProductId);
            if (existing == null) return NotFound();

            // Cập nhật giá trị
            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.DiscountPercent = product.DiscountPercent;
            existing.CategoryId = product.CategoryId;
            existing.MainImageUrl = product.MainImageUrl;
            existing.SubImage1Url = product.SubImage1Url;
            existing.SubImage2Url = product.SubImage2Url;
            existing.SubImage3Url = product.SubImage3Url;
            existing.UpdatedAt = DateTime.Now;
            // ❗❗ Thêm 2 dòng này:
            existing.Color = product.Color;
            existing.Storage = product.Storage;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryId", "Name");
        return View("~/Views/Admin/AdminProducts/Create.cshtml", product);
    }

    public IActionResult Delete(int id)
    {
        var product = _context.Products.Find(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    // ✳️ Hàm tiện ích thêm prefix /images/ nếu thiếu
    private void NormalizeImagePaths(Product product)
    {
        string prefix = "/images/";

        if (!string.IsNullOrWhiteSpace(product.MainImageUrl) && !product.MainImageUrl.StartsWith(prefix))
            product.MainImageUrl = prefix + product.MainImageUrl;

        if (!string.IsNullOrWhiteSpace(product.SubImage1Url) && !product.SubImage1Url.StartsWith(prefix))
            product.SubImage1Url = prefix + product.SubImage1Url;

        if (!string.IsNullOrWhiteSpace(product.SubImage2Url) && !product.SubImage2Url.StartsWith(prefix))
            product.SubImage2Url = prefix + product.SubImage2Url;

        if (!string.IsNullOrWhiteSpace(product.SubImage3Url) && !product.SubImage3Url.StartsWith(prefix))
            product.SubImage3Url = prefix + product.SubImage3Url;
    }
}
