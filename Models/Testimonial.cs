using System.ComponentModel.DataAnnotations;

namespace E_commerce_Website__Skincare_.Models
{
    public class Testimonial
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required]
        public string Content { get; set; }

        public bool IsApproved { get; set; }

        public ApplicationUser User { get; set; }

    }
}
