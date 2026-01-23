using MediatR;
using OrderApplication.Orders.DTOs;
using OrderApplicationOrders.Commands.CreateOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Orders.Commands.CreateOrder
{
    public sealed  record CreateOrderCommand(Guid UserId, AdressDto adressdto,List<OrderItemDto> OrderItems, PaymentDto Payment) : IRequestByServiceResult;

    public record PaymentDto(string CardNumber, string CardHolderName, string Expiration, string Cvc, decimal Amount);

}
