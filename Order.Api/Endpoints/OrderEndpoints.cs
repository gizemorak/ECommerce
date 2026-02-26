using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Order.Api.Endpoints.Orders;

namespace Order.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var versionedApi = app.NewVersionedApi();

        var group = versionedApi
            .MapGroup("/api/v{version:apiVersion}/orders")
            .WithTags("Orders");

        group.MapSendOrder();
        group.MapCancelOrder();
    }
}
