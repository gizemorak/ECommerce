using Bus.Shared.Options;
using Microsoft.EntityFrameworkCore;
using Order.Api.Extensions;
using OrderPersistence;
using System;

var builder = WebApplication.CreateBuilder(args);

// register services via extension
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddSingleton<IKeyedServiceProvider>(provider => (IKeyedServiceProvider)provider);


builder.AddKafkaProducer<string, string>("kafka");
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
   // applies pending migrations
}

// configure middleware and endpoints via extension
app.UseApi();

app.Run();