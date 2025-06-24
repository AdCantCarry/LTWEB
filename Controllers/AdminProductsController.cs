using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechNova.middleware;
using TechNova.Models;
using System.Linq;
using X.PagedList;
using X.PagedList.Extensions;

[AdminAuthorize]
public class AdminProductsController : Controller
{
    private readonly StoreDbContext _context;

    public AdminProductsController(StoreDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(int? categoryId, string search, string status, int page = 1)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search));

        if (!string.IsNullOrEmpty(status))
        {
            bool isActive = status == "true";
            query = query.Where(p => p.IsActive == isActive);
        }

        int pageSize = 10;
        var pagedProducts = query.OrderByDescending(p => p.ProductId).ToPagedList(page, pageSize);

        ViewBag.Categories = _context.Categories.ToList();
        ViewBag.CategoryId = categoryId;
        ViewBag.Search = search;
        ViewBag.Status = status;

        return View("~/Views/Admin/AdminProducts/Index.cshtml", pagedProducts);
    }




    public IActionResult Create()
    {
        ViewBag.CategoryList = new SelectList(
            _context.Categories.Where(c => c.IsActive), "CategoryId", "Name"
        );
        ViewBag.BrandList = new SelectList(
            _context.Brands, "BrandId", "Name"
        );

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
        if (product.StockQuantity == 0)
            product.IsActive = false;

        ViewBag.CategoryList = new SelectList(_context.Categories.Where(c => c.IsActive), "CategoryId", "Name", product.CategoryId);
        ViewBag.BrandList = new SelectList(_context.Brands, "BrandId", "Name", product.BrandId);

        return View("~/Views/Admin/AdminProducts/Create.cshtml", product);
    }


    public IActionResult Edit(int id)
    {
        var product = _context.Products.Find(id);
        if (product == null) return NotFound();

        ViewBag.CategoryList = new SelectList(_context.Categories.Where(c => c.IsActive), "CategoryId", "Name", product.CategoryId);
        ViewBag.BrandList = new SelectList(_context.Brands, "BrandId", "Name", product.BrandId);

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

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.DiscountPercent = product.DiscountPercent;
            existing.CategoryId = product.CategoryId;
            existing.BrandId = product.BrandId;
            existing.MainImageUrl = product.MainImageUrl;
            existing.SubImage1Url = product.SubImage1Url;
            existing.SubImage2Url = product.SubImage2Url;
            existing.SubImage3Url = product.SubImage3Url;
            existing.Color = product.Color;
            existing.Storage = product.Storage;
            existing.StockQuantity = product.StockQuantity;
            existing.IsActive = product.IsActive; // mặc định từ form

            if (existing.StockQuantity == 0)
                existing.IsActive = false; // tự động ngừng bán nếu hết hàng

            existing.UpdatedAt = DateTime.Now;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // load lại list
        ViewBag.CategoryList = new SelectList(_context.Categories.Where(c => c.IsActive), "CategoryId", "Name", product.CategoryId);
        ViewBag.BrandList = new SelectList(_context.Brands, "BrandId", "Name", product.BrandId);

        return View("~/Views/Admin/AdminProducts/Edit.cshtml", product);
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

    /// <summary>
    /// Bổ sung "/images/" vào đầu đường dẫn ảnh nếu thiếu
    /// </summary>
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
