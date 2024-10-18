using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Repository;

namespace Shopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Order")]

    [Authorize]
    public class OrderController : Controller
    {

        private readonly DataContext _dataContext;
        public OrderController(DataContext context)
        {
            _dataContext = context;
        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
        }

        [Route("ViewOrder")]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product).Where(od => od.OrderCode == ordercode).ToListAsync();
            return View(DetailsOrder);
        }

        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                return NotFound();
            }
            order.Status = status;
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status updated successfully" });
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the order status.");
            }
        }

        // Delete action method
        [Route("Delete")]

        public async Task<IActionResult> Delete(int id)
        {
            var order = await _dataContext.Orders.FindAsync(id);
            var orderDetail = await _dataContext.OrderDetails.FirstOrDefaultAsync(od => od.OrderCode == order.OrderCode);
            if (order == null)
            {
                return NotFound();
            }

            try
            {
                _dataContext.Orders.Remove(order);
                _dataContext.OrderDetails.Remove(orderDetail);
                await _dataContext.SaveChangesAsync();
                TempData["error"] = "Đơn hàng đã xóa ";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the order.");
            }
        }

    }
}