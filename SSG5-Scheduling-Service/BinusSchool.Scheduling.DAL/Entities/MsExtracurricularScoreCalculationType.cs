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
    public class MsExtracurricularScoreCalculationType : AuditEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public string CalculationType { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsExtracurricularScoreCompCategory> ExtracurricularScoreCompCategories { get; set; }
        public virtual ICollection<MsExtracurricularScoreGrade> ExtracurricularScoreGrades { get; set; }
    }

    internal class MsExtracurricularScoreCalculationTypeConfiguration : AuditEntityConfiguration<MsExtracurricularScoreCalculationType>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularScoreCalculationType> builder)
        {
            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.CalculationType)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.ExtracurricularScoreCalculationTypes)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsExtracurricularScoreCalculationType_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
