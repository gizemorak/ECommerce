using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderPersistence;

namespace ECommerce.IntegrationTests.Infrastructure;

internal static class TestDbMigrator
{
    internal static async Task MigrateAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    internal static async Task<T> QueryAsync<T>(IServiceProvider services, Func<ApplicationDbContext, Task<T>> query)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await query(db);
    }
}
