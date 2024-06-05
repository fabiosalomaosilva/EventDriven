namespace BillingService.Models
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public required string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
        public DateTime CreatedAt { get; set; }
    }
}
