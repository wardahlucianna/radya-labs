using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularNoAttDate : AuditEntity, ISchedulingEntity
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public virtual ICollection<MsExtracurricularNoAttDateMapping> ExtracurricularNoAttDateMappings { get; set; }
    }

    internal class MsExtracurricularNoAttDateConfiguration : AuditEntityConfiguration<MsExtracurricularNoAttDate>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularNoAttDate> builder)
        {
            builder.Property(x => x.Description)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.StartDate)
                .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.EndDate)
                .HasColumnType(typeName: "datetime2");

            base.Configure(builder);
        }
    }
}
