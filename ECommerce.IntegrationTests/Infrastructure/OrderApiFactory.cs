using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ECommerce.IntegrationTests.Infrastructure;

public sealed class OrderApiFactory : WebApplicationFactory<global::Program>
{
 private readonly string _connectionString;

 public OrderApiFactory(string connectionString)
 {
 _connectionString = connectionString;
 }

 protected override void ConfigureWebHost(IWebHostBuilder builder)
 {
 builder.ConfigureAppConfiguration((_, config) =>
 {
 var dict = new Dictionary<string, string?>
 {
 // Order.Api reads ConnectionStrings:"ecommercedb".
 ["ConnectionStrings:ecommercedb"] = _connectionString
 };

 config.AddInMemoryCollection(dict);
 });
 }
}
