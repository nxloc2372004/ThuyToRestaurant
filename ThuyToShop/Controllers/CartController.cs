using AspNetCoreHero.ToastNotification.Abstractions;
using ThuyTo.Models;
using ThuyTo.SessionSystem;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Newtonsoft.Json;
using ThuyTo.Models.VNPAY;
using ThuyTo.Models.Service;

namespace ThuyTo.Controllers
{
	public class CartController : Controller
	{
		private readonly ThuyToContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly INotyfService _notyf;
		private readonly IVnPayService _vnPayService;
		public CartController(ThuyToContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, INotyfService notyf, IVnPayService vnPayService)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
			_httpContextAccessor = httpContextAccessor;
			_notyf = notyf;
			_vnPayService = vnPayService;
		}
		public const string CARTKEY = "cart";

		// Lấy cart từ Session (danh sách CartItem)
		List<CartItem> GetCartItems()
		{
			var session = HttpContext.Session;
			string jsoncart = session.GetString(CARTKEY);
			if (jsoncart != null)
			{
				return JsonConvert.DeserializeObject<List<CartItem>>(jsoncart);
			}
			return new List<CartItem>();
		}

		// Xóa cart khỏi session
		void ClearCart()
		{
			var session = HttpContext.Session;
			session.Remove(CARTKEY);
		}

		// Lưu Cart (Danh sách CartItem) vào session
		void SaveCartSession(List<CartItem> ls)
		{
			var session = HttpContext.Session;
			string jsoncart = JsonConvert.SerializeObject(ls);
			session.SetString(CARTKEY, jsoncart);
		}
		string TotalCart()
		{
			var ls = GetCartItems();
			decimal? total = 0;
			if (ls.Count > 0)
			{
				foreach (var item in ls)
				{
					total += (item.product.ProductPrice - item.product.ProductPrice * item.product.ProductDisCount / 100) * item.quantity;
				}
			}
			return String.Format("{0:0,0 đ}", total);
		}

		public IActionResult Index()
		{
			return View();
		}

		/*[Route("/Cart/addcart/{productid:int}", Name = "addcart")]*/
		public IActionResult AddToCart(int productid)
		{
			try
			{
				var product = _context.Products.Where(p => p.ProductId == productid).FirstOrDefault();
				if (product == null) return Json(new
				{
					status = 1,
					message = "Cannot find this product"
				});

				var cart = GetCartItems();
				var cartitem = cart.Find(p => p.product.ProductId == productid);
				if (cartitem != null)
				{
					cartitem.quantity++;
				}
				else
				{
					cart.Add(new CartItem() { quantity = 1, product = product });
				}
				SaveCartSession(cart);
				return Json(new
				{
					status = 0,
					message = "Add cart success"
				});
			}
			catch
			{
				return Json(new
				{
					status = 2,
					message = "Error from server"
				});
			}
		}

		/// xóa item trong cart
		public IActionResult RemoveCart(int productid)
		{
			try
			{
				var cart = GetCartItems();
				var cartitem = cart.Find(p => p.product.ProductId == productid);
				if (cartitem != null)
				{
					cart.Remove(cartitem);
				}
				SaveCartSession(cart);
				var CartNumber = cart.Count;
				return Json(new
				{
					status = 0,
					cartnumber = CartNumber,
					message = "Remove Item from cart success"
				});
			}
			catch
			{
				return Json(new
				{
					status = 1,
					message = "Error from server"
				});
			}
		}
		public IActionResult DecreaseCount(int productid)
		{
			try
			{
				var cart = GetCartItems();
				var cartitem = cart.Find(p => p.product.ProductId == productid);
				string TotalPrice = "";
				if (cartitem != null)
				{
					cartitem.quantity -= 1;
					TotalPrice = String.Format("{0:0,0 đ}", (cartitem.product.ProductPrice - cartitem.product.ProductPrice * cartitem.product.ProductDisCount / 100) * cartitem.quantity);
				}
				SaveCartSession(cart);
				string totalCart = TotalCart();
				return Json(new
				{
					status = 0,
					totalprice = TotalPrice,
					totalcart = totalCart,
					message = "Remove Item from cart success"
				});
			}
			catch
			{
				return Json(new
				{
					status = 1,
					message = "Error from server"
				});
			}
		}
		public IActionResult IncreaseCount(int productid)
		{
			try
			{
				var cart = GetCartItems();
				var cartitem = cart.Find(p => p.product.ProductId == productid);
				string TotalPrice = "";
				if (cartitem != null)
				{
					cartitem.quantity += 1;
					TotalPrice = String.Format("{0:0,0 đ}", (cartitem.product.ProductPrice - cartitem.product.ProductPrice * cartitem.product.ProductDisCount / 100) * cartitem.quantity);
				}
				SaveCartSession(cart);
				string totalCart = TotalCart();
				return Json(new
				{
					status = 0,
					totalprice = TotalPrice,
					totalcart = totalCart,
					message = "Remove Item from cart success"
				});
			}
			catch
			{
				return Json(new
				{
					status = 1,
					message = "Error from server"
				});
			}
		}

