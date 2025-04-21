using ThuyTo.Models;
using ThuyTo.SessionSystem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PagedList.Core;
using System.Globalization;
using System.Text;
using QuestPDF.Helpers;
using System.Security.Claims;
using Microsoft.AspNet.SignalR;

namespace ThuyTo.Controllers
{
    public class ProductController : Controller
    {
        private readonly ThuyToContext _context;
        private const int PageSize = 9;

        // Giữ nguyên các hằng số session
        public const string RECENTPRODUCT = "RecentProduct";
        public const string FAVOURITEPRODUCT = "FavouriteProduct";

        public ProductController(ThuyToContext context)
        {
            _context = context;
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        public IActionResult Index(int? page, string searchTerm)
        {
            var pageNumber = page ?? 1;
            var pageSize = 9;

            IQueryable<Product> query = _context.Products
                .Where(p => (bool)p.IsActive)
                .Include(p => p.Category);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p =>
                    p.ProductName.Contains(searchTerm));
            }

            var model = new PagedList<Product>(query.OrderBy(p => p.ProductName), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;

            return View(model);
        }
        List<Product> GetProductFromSession(string SessionKey)
        {
            var session = HttpContext.Session;
            string jsonstring = session.GetString(SessionKey);
            if (jsonstring != null)
            {
                return JsonConvert.DeserializeObject<List<Product>>(jsonstring);
            }
            return new List<Product>();
        }

        void ClearSession(string SessionKey)
        {
            var session = HttpContext.Session;
            session.Remove(SessionKey);
        }

        void SaveSession(List<Product> ls, string SessionKey)
        {
            var session = HttpContext.Session;
            string jsonstring = JsonConvert.SerializeObject(ls);
            session.SetString(SessionKey, jsonstring);
        }
        [Route("/san-pham/{slug}-{id:long}")]
        public IActionResult ProductDetail(long id)
        {
            var product = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Feedbacks)
                    .ThenInclude(f => f.User) // Include thông tin người đánh giá
                .FirstOrDefault(m => m.IsDeleted == false && m.IsActive == true && m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            product.ProductViewCount += 1;
            _context.Products.Update(product);
            _context.SaveChanges();

            return View(product);
        }

        [Route("/danh-muc-san-pham/{slug}-{id:long}", Name = "ProductByCategory")]
        public IActionResult ProductByCategory(long id, int? page, string searchTerm)
        {
            var pageNumber = page ?? 1;
            var pageSize = 9;

            // Lấy danh sách sản phẩm active
            var products = _context.Products
                .Where(p => (bool)p.IsActive)
                .Include(p => p.Category)
                .AsEnumerable(); // Chuyển sang client-side để xử lý tìm kiếm không dấu

            // Lọc theo danh mục nếu có id
            if (id != 0)
            {
                products = products.Where(p => p.CategoryId == id);
                var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);
                ViewBag.CategoryName = category?.CategoryName ?? "Danh mục không tồn tại";
            }
            else
            {
                ViewBag.CategoryName = "Tất cả sản phẩm";
            }

            // Xử lý tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var keyword = RemoveDiacritics(searchTerm.ToLower());
                products = products.Where(p =>
                    RemoveDiacritics(p.ProductName.ToLower()).Contains(keyword)
                );
                ViewBag.SearchTerm = searchTerm;
            }

            // Phân trang
            var pagedProducts = products
                .OrderBy(p => p.ProductName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new StaticPagedList<Product>(
                pagedProducts,
                pageNumber,
                pageSize,
                products.Count()
            );

            ViewBag.CurrentPage = pageNumber;
            ViewBag.CategoryId = id;

            return View(model);
        }

        public IActionResult AddToFavouritePorduct(int productid)
        {
            try
            {
                var product = _context.Products.Where(p => p.ProductId == productid).FirstOrDefault();
                if (product == null) return Json(new
                {
                    status = 1,
                    message = "Cannot find this product"
                });
                var listFavouriteProduct = GetProductFromSession(SessionKey.FAVOURITEPRODUCT);
                var IsExists = listFavouriteProduct.Find(p => p.ProductId == product.ProductId);
                var res = "Sản phẩm đã được thêm vào yêu thích";
                if (IsExists != null)
                {
                    listFavouriteProduct.Remove(IsExists);
                    res = "Đã xóa sản phẩm khỏi danh sách sản phẩm yêu thích";
                }
                listFavouriteProduct.Add(product);
                SaveSession(listFavouriteProduct, SessionKey.FAVOURITEPRODUCT);
                return Json(new
                {
                    status = 0,
                    message = res
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
        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromBody] Feedbacks feedback)
        {
            if (feedback == null || feedback.ProductId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            // Lấy UserId từ Session và chuyển sang long
            var userId = HttpContext.Session.GetInt32(SessionKey.USERID);
            if (userId == null)
            {
                return Json(new { success = false, message = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.", loginRequired = true });
            }

            long userIdLong = (long)userId;

            try
            {
                // Tìm sản phẩm
                var product = await _context.Products.FindAsync((long)feedback.ProductId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
                }

                // Kiểm tra nếu user đã đánh giá sản phẩm
                var existingFeedback = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.ProductId == feedback.ProductId && f.UserId == userIdLong);

                if (existingFeedback != null)
                {
                    return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này rồi." });
                }

                // Tạo feedback mới
                var newFeedback = new Feedbacks
                {
                    ProductId = feedback.ProductId,
                    UserId = userIdLong,
                    Rating = feedback.Rating,
                    FeedbackText = feedback.FeedbackText.Trim(),
                    CreatedDate = DateTime.Now
                };

                _context.Feedbacks.Add(newFeedback);
                await _context.SaveChangesAsync();

                // Lấy thông tin user để hiển thị
                var user = await _context.Users.FindAsync(userIdLong);
                var userFullName = user?.FullName ?? "Khách hàng";

                return Json(new
                {
                    success = true,
                    message = "Cảm ơn bạn đã gửi đánh giá!",
                    data = new
                    {
                        fullName = userFullName,
                        rating = newFeedback.Rating,
                        feedbackText = newFeedback.FeedbackText,
                        createdDate = newFeedback.CreatedDate.ToString("dd/MM/yyyy")
                    }
                });
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần thiết
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }


    }
}