using System;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.Entities
{
    /// <summary>
    /// Entity base to hold auditrail information with no Id and Primary Key
    /// </summary>
    public abstract class AuditNoUniqueEntity : ActiveEntity, IAuditable
    {
        public string UserIn { get; set; }
        public DateTime? DateIn { get; set; }
        public string UserUp { get; set; }
        public DateTime? DateUp { get; set; }
    }

    public class AuditNoUniqueEntityConfiguration<T> : ActiveEntityConfiguration<T> where T : AuditNoUniqueEntity, IEntity
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(p => p.UserIn)
                .HasMaxLength(maxLength: 36)
                .IsRequired();
            
            builder.Property(p => p.DateIn)
                .IsRequired();
            
            builder.Property(p => p.UserUp)
                .HasMaxLength(maxLength: 36);
            
            base.Configure(builder);
        }
    }
}
