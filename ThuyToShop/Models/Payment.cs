// Models/Payment.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThuyTo.Models
{
	public class Payment
	{
		[Key]
		public int PaymentId { get; set; }

		[ForeignKey("Orders")]
		public long OrderId { get; set; }

		public int PaymentMethod { get; set; } // 0: COD, 1: VNPay,...
		public decimal Amount { get; set; }
		public DateTime PaymentDate { get; set; }
		public string TransactionId { get; set; }
		public int Status { get; set; } 

		public virtual Order Order { get; set; }
	}
}