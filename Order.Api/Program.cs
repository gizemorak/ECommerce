using Bus.Shared.Options;
using Order.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// register services via extension
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// configure middleware and endpoints via extension
app.UseApi();

app.Run();