		/// Cập nhật
		[Route("/updatecart", Name = "updatecart")]
		[HttpPost]
		public IActionResult UpdateCart([FromForm] int productid, [FromForm] int quantity)
		{
			// Cập nhật Cart thay đổi số lượng quantity ...
			var cart = GetCartItems();
			var cartitem = cart.Find(p => p.product.ProductId == productid);
			if (cartitem != null)
			{
				// Đã tồn tại, tăng thêm 1
				cartitem.quantity = quantity;
			}
			SaveCartSession(cart);
			// Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
			return Ok();
		}


		// Hiện thị giỏ hàng
		[Route("/gio-hang", Name = "cart")]
		public IActionResult Cart()
		{
			var UserId = HttpContext.Session.GetInt32(SessionKey.USERID);
			var listProvince = _context.Provinces.ToList();
			ViewBag.Provinces = new SelectList(listProvince.ToList(), "ProvinceId", "ProvinceName");
			ViewBag.UserId = UserId;
			return View(GetCartItems());
		}
		[HttpPost]
		[Route("/thanh-toan")]
		public IActionResult CheckOut(string FullName, string Phone, int province, int districts, int commune, string Address)
		{

			if (FullName == null) TempData["FullName"] = "Họ tên không được để trống";
			if (Phone == null) TempData["Phone"] = "Số điện thoại khôgn được để trống";
			if (districts == 0) TempData["districts"] = "Bạn chưa chọn quận - huyện";
			if (commune == 0) TempData["commune"] = "Bạn chưa chọn thị xã";
			if (Address == null) TempData["Address"] = "Địa chỉ không được để trống";

			if (FullName == null || Phone == null || districts == 0 || commune == 0 || Address == null)
			{
				var listProvince = _context.Provinces.ToList();
				ViewBag.Provinces = new SelectList(listProvince.ToList(), "ProvinceId", "ProvinceName");
				return RedirectToAction("Cart");
			}
			var UserId = HttpContext.Session.GetInt32(SessionKey.USERID);
			if (UserId == null)
			{
				return RedirectToAction("Login", "Account");
			}
			var feeshipId = 0;
			decimal feeshipPrice = 20000m;
			var feeship = _context.FeeShips.Where(m => m.DistrictId == districts && m.ProvinceId == province && m.CommuneId == commune).FirstOrDefault();
			if (feeship != null)
			{
				feeshipId = Convert.ToInt32(feeship.FeeShipId);
				feeshipPrice = decimal.Round((decimal)feeship.ShipPrice, 2);
			}
			var provinceName = _context.Provinces.Where(m => m.ProvinceId == province).Select(m => m.ProvinceName).FirstOrDefault();
			var districtName = _context.Districts.Where(m => m.DistrictId == districts).Select(m => m.DistrictName).FirstOrDefault();
			var communeName = _context.Communes.Where(m => m.CommuneId == commune).Select(m => m.CommuneName).FirstOrDefault();
			Random ran = new Random();
			int number = ran.Next(10000);
			var order = new Order();

			order.FullName = FullName;
			order.Address = string.Format("{0} - {1} - {2} - {3}", Address, communeName, districtName, provinceName);
			order.Phone = Phone;
			order.UserId = UserId;
			order.CreatedBy = UserId;
			order.ModifiedBy = UserId;
			order.CreatedDate = DateTime.Now;
			order.ModifiedDate = DateTime.Now;
			order.FeeShipId = feeshipId;
			order.Code = "DONHANG-" + number.ToString();

			ViewBag.ShipPrice = feeshipPrice;
			ViewBag.ListCart = GetCartItems();
			ViewBag.ProvinceName = provinceName;
			ViewBag.DistrictName = districtName;
			ViewBag.CommuneName = communeName;
			ViewBag.Address = Address;
			return View(order);
			//var cart = GetCartItems();
			//decimal total = cart.Sum(item =>
			//	(item.product.ProductPrice - item.product.ProductPrice * item.product.ProductDisCount / 100) * item.quantity);

			//ViewBag.ShipPrice = feeshipPrice; // Gán decimal, không format ở đây
			//ViewBag.Total = total;
		}
		void SendConfirmMail(Order order)
		{
			try
			{
				var user = _context.Users.FirstOrDefault(m => m.UserId == order.UserId && m.IsBlocked == 1 && m.IsDeleted == false);
				if (user == null || string.IsNullOrEmpty(user.Email))
				{
					_notyf.Error("Không thể gửi email: Người dùng không tồn tại hoặc chưa có email.");
					return;
				}

				var feeship = _context.FeeShips.FirstOrDefault(m => m.FeeShipId == order.FeeShipId);
				decimal? shipPrice = feeship?.ShipPrice ?? 25000;

				var cart = GetCartItems();
				if (cart == null || !cart.Any())
				{
					_notyf.Error("Không thể gửi email: Giỏ hàng trống.");
					return;
				}

				var content = "";
				foreach (var item in cart)
				{
					var total = String.Format("{0:0,0 đ}", (item.product.ProductPrice - item.product.ProductPrice * item.product.ProductDisCount / 100) * item.quantity);
					var price = String.Format("{0:0,0 đ}", (item.product.ProductPrice - item.product.ProductPrice * item.product.ProductDisCount / 100));
					content += $"<tr><td style='padding: 5px;'><a href='/'>{item.product.ProductName}</a></td><td style='padding: 5px;'>{price}</td><td style='padding: 5px;'>{item.quantity}</td><td style='padding: 5px;'>{total}</td></tr>";
				}

				var linkConfirm = $"{_httpContextAccessor?.HttpContext?.Request.Scheme}://{_httpContextAccessor?.HttpContext?.Request.Host}/confirm-buy-product?orderId={order.OrderId}";

				var path = Path.Combine(_webHostEnvironment.WebRootPath, "EmailTemplate", "ConfirmOrder.html");
				string HtmlBody;
				using (StreamReader reader = System.IO.File.OpenText(path))
				{
					HtmlBody = reader.ReadToEnd();
				}

				var messageBody = string.Format(HtmlBody,
					user.FullName,
					order.CreatedDate,
					order.FullName,
					order.Address,
					order.Phone,
					user.Email,
					order.CreatedDate,
					content,
					order.TotalAmount,
					shipPrice,
					order.TotalPayment,
					linkConfirm
				);

				var message = new MimeMessage();
				message.From.Add(new MailboxAddress("Thủy Tồ Shops", "tongthuan15092003@gmail.com"));
				message.To.Add(new MailboxAddress(user.FullName, user.Email));
				message.Subject = "Xác nhận đặt hàng";

				var builder = new BodyBuilder
				{
					HtmlBody = messageBody
				};
				message.Body = builder.ToMessageBody();

				using (var client = new SmtpClient())
				{
					client.Connect("smtp.gmail.com", 465, true);
					client.Authenticate("tongthuan15092003@gmail.com", "wcnifnnsjnyjkvlr");
					client.Send(message);
					client.Disconnect(true);
				}

				_notyf.Success("Đã gửi email xác nhận thành công.");
			}
			catch (Exception ex)
			{
				_notyf.Error($"Lỗi khi gửi email xác nhận: {ex.Message}");
			}
		}
		[HttpPost]
		public IActionResult ConfirmCheckout(Order order, string? province, string? district, string? commune, string paymentMethod = "cod")
		{
			try
			{
				// 1. Kiểm tra giỏ hàng
				var cart = GetCartItems();
				if (cart.Count == 0)
				{
					_notyf.Error("Giỏ hàng trống");
					return RedirectToAction("Cart");
				}

				// 2. Tính toán tổng tiền
				decimal totalAmount = cart.Sum(item =>
				{
					decimal price = item.product.ProductPrice;
					decimal discount = item.product.ProductDisCount ?? 0;
					decimal itemTotal = (price - price * discount / 100) * item.quantity;
					return itemTotal;
				});

				// 3. Chuẩn bị thông tin đơn hàng
				order.Code = order.Code ?? GenerateOrderCode(); // Tạo mã đơn hàng nếu chưa có
				order.TotalAmount = totalAmount;
				order.TotalPayment = totalAmount + (ViewBag.ShipPrice ?? 0);
				order.Address = $"{order.Address}, {commune}, {district}, {province}";
				order.OrderStatus = 1; // Chờ thanh toán
				order.PaymentMethod = paymentMethod == "vnpay" ? 1 : 0; // 1=VNPay, 0=COD
				order.CreatedDate = DateTime.Now;

				// 4. Xử lý theo phương thức thanh toán
				if (paymentMethod == "vnpay")
				{
					var paymentModel = new PaymentInformationModel
					{

						Amount = order.TotalPayment,
						OrderDescription = $"Thanh toán đơn hàng {order.Code}",
						Name = order.FullName
					};


					var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
					return Redirect(paymentUrl);

				}
				else
				{
					// Xử lý COD
					_context.Orders.Add(order);
					_context.SaveChanges();

					foreach (var item in cart)
					{
						_context.OrderDetails.Add(new OrderDetail
						{
							ProductId = item.product.ProductId,
							Quantity = item.quantity,
							Price = item.product.ProductPrice * (100 - item.product.ProductDisCount) / 100,
							OrderId = order.OrderId
						});
					}
					_context.SaveChanges();

					ClearCart();
					_notyf.Success("Đặt hàng COD thành công");
					return RedirectToAction("Index", "Order", new { orderId = order.Code });
				}
			}
			catch (Exception ex)
			{
				_notyf.Error("Lỗi hệ thống: " + ex.Message);
				return RedirectToAction("Cart");
			}
		}

