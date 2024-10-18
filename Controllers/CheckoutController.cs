using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Areas.Admin.Repository;
using Shopping.Models;
using Shopping.Models.ViewModels;
using Shopping.Repository;
using System.Drawing.Printing;
using System.Security.Claims;

namespace Shopping.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;

        public CheckoutController(IEmailSender emailSender,DataContext context)
        {
            _dataContext = context;
            _emailSender = emailSender;
        }
        
      

        public async Task<IActionResult> Checkout()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login","Account");
            }

            List<CartItemModel> cartItemsCheck = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            foreach (var cart in cartItemsCheck)
            {
              
                var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();

                if(cart.Quantity > product.Quantity)
                {
                    TempData["success"] = "Sản phẩm " +" " +cart.ProductName+" " + " hiện tại trong kho chỉ còn"+" "+product.Quantity;
                    return RedirectToAction("Index", "Cart");

                }
            }


            {
                var ordercode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel();
                orderItem.OrderCode = ordercode;
                orderItem.UserName = userEmail;
                orderItem.Status = 1;
                orderItem.CreatedDate = DateTime.Now;
                _dataContext.Add(orderItem);
                _dataContext.SaveChanges();
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                foreach (var cart in cartItems)
                {
                    var orderdetails = new OrderDetails();
                    orderdetails.UserName = userEmail;
                    orderdetails.OrderCode = ordercode;
                    orderdetails.Price = cart.Price;
                    orderdetails.ProductId = cart.ProductId;
                    orderdetails.Quantity = cart.Quantity;

                    var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync(); 
                    product.Quantity -= cart.Quantity;
                    product.Sold += cart.Quantity;
                    _dataContext.Update(product);
                    _dataContext.Add(orderdetails);
                    _dataContext.SaveChanges();
                }
                HttpContext.Session.Remove("Cart");

                //send mail oder when success
                var receiver = userEmail;
                var subject = "Đặt hàng thành công";
                var message = "Đặt hàng thành công, trải nghiệm dịch vụ nhé.";


                await _emailSender.SendEmailAsync(receiver, subject, message);

                TempData["success"] = "Đơn hàng đặt thành công, vui lòng chờ duyệt hàng nhé";
                return RedirectToAction("Index", "Cart");
            }
            return View();
        }
    }
}