using Microsoft.AspNetCore.Mvc;
using TechNova.Models;
using TechNova.middleware;

[AdminAuthorize] // vẫn dùng middleware kiểm tra role
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
        return View("~/Views/Admin/AdminProducts/Create.cshtml");
    }

    [HttpPost]
    public IActionResult Create(Product product)
    {
        if (ModelState.IsValid)
        {
            product.CreatedAt = DateTime.Now;
            _context.Products.Add(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View("~/Views/Admin/AdminProducts/Create.cshtml", product);
    }


    public IActionResult Edit(int id)
    {
        var product = _context.Products.Find(id);
        return View("~/Views/Admin/AdminProducts/Edit.cshtml", product);
    }

    [HttpPost]
    public IActionResult Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            product.UpdatedAt = DateTime.Now;
            _context.Products.Update(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View("~/Views/Admin/AdminProducts/Edit.cshtml", product);
    }


    public IActionResult Delete(int id)
    {
        var product = _context.Products.Find(id);
        _context.Products.Remove(product);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}
