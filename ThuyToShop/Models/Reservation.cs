using System.ComponentModel.DataAnnotations;

namespace ThuyTo.Models
{
    public enum ReservationStatus
    {
        Available,  // Bàn trống
        Pending,    // Đặt bàn mới
        Confirmed,  // Đã xác nhận
        Cancelled   // Đã hủy
    }

    public class Reservation
    {
        public int ReservationId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? ReservationDate { get; set; }
        public int NumberOfGuests { get; set; }
        public string? SpecialRequest { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public ReservationStatus? Status { get; set; } = ReservationStatus.Available; // Mặc định là Available
    }
}
