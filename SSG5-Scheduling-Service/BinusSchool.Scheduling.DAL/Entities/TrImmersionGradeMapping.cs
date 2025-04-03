using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrImmersionGradeMapping : AuditEntity, ISchedulingEntity
    {
        public string IdImmersion { get; set; }
        public string IdGrade { get; set; }
        public virtual MsImmersion Immersion { get; set; }
        public virtual MsGrade Grade { get; set; }
    }

    internal class TrImmersionGradeMappingConfiguration : AuditEntityConfiguration<TrImmersionGradeMapping>
    {
        public override void Configure(EntityTypeBuilder<TrImmersionGradeMapping> builder)
        {
            builder.Property(x => x.IdImmersion)
             .HasMaxLength(36)
             .IsRequired();

            builder.Property(x => x.IdGrade)
             .HasMaxLength(36)
             .IsRequired();

            builder.HasOne(x => x.Immersion)
                 .WithMany(y => y.ImmersionGradeMappings)
                 .HasForeignKey(fk => fk.IdImmersion)
                 .HasConstraintName("FK_TrImmersionGradeMapping_MsImmersion")
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Grade)
                 .WithMany(y => y.ImmersionGradeMappings)
                 .HasForeignKey(fk => fk.IdGrade)
                 .HasConstraintName("FK_TrImmersionGradeMapping_MsGrade")
                 .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
