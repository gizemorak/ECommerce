
using System.Net;

namespace OrderDomain.Orders
{
    public class Order : Entity, IAggregateRoot
    {
        protected Order() : base() { }

        private readonly List<OrderItem> _orderItems;

        public Order(Guid UserId, Address shiptoAddress, List<OrderItem> orderItems)
        {
            _orderItems = orderItems;
            BuyerId = UserId;
            ShiptoAddress = shiptoAddress;
            CreatedDate = DateTime.UtcNow;
   
        
        }

        public Address ShiptoAddress { get; private set; }

        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

        public DateTime CreatedDate { get; set; }

        public Guid BuyerId { get; set; }
        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; }

        public Guid? PaymentId { get; set; }

        public void SetPaidStatus(Guid paymentId)
        {
            Status = OrderStatus.Completed;
            PaymentId = paymentId;
        }

        public DateTime PaymentDueAtUtc { get; set; }
        public static Order CreateNewOrder(Guid UserId, Address shiptoAddress, List<OrderItem> orderItems)
        {
            var order = new Order(UserId, shiptoAddress, orderItems);
            order._orderItems.AddRange(orderItems);
            order.TotalPrice = orderItems.Sum(x=>x.Price);



            return order;
        }
    }
}
