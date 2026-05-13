using E_commerce_Website__Skincare_.Data;
using E_commerce_Website__Skincare_.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(products);
        }











    }
}