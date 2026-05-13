namespace E_commerce_Website__Skincare_.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public string UserId { get; set; } // nullable

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public Product Product { get; set; }

    }
}
