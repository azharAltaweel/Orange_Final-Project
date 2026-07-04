using System.ComponentModel.DataAnnotations.Schema;

namespace Jumla
    .Models
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

        public OrderStatus Status { get; set; } = OrderStatus.Processing;

        public bool IsPaid { get; set; } = false;
        public string DeliveryAddress { get; set; }
        public string Notes { get; set; }
        public ApplicationUser User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
