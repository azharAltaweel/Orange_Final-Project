using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jumla.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
            [StringLength(150)]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal WholesalePrice { get; set; }

        public bool IsActive { get; set; } = true;
        public int MinOrderQuantity { get; set; }
        public string Unit { get; set; } // "كرتون", "كيلو", "قطعة"
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int? DiscountId { get; set; }

        public Discount? Discount { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }

    }
}
