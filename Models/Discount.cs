namespace E_commerce_Website__Skincare_.Models
{
    public class Discount
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int Percentage { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