		// Hàm tạo mã đơn hàng
		private string GenerateOrderCode()
		{
			return "DH" + DateTime.Now.ToString("yyyyMMddHHmmss");
		}

		//[HttpPost]
		//public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
		//{



		//	var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

		//	return Redirect(url);
		//}

		//[HttpGet]
		//public IActionResult PaymentCallback()
		//{
		//	var response = _vnPayService.PaymentExecute(Request.Query);

		//	return Json(response);
		//}

		[HttpPost]
		public IActionResult CreatePaymentUrlVnpay(
	string Name,
	double Amount,
	string OrderDescription,
	string OrderType,
	string OrderId,
	int UserId,
	string Address,
	string Phone,
	int FeeShipId,
	decimal TotalAmount)
		{
			try
			{

				var cart = GetCartItems();
				if (cart.Count == 0)
				{
					_notyf.Error("Giỏ hàng trống");
					return RedirectToAction("Cart");
				}


				var order = new Order
				{
					Code = OrderId,
					UserId = UserId,
					FullName = Name,
					Phone = Phone,
					Address = Address,
					TotalAmount = TotalAmount,
					TotalPayment = (decimal)Amount,
					FeeShipId = FeeShipId,
					OrderStatus = 1, // Chờ thanh toán
					PaymentMethod = 1, // VNPay
					CreatedDate = DateTime.Now,
					ModifiedDate = DateTime.Now
				};

				_context.Orders.Add(order);
				_context.SaveChanges();

				// 3. Lưu chi tiết đơn hàng
				foreach (var item in cart)
				{
					_context.OrderDetails.Add(new OrderDetail
					{
						ProductId = item.product.ProductId,
						Quantity = item.quantity,
						Price = item.product.ProductPrice * (100 - item.product.ProductDisCount) / 100,
						OrderId = order.OrderId
					});
				}
				_context.SaveChanges();

				// 4. Tạo URL thanh toán VNPay (sử dụng service hiện có)
				var paymentModel = new PaymentInformationModel
				{
					Amount = (decimal)TotalAmount,
					OrderDescription = OrderDescription,
					Name = Name,
					OrderType = OrderType
				};
				HttpContext.Session.SetString("LastOrderId", order.OrderId.ToString());
				var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);

				return Redirect(paymentUrl);
			}
			catch (Exception ex)
			{
				_notyf.Error("Lỗi khi tạo thanh toán VNPay: " + ex.Message);
				return RedirectToAction("Cart");
			}
		}

		[HttpGet]
		public IActionResult PaymentCallback()
		{
			
			var response = _vnPayService.PaymentExecute(Request.Query);

			if (response.Success)
			{

				var order = _context.Orders
						.Where(o => o.OrderStatus == 1) 
						.OrderByDescending(o => o.CreatedDate)
						.FirstOrDefault();

				if (order != null)
				{
					// 2. Cập nhật trạng thái đơn hàng
					order.PaymentMethod = 1; // VNPay
					order.OrderStatus = 4; // Đã thanh toán
					order.ModifiedDate = DateTime.Now;
					_context.SaveChanges();

					
					ClearCart();

					// 4. Hiển thị thông báo
					_notyf.Success("Thanh toán VNPay thành công");
					return RedirectToAction("Index", "Order", new { code = order.Code });
				}
			}

			_notyf.Error("Thanh toán VNPay thất bại: " + response);
			return RedirectToAction("Cart");
		}

	}
}