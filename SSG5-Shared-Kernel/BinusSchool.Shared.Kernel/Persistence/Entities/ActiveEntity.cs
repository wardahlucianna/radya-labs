using System;
using BinusSchool.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.Entities
{
    /// <summary>
    /// Entity base for all entities to indicate soft delete state
    /// </summary>
    public abstract class ActiveEntity : IActiveState
    {
        public bool IsActive { get; set; }
    }

    public class ActiveEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : ActiveEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(p => p.IsActive)
                .HasColumnName("Stsrc");

            builder.HasIndex(p => p.IsActive);

            var isFnSync = Environment.GetEnvironmentVariable("ISRUNFROMSYNCFUNCTION");
            if (isFnSync is null)
                builder.HasQueryFilter(p => p.IsActive);
        }
    }
}
