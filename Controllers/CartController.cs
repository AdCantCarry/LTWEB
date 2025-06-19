using Microsoft.AspNetCore.Mvc;
using TechNova.Models;

namespace TechNova.Controllers
{
    public class CartController : Controller
    {
        private readonly StoreDbContext _context;

        public CartController(StoreDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, string color, string storage, int quantity = 1)
        {
            var user = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(user))
                return RedirectToAction("Login", "Account");

            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(x => x.ProductId == productId && x.Color == color && x.Storage == storage);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    ImageUrl = product.MainImageUrl,
                    Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price,
                    Color = color,
                    Storage = storage,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, string color, string storage, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(x => x.ProductId == productId && x.Color == color && x.Storage == storage);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int productId, string color, string storage)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            cart.RemoveAll(x => x.ProductId == productId && x.Color == color && x.Storage == storage);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }
    }


}
