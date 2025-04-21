using Microsoft.AspNetCore.Mvc;
using ThuyTo.Models;

namespace ThuyTo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminReservationController : Controller
    {
        private readonly ThuyToContext _thuyToContext;

        public AdminReservationController(ThuyToContext thuyToContext)
        {
            _thuyToContext = thuyToContext;
        }
        public IActionResult Index()
        {
            var reservations = _thuyToContext.Reservations.ToList();
            return View(reservations);
        }

        [HttpPost]
        [Route("/UpdateReservationStatus")]
        public JsonResult UpdateReservationStatus([FromBody] reservationSent data)
        {
            try
            {
                var reservation = _thuyToContext.Reservations.Find(data.reservationId);
                if (reservation != null)
                {
                    reservation.Status = Enum.Parse<ReservationStatus>(data.status);

                    if(reservation.Status == ReservationStatus.Available)
                    {
                        reservation.FullName = null;
                        reservation.Phone = null;
                        reservation.Email = null;
                        reservation.ReservationDate = null;
                        reservation.NumberOfGuests = 0;
                        reservation.SpecialRequest = null;
                    }
                    _thuyToContext.SaveChanges();

                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy đặt bàn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class reservationSent
    {
        public int reservationId {  get; set; }
        public string? status { get; set; }
    }
}
