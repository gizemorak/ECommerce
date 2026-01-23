using MediatR;
using OrderApplication.Orders.DTOs;
using OrderApplicationOrders.Commands.CreateOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderApplication.Orders.Queries.GetOrder
{
    public sealed  record GetByIdOrderCommand(int OrderId) : IRequestByServiceResult;

   

}
