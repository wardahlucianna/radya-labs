using System;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Domain.NoEntities;
using BinusSchool.Persistence.AuditDb.Abstractions;
using BinusSchool.Persistence.NoConfigurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AuditDb.NoEntities
{
    public class HsAuditTrail : UniqueNoEntity2, IAuditNoEntity
    {
        public DateTime Time { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }
        public string Executor { get; set; }
        public AuditAction Action { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    internal class HsAuditTrailConfiguration : UniqueNoEntity2Configuration<HsAuditTrail>
    {
        public override void Configure(EntityTypeBuilder<HsAuditTrail> builder)
        {
            builder.Property(x => x.Time).IsRequired();

            builder.Property(x => x.Table).IsRequired();
            
            builder.Property(x => x.Column).IsRequired();

            builder.Property(x => x.Executor).IsRequired();

            builder.Property(x => x.Action)
                .HasConversion<string>()
                .IsRequired();

            base.Configure(builder);
        }
    }
}