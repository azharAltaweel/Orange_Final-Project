using Microsoft.AspNetCore.Mvc;
using E_commerce_Website__Skincare_.Data;
using E_commerce_Website__Skincare_.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace E_commerce_Website__Skincare_.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Checkout()
        {
            return View();
        }

        public IActionResult Payment()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDefaultCartItems()
        {
            // Fetch default products from database
            var dbProducts = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Take(2)
                .ToListAsync();

            // If database doesn't have any products, we seed it with some default products!
            if (dbProducts.Count == 0)
            {
                // Ensure a default category exists first
                var category = await _context.Categories.FirstOrDefaultAsync();
                if (category == null)
                {
                    category = new Category
                    {
                        Name = "Skincare",
                        ImageUrl = "https://images.unsplash.com/photo-1620916566398-39f1143ab7be?auto=format&fit=crop&q=80&w=400"
                    };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }

                // Add default seed products
                var p1 = new Product
                {
                    Name = "Botanical Radiance Serum",
                    Description = "Rejuvenating serum for all skin types.",
                    Price = 84.00m,
                    StockQuantity = 50,
                    CategoryId = category.Id
                };
                var p2 = new Product
                {
                    Name = "Cloud Hydration Cream",
                    Description = "Intense moisture and hydration cream.",
                    Price = 62.00m,
                    StockQuantity = 30,
                    CategoryId = category.Id
                };

                _context.Products.Add(p1);
                _context.Products.Add(p2);
                await _context.SaveChangesAsync();

                // Save their associated images
                var img1 = new ProductImage
                {
                    ImageUrl = "https://images.unsplash.com/photo-1620916566398-39f1143ab7be?auto=format&fit=crop&q=80&w=400",
                    ProductId = p1.Id
                };
                var img2 = new ProductImage
                {
                    ImageUrl = "https://images.unsplash.com/photo-1608248597279-f99d160bfcbc?auto=format&fit=crop&q=80&w=400",
                    ProductId = p2.Id
                };

                _context.ProductImages.Add(img1);
                _context.ProductImages.Add(img2);
                await _context.SaveChangesAsync();

                // Re-fetch
                dbProducts = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .Take(2)
                    .ToListAsync();
            }

            var items = dbProducts.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = (double)p.Price,
                image = p.Images.FirstOrDefault()?.ImageUrl ?? "https://images.unsplash.com/photo-1620916566398-39f1143ab7be?auto=format&fit=crop&q=80&w=400",
                size = "50ml", // default size
                category = p.Category?.Name ?? "Skincare",
                quantity = 1
            }).ToList();

            return Json(items);
        }
    }
}

