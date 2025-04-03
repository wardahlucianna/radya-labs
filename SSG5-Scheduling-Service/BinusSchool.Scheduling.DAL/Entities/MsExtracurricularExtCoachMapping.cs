using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsExtracurricularExtCoachMapping : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricular { set; get; }
        public string IdExtracurricularExternalCoach { set; get; }
        public virtual MsExtracurricularExternalCoach ExtracurricularExternalCoach { get; set; }
        public virtual MsExtracurricular Extracurricular { get; set; }
    }
    internal class MsExtracurricularExtCoachMappingConfiguration : AuditEntityConfiguration<MsExtracurricularExtCoachMapping>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularExtCoachMapping> builder)
        {

            builder.Property(p => p.IdExtracurricular)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdExtracurricularExternalCoach)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Extracurricular)
                .WithMany(x => x.ExtracurricularExtCoachMappings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricular)
                .HasConstraintName("FK_MsExtracurricularExtCoachMapping_MsExtracurricular")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ExtracurricularExternalCoach)
                .WithMany(x => x.ExtracurricularExtCoachMappings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricularExternalCoach)
                .HasConstraintName("FK_MsExtracurricularExtCoachMapping_MsExtracurricularExternalCoach")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);

        }
    }
}
