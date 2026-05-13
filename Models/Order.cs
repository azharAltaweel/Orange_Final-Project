using System.ComponentModel.DataAnnotations.Schema;

namespace E_commerce_Website__Skincare_.Models
{
    public enum OrderStatus
    {
        Processing,
        Completed,
        Cancelled
    }
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } // Processing, Completed, Cancelled

        public ApplicationUser User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
