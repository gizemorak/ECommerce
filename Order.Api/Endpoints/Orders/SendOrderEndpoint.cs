using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using OrderApplication.Orders.Commands.CreateOrder;

namespace Order.Api.Endpoints.Orders;

public static class SendOrderEndpoint
{
 public static void MapSendOrder(this RouteGroupBuilder group)
 {
        group.MapPost("/send", Handler)
        .MapToApiVersion(1.0)
        .WithName("SendOrder")
        .WithOpenApi()
        .WithSummary("Send a new order")
        .WithDescription("Creates and sends a new order for processing");
 // Remove this line: .HasApiVersion(1, 0)
 //.RequireAuthorization();
 }

 private static async Task<IResult> Handler(CreateOrderCommand request, IMediator mediator)
 {
 var result = await mediator.Send(request);
 return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Fail);
 }
}
