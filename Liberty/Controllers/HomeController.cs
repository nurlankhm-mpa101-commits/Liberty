using System.Diagnostics;
using Liberty.Context;
using Liberty.Models;
using Liberty.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Liberty.Controllers
{
    public class HomeController(AppDbContext _context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Select(x => new ProductGetVM()
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImagePath = x.ImagePath,
                Price = x.Price,
                Rating = x.Rating,
                CategoryName = x.Category.Name

            }).ToListAsync();

            return View(products);
        }
    }
}
