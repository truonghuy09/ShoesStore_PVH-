﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Models;
using Shopping.Repository;
using System.Diagnostics;

namespace Shopping.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUserModel> _userManager;

        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;
        }




        public async Task<IActionResult> Index(int pg = 1)
        {

            var product = _dataContext.Products.Include("Category").Include("Brand").ToList();
            var sliders = _dataContext.Sliders.Where(s => s.Status == 1).ToList();
            const int pageSize = 6;
            if (pg < 1)
            {
                pg = 1;
            }
            int recsCount = product.Count();

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize;

            var data = product.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;
            ViewBag.Sliders = sliders;

            return View(data);
        }


        public async Task<IActionResult> Compare()
        {
            var compare_product = await(from c in _dataContext.Compares
                                        join p in _dataContext.Products on c.ProductId equals p.Id
                                        join u in _dataContext.Users on c.UserId equals u.Id
                                        select new { User = u, Product = p, Compares = c })
            .ToListAsync();
            return View(compare_product);

        }

        
        public async Task<IActionResult> DeleteCompare(int Id)
        {
            CompareModel compare = await _dataContext.Compares.FindAsync(Id);

            _dataContext.Compares.Remove(compare);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "So sánh đã được xóa thành công";
            return RedirectToAction("Compare" ,"Home");

        }

        public async Task<IActionResult> Wishlist()
        {
            var wishlist_product = await (from w in _dataContext.Wishlists
                                         join p in _dataContext.Products on w.ProductId equals p.Id
                                         join u in _dataContext.Users on w.UserId equals u.Id
                                         select new { User = u, Product = p, Wishlist = w })
            .ToListAsync();
            return View(wishlist_product);

        }

        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            WishlistModel Wishlist = await _dataContext.Wishlists.FindAsync(Id);

            _dataContext.Wishlists.Remove(Wishlist);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Yêu thích đã được xóa thành công";
            return RedirectToAction("Wishlist" ,"Home");

        }

        [HttpPost]
        public async Task<IActionResult> AddWishlist(int Id, WishlistModel wishlistmodel)
        {
            var user = await _userManager.GetUserAsync(User);
            var wishlistProduct = new WishlistModel
            {
                ProductId = Id,
                UserId = user.Id
            };
            _dataContext.Wishlists.Add(wishlistProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Add to wishlisht Successfully" });
            }
            catch (Exception)
            {
            return StatusCode(500, "An error occurred while adding to wishlist table.");
            }
        }



        [HttpPost]
        public async Task<IActionResult> AddCompare(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            var compareModel = new CompareModel
            {
                ProductId = Id,
                UserId = user.Id
            };
            _dataContext.Compares.Add(compareModel);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Add to compare Successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding to compare table.");
            }
        }



        public IActionResult Privacy()
		{
			return View();
		}

		public async Task<IActionResult> Contact()
		{
			var contact= await _dataContext.Contact.FirstAsync();
			return View(contact);
		}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error(int statuscode )
		{
			if(statuscode == 404)
			{
				return View("NotFound");
			}
			else
			{
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
		}
	}
}
