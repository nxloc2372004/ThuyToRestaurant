using Microsoft.AspNetCore.Mvc;
using ThuyTo.Models;

namespace ThuyTo.Controllers
{
    public class ResrevationController : Controller
    {
        private readonly ThuyToContext thuyToContext;
        public ResrevationController(ThuyToContext context)
        {
            thuyToContext = context;
        }
        [HttpPost]
        [Route("/SaveReservation")]
        public IActionResult SaveReservation([FromBody] Reservation reservation)
        {
            var existingReservation = thuyToContext.Reservations.Find(reservation.ReservationId);
            if (existingReservation == null || existingReservation.Status != ReservationStatus.Available)
            {
                return BadRequest("Bàn không khả dụng.");
            }
            existingReservation.FullName = reservation.FullName;
            existingReservation.Phone = reservation.Phone;
            existingReservation.Email = reservation.Email;
            existingReservation.SpecialRequest = reservation.SpecialRequest;
            existingReservation.ReservationDate = DateTime.Now;
            existingReservation.Status = ReservationStatus.Pending;
            existingReservation.NumberOfGuests = reservation.NumberOfGuests;

            thuyToContext.SaveChanges();

            return Ok("Đặt bàn thành công!");
        }
    }
}
