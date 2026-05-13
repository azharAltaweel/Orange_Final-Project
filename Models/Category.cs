using System.ComponentModel.DataAnnotations;

namespace E_commerce_Website__Skincare_.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string ImageUrl { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
