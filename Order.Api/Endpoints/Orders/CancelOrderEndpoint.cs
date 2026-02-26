using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using OrderApplication.Orders.Commands.CancelOrder;

namespace Order.Api.Endpoints.Orders;

public static class CancelOrderEndpoint
{
    public static void MapCancelOrder(this RouteGroupBuilder group)
    {
        group.MapPost("/cancel", Handler)
            .MapToApiVersion(1.0)
            .WithName("CancelOrder")
            .WithOpenApi()
            .WithSummary("Cancel an existing order")
            .WithDescription("Cancels an order that was previously created")
            .RequireAuthorization();
    }

    private static async Task<IResult> Handler(CancelOrderCommand request, IMediator mediator)
    {
        var result = await mediator.Send(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Fail);
    }
}
