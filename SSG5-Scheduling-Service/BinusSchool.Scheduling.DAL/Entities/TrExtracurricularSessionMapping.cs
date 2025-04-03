using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrExtracurricularSessionMapping : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricular { get; set; }
        public string IdExtracurricularSession { get; set; }

        public virtual MsExtracurricular Extracurricular { get; set; }
        public virtual MsExtracurricularSession ExtracurricularSession { get; set; }
    }
    internal class TrExtracurricularSessionMappingConfiguration : AuditEntityConfiguration<TrExtracurricularSessionMapping>
    {
        public override void Configure(EntityTypeBuilder<TrExtracurricularSessionMapping> builder)
        {
            builder.Property(p => p.Id)
                .IsRequired();

            builder.Property(p => p.IdExtracurricular)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdExtracurricularSession)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Extracurricular)
                .WithMany(x => x.ExtracurricularSessionMappings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricular)
                .HasConstraintName("FK_TrExtracurricularSessionMapping_MsExtracurricular")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ExtracurricularSession)
                .WithMany(x => x.ExtracurricularSessionMappings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExtracurricularSession)
                .HasConstraintName("FK_TrExtracurricularSessionMapping_MsExtracurricularSession")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);

        }
    }

}
