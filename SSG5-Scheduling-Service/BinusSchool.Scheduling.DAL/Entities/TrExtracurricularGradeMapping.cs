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
    public class TrExtracurricularGradeMapping : AuditEntity, ISchedulingEntity
    {
        public string IdExtracurricular { get; set; }
        public string IdGrade { get; set; }
        public virtual MsExtracurricular Extracurricular { get; set; }
        public virtual MsGrade Grade { get; set; }
    }

    internal class TrExtracurricularGradeMappingConfiguration : AuditEntityConfiguration<TrExtracurricularGradeMapping>
    {
        public override void Configure(EntityTypeBuilder<TrExtracurricularGradeMapping> builder)
        {

            builder.Property(x => x.IdExtracurricular)
                  .HasMaxLength(36)
                  .IsRequired();

            builder.Property(x => x.IdGrade)
                 .HasMaxLength(36)
                 .IsRequired();

            builder.HasOne(x => x.Extracurricular)
                 .WithMany(y => y.ExtracurricularGradeMappings)
                 .HasForeignKey(fk => fk.IdExtracurricular)
                 .HasConstraintName("FK_TrExtracurricularGradeMapping_MsExtracurricular")
                 .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Grade)
                 .WithMany(y => y.ExtracurricularGradeMappings)
                 .HasForeignKey(fk => fk.IdGrade)
                 .HasConstraintName("FK_TrExtracurricularGradeMapping_MsGrade")
                 .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
