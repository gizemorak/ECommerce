using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using Testcontainers.MsSql;
using Xunit;

namespace ECommerce.IntegrationTests.Infrastructure;

public sealed class MsSqlContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container;

    public string ConnectionString => _container.GetConnectionString();

    public MsSqlContainerFixture()
    {
        _container = new MsSqlBuilder()
            .WithPassword("yourStrong(!)Password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
