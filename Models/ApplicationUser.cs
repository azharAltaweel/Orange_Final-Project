using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Jumla.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        [Required]
        [StringLength(150)]
        public string BusinessName { get; set; }
        public string? Address { get; set; }

        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<Wishlist>? Wishlists { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
        public virtual ICollection<Testimonial>? Testimonials { get; set; }

    }
}
