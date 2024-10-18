﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Shopping.Models;
using Shopping.Repository;


namespace Shopping.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("Admin/Product")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [Route("Index")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync());
        }
        [HttpGet]
        [Route("Create")]

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);



            if (ModelState.IsValid)
            {

                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Sản phẩm đã có trong database");
                    return View(product);
                }

                if (product.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;


                }
                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công ";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang bị lỗi ";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }

            return View(product);
        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id, ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            var existed_product = _dataContext.Products.Find(product.Id);
            if (ModelState.IsValid)
            {

                product.Slug = product.Name.Replace(" ", "-");
                

                if (product.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    string oldFilePath = Path.Combine(uploadDir, existed_product.Image);

                    try
                    {
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "An error occueeed while deleting the product image ");
                    }

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    existed_product.Image = imageName;


                }
                existed_product.Name = product.Name;
                existed_product.Description = product.Description;
                existed_product.Price = product.Price;
                existed_product.CategoryId = product.CategoryId;
                existed_product.BrandId = product.BrandId;

                _dataContext.Update(existed_product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật sản phẩm thành công ";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang bị lỗi ";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }

            return View(product);
        }


        [Route("Delete")]
        public async Task<IActionResult> Delete(int Id)
        {
            var orderDetailsCount = await _dataContext.OrderDetails
        .CountAsync(od => od.ProductId == Id);

            if (orderDetailsCount > 0)
            {
                ModelState.AddModelError("", "Sản phẩm này không thể xóa vì đã có đơn hàng liên quan.");
                return RedirectToAction("Index"); // Trở về danh sách sản phẩm
            }
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            if (product == null)
            {
                return NotFound();
            }
            if (!string.Equals(product.Image, "noimage.jpg"))
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string oldFilePath = Path.Combine(uploadDir, product.Image);

                try
                {
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occueeed while deleting the product image ");
                }
            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["error"] = "Sản phẩm đã xóa ";
            return RedirectToAction("Index");
        }



        [Route("AddQuantity")]
        [HttpGet]
        public async Task<IActionResult> AddQuantity(int Id)
        {
            var productbyquantity = await _dataContext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
            ViewBag.ProductByQuantity = productbyquantity;
            ViewBag.Id = Id;
            return View();
        }



        [Route("StoreProductQuantity")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StoreProductQuantity(ProductQuantityModel productQuantityModel)
        {
            // Get the product to update
            var product = _dataContext.Products.Find(productQuantityModel.ProductId);
            if (product == null)
            {

                return NotFound(); // Handle product not found scenario
            }
            product.Quantity += productQuantityModel.Quantity;

            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.DateCreated = DateTime.Now;
            _dataContext.Add(productQuantityModel);
            _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm số lượng sản phẩm thành công";
            /*            return RedirectToAction("CreateProductQuantity", "Product", new { Id = productQuantityModel.ProductId });*/
            return RedirectToAction("Index", "Product");
        }

    }

}
