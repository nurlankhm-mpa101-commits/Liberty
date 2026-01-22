using Liberty.Context;
using Liberty.FileHelpers;
using Liberty.Models;
using Liberty.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Liberty.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly string _folderPath;

        public ProductController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            _folderPath = Path.Combine(_environment.WebRootPath, "assets", "images");
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Select(x => new ProductGetVM()
            {
                Id = x.Id,
                Name=x.Name,
                Description=x.Description,
                ImagePath=x.ImagePath,
                Price=x.Price,
                Rating=x.Rating,
                CategoryName=x.Category.Name

            }).ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await _sendCategoriesWithViewBag();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult>Create(ProductCreateVM vm)
        {
            await _sendCategoriesWithViewBag();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var isExistCategory = await _context.Categories.AnyAsync(x => x.Id == vm.CategoryId);

            if (!isExistCategory)
            {
                ModelState.AddModelError("CategoryId", "There is no such category");
                return View(vm);
            }

            if (!vm.Image.CheckSize(2))
            {
                ModelState.AddModelError("Image", "image file only");
                return View(vm);
            }

            if (!vm.Image.CheckType("image"))
            {
                ModelState.AddModelError("image", "image file only");
                return View(vm);
            }

            string uniqueFileName = await vm.Image.FileUploadAsync(_folderPath);

            Product product = new()
            {
                Name = vm.Name,
                Description = vm.Description,
                ImagePath = uniqueFileName,
                Price = vm.Price,
                Rating = vm.Rating,
                CategoryId = vm.CategoryId
            };

            _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult>Update(int id)
        {
            var product = await _context.Products.FindAsync(id);

            ProductUpdateVM vm = new()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price=product.Price,
                Rating = product.Rating,
                CategoryId=product.CategoryId
            };

            await _sendCategoriesWithViewBag();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult>Update(ProductUpdateVM vm)
        {
            await _sendCategoriesWithViewBag();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var isExistCategory = await _context.Categories.AnyAsync(x => x.Id == vm.CategoryId);

            if (!isExistCategory)
            {
                ModelState.AddModelError("CategoryId", "There is no such category");
                return View(vm);
            }

            if (!vm.Image?.CheckSize(2) ?? false)
            {
                ModelState.AddModelError("Image", "image file only");
                return View(vm);
            }

            if (!vm.Image?.CheckType("image") ?? false)
            {
                ModelState.AddModelError("image", "image file only");
                return View(vm);
            }

            var existProduct = await _context.Products.FindAsync(vm.Id);

            if (existProduct is null)
                return BadRequest();

            existProduct.Name = vm.Name;
            existProduct.Description = vm.Description;
            existProduct.Price = vm.Price;
            existProduct.Rating = vm.Rating;
            existProduct.CategoryId = vm.CategoryId;

            if(vm.Image is { })
            {
                string newImagePath = await vm.Image.FileUploadAsync(_folderPath);
                string oldImagePath = Path.Combine(_folderPath, existProduct.ImagePath);
                existProduct.ImagePath = newImagePath;
                FileHelper.FileDelete(oldImagePath);
            }

            _context.Products.Update(existProduct);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult>Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            string deletedImagePath = Path.Combine(_folderPath,product.ImagePath);
            FileHelper.FileDelete(deletedImagePath);
            return RedirectToAction("Index");
        }

        private async Task _sendCategoriesWithViewBag()
        {
            var categories = await _context.Categories.Select(c => new SelectListItem()
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToListAsync();

            ViewBag.Categories = categories;
        }
    }
}
