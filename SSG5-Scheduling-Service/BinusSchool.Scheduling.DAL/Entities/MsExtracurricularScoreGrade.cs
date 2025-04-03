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
    public class MsExtracurricularScoreGrade : AuditEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public string IdExtracurricularScoreCalculationType { get; set; }
        public decimal MinScore { get; set; }
        public decimal MaxScore { get; set; }
        public string Grade { get; set; }

        public virtual MsSchool School { get; set; }
        public virtual MsExtracurricularScoreCalculationType ExtracurricularScoreCalculationType { get; set; }
    }

    internal class MsExtracurricularScoreGradeConfiguration : AuditEntityConfiguration<MsExtracurricularScoreGrade>
    {
        public override void Configure(EntityTypeBuilder<MsExtracurricularScoreGrade> builder)
        {
            builder.Property(x => x.IdSchool)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdExtracurricularScoreCalculationType)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.MinScore)
                .HasColumnType("DECIMAL(5,2)")
                .IsRequired();

            builder.Property(x => x.MaxScore)
                .HasColumnType("DECIMAL(5,2)")
                .IsRequired();

            builder.Property(x => x.Grade)
                .HasMaxLength(128)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.ExtracurricularScoreGrades)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsExtracurricularScoreGrade_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.ExtracurricularScoreCalculationType)
                .WithMany(x => x.ExtracurricularScoreGrades)
                .HasForeignKey(fk => fk.IdExtracurricularScoreCalculationType)
                .HasConstraintName("FK_MsExtracurricularScoreGrade_MsExtracurricularScoreCalculationType")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
