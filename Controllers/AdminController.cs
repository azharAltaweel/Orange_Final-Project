using E_commerce_Website__Skincare_.Data;
using E_commerce_Website__Skincare_.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace E_commerce_Website__Skincare_.Controllers
{
    //[Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;

            _context = context;
        }


        // USERS PAGE
        public async Task<IActionResult> UsersInfo()
        {
            var users = await _userManager.Users.ToListAsync();

            return View(users);
        }

        // PRODUCTS PAGE

        public async Task<IActionResult> ProductsInfo()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                 .Include(p => p.Discount)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Discounts = await _context.Discounts.ToListAsync();
            return View(products);
        }

        // ADD PRODUCT

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product,List<IFormFile> images)
        {
              // save product first

                _context.Products.Add(product);

                await _context.SaveChangesAsync();

                // SAVE IMAGES

                if (images != null && images.Count > 0)
                {
                    foreach (var image in images)
                    {
                        // UNIQUE FILE NAME

                        var fileName = Guid.NewGuid().ToString()
                                       + Path.GetExtension(image.FileName);

                        // FILE PATH

                        var path = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot/images/products",
                            fileName);

                        // SAVE IMAGE INSIDE WWWROOT

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // SAVE IMAGE INSIDE DATABASE

                        ProductImage productImage = new ProductImage
                        {
                            ImageUrl = "/images/products/" + fileName,

                            ProductId = product.Id
                        };

                        _context.ProductImages.Add(productImage);
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("ProductsInfo");
            

            ViewBag.Categories = await _context.Categories.ToListAsync();

            return RedirectToAction("ProductsInfo");
        }



        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // DELETE IMAGES FROM FOLDER

            foreach (var image in product.Images)
            {
                var oldPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    image.ImageUrl.TrimStart('/'));

                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            // DELETE IMAGES FROM DATABASE

            _context.ProductImages.RemoveRange(product.Images);

            // DELETE PRODUCT

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return RedirectToAction("ProductsInfo");
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(
            Product updatedProduct,
            List<IFormFile> images)
        {
            // GET PRODUCT FROM DATABASE

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == updatedProduct.Id);

            if (product == null)
            {
                return NotFound();
            }

            // UPDATE PRODUCT DATA

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.StockQuantity = updatedProduct.StockQuantity;
            product.CategoryId = updatedProduct.CategoryId;
            product.DiscountId = updatedProduct.DiscountId;

            // REPLACE IMAGES

            if (images != null && images.Count > 0)
            {
                // DELETE OLD IMAGES FROM WWWROOT

                foreach (var oldImage in product.Images)
                {
                    var oldPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        oldImage.ImageUrl.TrimStart('/'));

                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // DELETE OLD IMAGES FROM DATABASE

                _context.ProductImages.RemoveRange(product.Images);

                // ADD NEW IMAGES

                foreach (var image in images)
                {
                    // GENERATE UNIQUE FILE NAME

                    var fileName = Guid.NewGuid().ToString()
                                   + Path.GetExtension(image.FileName);

                    // CREATE FILE PATH

                    var path = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/images/products",
                        fileName);

                    // SAVE IMAGE IN WWWROOT

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    // SAVE IMAGE IN DATABASE

                    ProductImage productImage = new ProductImage
                    {
                        ImageUrl = "/images/products/" + fileName,
                        ProductId = product.Id
                    };

                    _context.ProductImages.Add(productImage);
                }
            }

            // SAVE CHANGES

            await _context.SaveChangesAsync();

            return RedirectToAction("ProductsInfo");
        }
















        // Action to  fetch info orders from db to show in view 
        public async Task<IActionResult> OrdersInfo()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // Update statsus order (Processing / Completed / Cancelled)
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Done Update Status Order Succesed" });
        }

        // Delete Orderer
        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Done Delete Order Succesed" });
        }

        // Order Details 
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return PartialView("_OrderDetails", order);
        }

        // Creat Recumendation => Testimonials
        public async Task<IActionResult> Testimonials()
        {
            var testimonials = await _context.Testimonials
                .Include(t => t.User)
                .OrderByDescending(t => t.Id)
                .ToListAsync();

            var reviews = await _context.Reviews
                .Include(t => t.User)
                .Include(t => t.Product)
                .OrderByDescending(t => t.Id)
                .ToListAsync();

            ViewBag.Reviews = reviews;
            return View(testimonials);
        }

        // Accept recumendation 
        [HttpPost]
        public async Task<IActionResult> ApproveTestimonial(int id)
        {
            var testimonials = await _context.Testimonials.FindAsync(id);
            if (testimonials == null) return NotFound();

            testimonials.IsApproved = !testimonials.IsApproved;
            await _context.SaveChangesAsync();
            return Json(new { success = true, approved = testimonials.IsApproved });
        }

        // deleet recumendation
        [HttpPost]
        public async Task<IActionResult> DeleteTestimonial(int id)
        {
            var testimonials = await _context.Testimonials.FindAsync(id);
            if (testimonials == null) return NotFound();

            _context.Testimonials.Remove(testimonials);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // Approve reviwe
        [HttpPost]
        public async Task<IActionResult> ApproveReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            review.IsApproved = !review.IsApproved;
            await _context.SaveChangesAsync();

            return Json(new { success = true, approved = review.IsApproved });
        }

        // Delete review
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        //(POPUP) PROFILE
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return PartialView("_Profile", user);
        }

        // eedit info profile 
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, string email, string? newPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = fullName;
            user.Email = email;
            user.UserName = email;

            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, newPassword);
            }

            return Json(new { success = true, messsage = "Profile updated successfully" });
        }
    }
}