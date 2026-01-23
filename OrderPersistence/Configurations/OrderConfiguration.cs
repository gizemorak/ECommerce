
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderDomain.Orders;

namespace OrderPersistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.BuyerId)
            .IsRequired();

        builder.Property(o => o.CreatedDate)
            .IsRequired();

        builder.Property(o => o.TotalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        //builder.Property(o => o.OrderStatus)
        //    .IsRequired();

        builder.OwnsOne(o => o.ShiptoAddress, sa =>
        {
            sa.Property(a => a.Street).IsRequired();
            sa.Property(a => a.City).IsRequired();
            sa.Property(a => a.State).IsRequired();
            sa.Property(a => a.Country).IsRequired();
            sa.Property(a => a.ZipCode).IsRequired();
        });

        builder.HasMany<OrderItem>("_orderItems")
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired();

        builder.Ignore(o => o.OrderItems);
    }
}
