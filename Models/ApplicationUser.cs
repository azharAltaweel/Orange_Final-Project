using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace E_commerce_Website__Skincare_.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<Wishlist>? Wishlists { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
        public virtual ICollection<Testimonial>? Testimonials { get; set; }

    }
}
