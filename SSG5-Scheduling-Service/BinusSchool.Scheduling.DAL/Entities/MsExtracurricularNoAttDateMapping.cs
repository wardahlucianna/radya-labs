using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularNoAttDateMapping : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricular { get; set; }
        public string IdExtracurricularNoAttDate { get; set; }
        public virtual MsExtracurricular Extracurricular { get; set; }
        public virtual MsExtracurricularNoAttDate ExtracurricularNoAttDate { get; set; }

    }

    internal class MsExtracurricularNoAttDateMappingConfiguration : AuditEntityConfiguration<MsExtracurricularNoAttDateMapping>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularNoAttDateMapping> builder)
        {
            builder.Property(x => x.IdExtracurricular)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdExtracurricularNoAttDate)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.Extracurricular)
                 .WithMany(y => y.ExtracurricularNoAttDateMappings)
                 .HasForeignKey(fk => fk.IdExtracurricular)
                 .HasConstraintName("FK_MsExtracurricularSessionMapping_MsExtracurricular")
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ExtracurricularNoAttDate)
                .WithMany(x => x.ExtracurricularNoAttDateMappings)
                .HasForeignKey(fk => fk.IdExtracurricularNoAttDate)
                .HasConstraintName("FK_MsExtracurricularSessionMapping_MsExtracurricularNoAttDate")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
