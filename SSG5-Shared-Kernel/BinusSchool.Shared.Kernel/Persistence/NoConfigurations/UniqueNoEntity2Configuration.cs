using BinusSchool.Domain.Abstractions;
using BinusSchool.Domain.NoEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.NoConfigurations
{
    public class UniqueNoEntity2Configuration<TBase> : IEntityTypeConfiguration<TBase> where TBase : UniqueNoEntity2
    {
        public virtual void Configure(EntityTypeBuilder<TBase> builder)
        {
            builder.HasKey(x => x.RowKey);
            builder.HasPartitionKey(x => x.PartitionKey);
            builder.HasNoDiscriminator();

            builder.Property(x => x.RowKey)
                .IsRequired();

            builder.Property(x => x.PartitionKey)
                .IsRequired();
        }
    }
}