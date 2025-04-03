using System;
using BinusSchool.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.Entities
{
    /// <summary>
    /// Entity base for all entities to indicate soft delete state
    /// </summary>
    public class AuditIsActiveEntity
    {
        public string IsActive { get; set; }
    }

    public class ExceptionActiveEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : AuditIsActiveEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(p => p.IsActive)
                .HasColumnName("IsActive");

            builder.HasIndex(p => p.IsActive);
        }
    }
}
