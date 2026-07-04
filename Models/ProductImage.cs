using System.ComponentModel.DataAnnotations;

namespace Jumla

    .Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

    }
}
