using ThuyTo.Models.Authentication;
using ThuyTo.Models;
using ThuyTo.SessionSystem;
using Microsoft.AspNetCore.Mvc;
using PagedList.Core;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;

namespace ThuyTo.Areas.Admin.Controllers
{
	[Area("Admin")]
    [AdminAuthentication]

    public class OrderController : Controller
	{
		private readonly ThuyToContext _context;
        private readonly INotyfService _notyf;
		public OrderController(ThuyToContext context, INotyfService notyf)
		{
			_context = context;
            _notyf = notyf;
		}

		public IActionResult Index(int? page)
		{
			var pageNumber = page == null || page <= 0 ? 1 : page.Value;
			var pageSize = 10;

			var ordersQuery = _context.Orders
				.OrderByDescending(m => m.CreatedDate);

			var models = new PagedList<Order>(ordersQuery, pageNumber, pageSize); 

			ViewBag.currentPage = pageNumber;
			return View(models);
		}

        public IActionResult Details(long? orderId)
        {
            if (orderId == null)
            {
                return NotFound();
            }
            // Lấy thông tin đơn hàng và khách hàng
            var order = _context.Orders
                        .AsNoTracking()
                        .Include(m => m.User)
                        .FirstOrDefault(m => m.OrderId == orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Lấy thông tin chi tiết đơn hàng (sản phẩm, số lượng, ...)
            var orderDetail = _context.OrderDetails
                              .AsNoTracking()
                              .Where(m => m.OrderId == orderId)
                              .Include(m => m.Product)
                              .ToList();
            ViewBag.OrderDetail = orderDetail;

            
            if (order.FeeShipId != null)
            {
                var feeShip = _context.FeeShips
                                .AsNoTracking()
                                .FirstOrDefault(x => x.FeeShipId == order.FeeShipId);
               
                ViewBag.ShippingFee = feeShip != null ? feeShip.ShipPrice : 20000;
            }
            else
            {
                ViewBag.ShippingFee = 20000;
            }

            return View(order);
        }

        [HttpPost]
		public async Task<IActionResult> DeletePernament(long? IdToDelete)
		{
			if (IdToDelete == null)
			{
				return Json(new { message = "Can not find id", status = 1 });
			}
			try
			{
				var item = await _context.Orders.FindAsync(IdToDelete);
				if (item == null)
				{
					return Json(new { message = "Không tìm thấy đơn hàng", status = 1 });
				}

				_context.Orders.Remove(item);
				await _context.SaveChangesAsync();

				return Json(new { message = "Success", status = 0 });
			}
			catch
			{
				return Json(new { message = "Lỗi từ server", status = 1 });
			}
		}

		public IActionResult ChangeOrderStatus(int? id, string type = "duyet")
		{
			if (id == null)
			{
				return NotFound();
			}
			var itemById = _context.Orders.FirstOrDefault(m => m.OrderId == id);
			if (itemById == null)
			{
				_notyf.Warning("Không thể thực hiện");
				return RedirectToAction("Index");
			}
            if(type == "duyet")
            {
				itemById.OrderStatus = 3;
				_notyf.Success("Đã duyệt đơn hàng");
			} else 
            {
				itemById.OrderStatus = 5;
				_notyf.Success("Đã hủy đơn hàng");
			}
			_context.Orders.Update(itemById);
			_context.SaveChanges();
			
			return RedirectToAction("Index", new {Area = "Admin"});
		}
		[HttpPost]
		public IActionResult UpdateOrderStatus(long orderId, int newStatus)
		{
			var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
			if (order == null)
			{
				_notyf.Error("Không tìm thấy đơn hàng");
				return RedirectToAction("Index");
			}

			order.OrderStatus = newStatus;
			_context.Orders.Update(order);
			_context.SaveChanges();

			_notyf.Success("Cập nhật trạng thái đơn hàng thành công");
			return RedirectToAction("Details", new { orderId = orderId });
		}
        [HttpGet]
        public IActionResult ExportInvoice(long? orderId)
        {
            if (orderId == null)
            {
                return NotFound();
            }

            var order = _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                _notyf.Error("Không tìm thấy đơn hàng.");
                return RedirectToAction("Index");
            }

            var orderDetails = _context.OrderDetails
                .AsNoTracking()
                .Where(od => od.OrderId == orderId)
                .Include(od => od.Product)
                .ToList();

            var shippingFee = ViewBag.ShippingFee ?? 20000;
            var total = orderDetails.Sum(od => od.Quantity * od.Price) + shippingFee;

            // Đảm bảo kiểu dữ liệu trước khi truyền vào
            var shippingFeeInt = Convert.ToInt32(shippingFee);
            var totalInt = Convert.ToInt32(total);

            // Tạo nội dung PDF
            var pdfContent = GenerateInvoicePDF(order, orderDetails, totalInt, shippingFeeInt);

            // Xuất file PDF
            var fileName = $"HoaDon_{order.OrderId}.pdf";
            return File(pdfContent, "application/pdf", fileName);
        }


       private byte[] GenerateInvoicePDF(Order order, List<OrderDetail> orderDetails, int total, int shippingFee)
{
            
            using (var ms = new MemoryStream())
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4); // Kích thước trang A4
                page.Margin(2, Unit.Centimetre); // Lề trang
                page.DefaultTextStyle(x => x.FontFamily("Roboto").FontSize(12));
                page.Content().Column(col =>
                {
                    // Thông tin khách hàng
                    col.Item().Text($"Ngày đặt: {order.CreatedDate:dd/MM/yyyy}");
                    col.Item().Text($"Khách hàng: {order.FullName}");
                    col.Item().Text($"Email: {order.User?.Email}");
                    col.Item().Text($"Số điện thoại: {order.Phone}");
                    col.Item().Text($"Địa chỉ: {order.Address}");
                    col.Item().LineHorizontal(1); // Đường kẻ ngang

                    // Bảng chi tiết sản phẩm
                    col.Item().Table(table =>
                    {
                        // Định nghĩa cột
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4); // Cột tên sản phẩm
                            columns.RelativeColumn(2); // Cột số lượng
                            columns.RelativeColumn(2); // Cột đơn giá
                            columns.RelativeColumn(2); // Cột thành tiền
                        });

                        // Header bảng
                        table.Header(header =>
                        {
                            header.Cell().Text("Sản phẩm").Bold();
                            header.Cell().Text("Số lượng").Bold();
                            header.Cell().Text("Đơn giá").Bold();
                            header.Cell().Text("Thành tiền").Bold();
                        });

                        // Duyệt qua các chi tiết đơn hàng
                        foreach (var detail in orderDetails)
                        {
                            table.Cell().Text(detail.Product.ProductName);
                            table.Cell().Text(detail.Quantity.ToString());
                            table.Cell().Text($"{detail.Price:#,##0 VNĐ}");
                            table.Cell().Text($"{(detail.Quantity * detail.Price):#,##0 VNĐ}");
                        }

                        // Tổng cộng và phí vận chuyển
                        table.Footer(footer =>
                        {
                            footer.Cell().Text("Phí vận chuyển").Bold();
                            footer.Cell().ColumnSpan(3).AlignRight().Text($"{shippingFee:#,##0 VNĐ}");
                        });

                        table.Footer(footer =>
                        {
                            footer.Cell().Text("Tổng cộng").Bold();
                            footer.Cell().ColumnSpan(3).AlignRight().Text($"{total:#,##0 VNĐ}");
                        });
                    });
                });
            });
        });

        document.GeneratePdf(ms); // Tạo PDF
        return ms.ToArray(); // Trả về mảng byte
    }
}
		

	}
}
