using OrderApplication.Orders.Commands.CreateOrder;

namespace OrderApplication.Orders.DTOs
{
    public class OrderItemDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public Decimal Price { get; set; }
    }
}