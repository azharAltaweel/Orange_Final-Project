using Jumla

    .Data;
using Jumla.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Jumla.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // ==========================================
        // HOME PAGE
        // ==========================================

        public async Task<IActionResult> HomePage()
        {
            var products = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Take(8)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();

            ViewBag.DiscountProducts = await _context.Discounts
                .Include(d => d.Products)
                    .ThenInclude(p => p.Images)
                .Take(4)
                .ToListAsync();

            // ✅ من azhar: تعليق توضيحي + نفس المنطق
            ViewBag.Testimonials = await _context.Testimonials
                .Include(t => t.User)
                .Where(t => t.IsApproved)   // Only show approved testimonials
                .Take(3)
                .ToListAsync();

            return View(products);
        }


        // ==========================================
        // PRODUCTS PAGE
        // ==========================================

        // ✅ من saeed: فيتشر كاملة مش موجودة عند azhar
        public IActionResult Products(
            string search,
            int? categoryId,
            string sortOrder)
        {
            var products = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.Discount)
                .AsQueryable();

            // SEARCH BY PRODUCT NAME OR CATEGORY
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                products = products.Where(p =>
                    p.Name.Contains(search)
                    ||
                    p.Category.Name.Contains(search));
            }

            // FILTER BY CATEGORY
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            // SORTING
            switch (sortOrder)
            {
                case "name":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "price":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "newest":
                    products = products.OrderByDescending(p => p.Id);
                    break;
                default:
                    products = products.OrderByDescending(p => p.Id);
                    break;
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.SelectedCategory = categoryId;

            return View(products.ToList());
        }


        // ==========================================
        // PRODUCT DETAILS
        // ==========================================

        // ✅ من saeed: فيتشر كاملة — ratings, related products, approved reviews فقط
        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.Discount)
                .Include(p => p.Reviews.Where(r => r.IsApproved)) // Only approved reviews
                    .ThenInclude(r => r.User)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            ViewBag.AverageRating = product.Reviews.Any()
                ? product.Reviews.Average(r => r.Rating)
                : 0;

            ViewBag.TotalReviews = product.Reviews.Count();

            var relatedProducts = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Discount)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToList();

            ViewData["RelatedProducts"] = relatedProducts;

            return View(product);
        }


        // ==========================================
        // ADD REVIEW
        // ==========================================

        // ✅ من saeed: فيتشر كاملة مش موجودة عند azhar
        [HttpPost]
        public IActionResult AddReview(int productId, string comment, int rating)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Please select a rating.";
                return RedirectToAction("ProductDetails", new { id = productId });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Prevent duplicate review
            var alreadyReviewed = _context.Reviews.Any(r =>
                r.ProductId == productId && r.UserId == userId);

            if (alreadyReviewed)
            {
                TempData["Error"] = "You already reviewed this product.";
                return RedirectToAction("ProductDetails", new { id = productId });
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Comment = comment,
                Rating = rating,
                IsApproved = false // Waits for admin approval
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            TempData["Success"] = "Review submitted and waiting for approval.";
            return RedirectToAction("ProductDetails", new { id = productId });
        }


        // ==========================================
        // ADD TO CART
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("HomePage");
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    _context.CartItems.Add(new CartItem
                    {
                        UserId = userId,
                        ProductId = productId,
                        Quantity = quantity
                    });
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                var sessionItems = HttpContext.Session
                    .GetObjectFromJson<List<SessionCartItem>>("SessionCart")
                    ?? new List<SessionCartItem>();

                var existingItem = sessionItems.FirstOrDefault(si => si.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    sessionItems.Add(new SessionCartItem
                    {
                        ProductId = productId,
                        Quantity = quantity
                    });
                }
                HttpContext.Session.SetObjectAsJson("SessionCart", sessionItems);
            }

            TempData["SuccessMessage"] = "Item added to cart successfully!";
            return RedirectToAction("HomePage");
        }


        // ==========================================
        // SUBMIT TESTIMONIAL
        // ==========================================

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTestimonial(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Testimonial content cannot be empty.";
                return RedirectToAction("HomePage");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Challenge(); // Forces re-login if identity check fails

            var testimonial = new Testimonial
            {
                UserId = userId,
                Content = content.Trim(),
                IsApproved = false // Hidden until admin approves
            };

            _context.Testimonials.Add(testimonial);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thank you! Your testimonial has been submitted and is pending admin approval."; return RedirectToAction("HomePage");
        }


        // ==========================================
        // CATEGORIES PAGE
        // ==========================================

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }


        // ==========================================
        // WISHLIST
        // ==========================================

        public IActionResult Wishlist()
        {
            return View();
        }

        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }  
    }
}