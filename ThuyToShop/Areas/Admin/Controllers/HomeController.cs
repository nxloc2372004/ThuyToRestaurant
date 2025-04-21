using ThuyTo.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using ThuyTo.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace ThuyTo.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthentication]
    public class HomeController : Controller
    {
        private readonly ThuyToContext _context;

        public HomeController(ThuyToContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Thống kê cơ bản
            var orders = _context.Orders.Where(o => o.CreatedDate.HasValue);
            ViewBag.TotalOrders = orders.Count();
            ViewBag.TotalRevenue = orders.Sum(o => (decimal?)o.TotalAmount) ?? 0;
            ViewBag.TotalCustomers = _context.Users.Count();

            // Đơn hàng gần đây
            ViewBag.RecentOrders = orders
                .OrderByDescending(o => o.CreatedDate)
                .Take(10)
                .Select(o => new
                {
                    o.OrderId,
                    CustomerName = o.FullName,
                    o.CreatedDate,
                    o.TotalAmount,
                    o.OrderStatus
                })
                .ToList();

            return View();
        }
        [HttpGet]
        public JsonResult GetDashboardData(string filter)
        {
            var now = DateTime.Now;
            var orders = _context.Orders.Where(o => o.CreatedDate.HasValue);

            switch (filter)
            {
                case "today":
                    orders = orders.Where(o => o.CreatedDate.Value.Date == DateTime.Today);
                    break;
                case "month":
                    orders = orders.Where(o => o.CreatedDate.Value.Month == now.Month && o.CreatedDate.Value.Year == now.Year);
                    break;
                case "year":
                    orders = orders.Where(o => o.CreatedDate.Value.Year == now.Year); 
                    break;
                
                default:
                    
                    break;
            }

            int totalOrders = orders.Count();
            decimal totalRevenue = orders.Sum(o => (decimal?)o.TotalAmount) ?? 0;

            return Json(new
            {
                totalOrders,
                totalRevenue = totalRevenue.ToString("N0") + "đ"
            });
        }
        [HttpGet]
        public JsonResult GetRecentOrders()
        {
            var recentOrders = _context.Orders
                .OrderByDescending(o => o.CreatedDate)
                .Take(5)
                .Select(o => new
                {
                    id = o.OrderId,
                    customerName = o.FullName,
                    createdDate = o.CreatedDate.HasValue ? o.CreatedDate.Value.ToString("dd/MM/yyyy") : "",
                    totalAmount = o.TotalAmount.ToString() + "đ",
                    status = o.OrderStatus,
                    statusCode = o.OrderStatus 
                })
                .ToList();

            return Json(recentOrders);
        }
        [HttpGet]
        public IActionResult GetTopSellingProducts()
        {
            try
            {
                var query = _context.OrderDetails
                    .Include(od => od.Order)
                    .Include(od => od.Product)
                    .Where(od => od.Product != null && od.Order != null)
                    .Where(od => od.Order.OrderStatus == 2 ||
                                od.Order.OrderStatus == 3 ||
                                od.Order.OrderStatus == 4)
                    .AsEnumerable()
                    .GroupBy(od => od.ProductId)
                    .Select(g => new
                    {
                        productId = g.Key,
                        productName = g.First().Product.ProductName,
                        price = g.First().Product.ProductPrice,
                        totalSold = g.Sum(od => od.Quantity),
                        totalRevenue = g.Sum(od => od.Quantity * od.Price)
                    })
                    .OrderByDescending(p => p.totalSold)
                    .Take(5)
                    .ToList();

                return Json(new
                {
                    success = true,
                    message = "Thành công",
                    data = query
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message,
                    data = new List<object>()
                });
            }
        }
		[HttpGet]
		public IActionResult GetRevenueData(DateTime startDate, DateTime endDate)
		{
			try
			{
				// Đảm bảo endDate bao gồm cả ngày đó
				endDate = endDate.Date.AddDays(1).AddSeconds(-1);

				var dailyRevenue = _context.Orders
					.Where(o => o.CreatedDate >= startDate && o.CreatedDate <= endDate)
					.GroupBy(o => o.CreatedDate.Value.Date)
					.Select(g => new
					{
						Date = g.Key,
						Revenue = g.Sum(o => o.TotalAmount)
					})
					.OrderBy(x => x.Date)
					.ToList();

				// Tạo danh sách tất cả các ngày trong khoảng để đảm bảo không bỏ sót ngày nào
				var allDates = new List<DateTime>();
				for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
				{
					allDates.Add(date);
				}

				var chartData = new
				{
					categories = allDates
						.Select(date => date.ToString("dd/MM"))
						.ToArray(),
					revenueData = allDates
						.Select(date => {
							var dayData = dailyRevenue.FirstOrDefault(d => d.Date == date.Date);
							return dayData != null ? (double)dayData.Revenue : 0;
						})
						.ToArray()
				};

				return Json(new
				{
					success = true,
					message = "Thành công",
					chartData = chartData
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Lỗi: " + ex.Message,
					chartData = new
					{
						categories = new string[0],
						revenueData = new double[0]
					}
				});
			}
		}
	}

}
