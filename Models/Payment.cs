namespace E_commerce_Website__Skincare_.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string PaymentMethod { get; set; }

        public string TransactionId { get; set; }

        public bool IsPaid { get; set; }

        public Order Order { get; set; }
    }
}
