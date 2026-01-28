using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderDomain;
using OrderDomain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderPersistence.Configurations
{
    internal class IdempotencyConfiguration : IEntityTypeConfiguration<Idempotency>
    {
        public void Configure(EntityTypeBuilder<Idempotency> builder)
        {
            builder.HasKey(o => o.Key);
        }
    }
}
