using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
namespace ThuyTo.Models
{
    public class Feedbacks
    {
        [Key]
        public long FeedbackId { get; set; }
        public long ProductId { get; set; }
        public long UserId { get; set; } 
        public int Rating { get; set; }
        public string FeedbackText { get; set; }
        public DateTime CreatedDate { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } // Thêm navigation property
    }
}