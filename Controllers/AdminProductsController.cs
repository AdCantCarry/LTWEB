using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TechNova.middleware;
using TechNova.Models.Core;
using TechNova.Models.Data;
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
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search));

        if (!string.IsNullOrEmpty(status))
        {
            bool isActive = status == "true";
            query = query.Where(p => p.IsActive == isActive);
        }

        var paged = query.OrderByDescending(p => p.ProductId).ToPagedList(page, 10);
        ViewBag.Categories = _context.Categories.ToList();
        ViewBag.CategoryId = categoryId;
        ViewBag.Search = search;
        ViewBag.Status = status;

        return View("~/Views/Admin/AdminProducts/Index.cshtml", paged);
    }

    public IActionResult Create()
    {
        ViewBag.CategoryList = new SelectList(_context.Categories.Where(c => c.IsActive), "CategoryId", "Name");
        ViewBag.BrandList = new SelectList(_context.Brands, "BrandId", "Name");
        return View("~/Views/Admin/AdminProducts/Create.cshtml");
    }

    [HttpPost]
    public IActionResult Create(Product product, IFormFile MainImageFile, List<IFormFile> SubImageFiles)
    {
        if (ModelState.IsValid)
        {
            string uploadsFolder = Path.Combine("wwwroot", "images");

            if (MainImageFile != null && MainImageFile.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(MainImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    MainImageFile.CopyTo(stream);
                }
                product.MainImageUrl = "/images/" + fileName;
            }

            for (int i = 0; i < Math.Min(3, SubImageFiles.Count); i++)
            {
                if (SubImageFiles[i] != null && SubImageFiles[i].Length > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(SubImageFiles[i].FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        SubImageFiles[i].CopyTo(stream);
                    }
                    string url = "/images/" + fileName;
                    switch (i)
                    {
                        case 0: product.SubImage1Url = url; break;
                        case 1: product.SubImage2Url = url; break;
                        case 2: product.SubImage3Url = url; break;
                    }
                }
            }

            product.CreatedAt = DateTime.Now;
            product.IsActive = product.StockQuantity > 0;

            _context.Products.Add(product);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đã thêm sản phẩm!";
            return RedirectToAction("Index");
        }

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
        if (!ModelState.IsValid)
        {
            ViewBag.CategoryList = new SelectList(_context.Categories.Where(c => c.IsActive), "CategoryId", "Name", product.CategoryId);
            ViewBag.BrandList = new SelectList(_context.Brands, "BrandId", "Name", product.BrandId);
            return View("~/Views/Admin/AdminProducts/Edit.cshtml", product);
        }

        var existing = _context.Products.Find(product.ProductId);
        if (existing == null) return NotFound();

        // Main image
        if (IsBase64Image(product.MainImageUrl))
        {
            DeleteOldImage(existing.MainImageUrl);
            product.MainImageUrl = SaveBase64Image(product.MainImageUrl);
        }
        else if (string.IsNullOrWhiteSpace(product.MainImageUrl))
        {
            DeleteOldImage(existing.MainImageUrl);
            product.MainImageUrl = null;
        }
        else
        {
            product.MainImageUrl = existing.MainImageUrl;
        }

        // Sub images
        for (int i = 1; i <= 3; i++)
        {
            string prop = $"SubImage{i}Url";
            var newValue = (string)product.GetType().GetProperty(prop)?.GetValue(product);
            var oldValue = (string)existing.GetType().GetProperty(prop)?.GetValue(existing);

            if (IsBase64Image(newValue))
            {
                DeleteOldImage(oldValue);
                var savedPath = SaveBase64Image(newValue);
                product.GetType().GetProperty(prop)?.SetValue(product, savedPath);
            }
            else if (string.IsNullOrWhiteSpace(newValue))
            {
                DeleteOldImage(oldValue);
                product.GetType().GetProperty(prop)?.SetValue(product, null);
            }
            else
            {
                product.GetType().GetProperty(prop)?.SetValue(product, oldValue);
            }
        }

        // Cập nhật các trường khác
        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.DiscountPercent = product.DiscountPercent;
        existing.StockQuantity = product.StockQuantity;
        existing.CategoryId = product.CategoryId;
        existing.BrandId = product.BrandId;
        existing.Storage = product.Storage;
        existing.Color = product.Color;
        existing.VideoUrl = product.VideoUrl;
        existing.UpdatedAt = DateTime.Now;

        existing.MainImageUrl = product.MainImageUrl;
        existing.SubImage1Url = product.SubImage1Url;
        existing.SubImage2Url = product.SubImage2Url;
        existing.SubImage3Url = product.SubImage3Url;

        existing.IsActive = product.StockQuantity > 0;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
        return RedirectToAction("Index");
    }


    public IActionResult Delete(int id)
    {
        var product = _context.Products.Find(id);
        if (product != null)
        {
            DeleteImageFile(product.MainImageUrl);
            DeleteImageFile(product.SubImage1Url);
            DeleteImageFile(product.SubImage2Url);
            DeleteImageFile(product.SubImage3Url);

            _context.Products.Remove(product);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ToggleStatus(int id)
    {
        var product = _context.Products.Find(id);
        if (product == null) return Json(new { success = false });

        product.IsActive = !product.IsActive;
        product.UpdatedAt = DateTime.Now;
        _context.SaveChanges();
        return Json(new { success = true, newStatus = product.IsActive });
    }

    // ==== Tiện ích ====
    private bool IsBase64Image(string base64) =>
        !string.IsNullOrEmpty(base64) && base64.StartsWith("data:image");

    private string SaveBase64Image(string base64)
    {
        try
        {
            var parts = base64.Split(',');
            if (parts.Length != 2) return null;

            var bytes = Convert.FromBase64String(parts[1]);
            var extension = GetImageExtensionFromBase64(base64);
            var fileName = Guid.NewGuid() + extension;
            var path = Path.Combine("wwwroot", "images", fileName);
            System.IO.File.WriteAllBytes(path, bytes);
            return "/images/" + fileName;
        }
        catch { return null; }
    }

    private void DeleteImageFile(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return;
        var fullPath = Path.Combine("wwwroot", imageUrl.TrimStart('/'));
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
    private void DeleteOldImage(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath)) return;

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }

    private string GetImageExtensionFromBase64(string base64)
    {
        if (base64.Contains("image/png")) return ".png";
        if (base64.Contains("image/jpeg")) return ".jpg";
        if (base64.Contains("image/gif")) return ".gif";
        if (base64.Contains("image/webp")) return ".webp";
        return ".jpg";
    }
}
