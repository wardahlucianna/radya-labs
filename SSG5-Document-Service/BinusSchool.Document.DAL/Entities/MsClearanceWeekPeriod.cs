using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsClearanceWeekPeriod : AuditEntity, IDocumentEntity
    {
        public string IdBLPGroup { get; set; }
        public string IdSurveyPeriod { get; set; }
        public int WeekID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual LtBLPGroup BLPGroup { get; set; }
        public virtual MsSurveyPeriod SurveyPeriod { get; set; }
        public virtual ICollection<MsRespondent> Respondents { get; set; }
        public virtual ICollection<TrSurveyStudentAnswer> SurveyStudentAnswers { get; set; }
        public virtual ICollection<HTrSurveyStudentAnswer> HSurveyStudentAnswers { get; set; }
        public virtual ICollection<TrBLPEmailSentLog> BLPEmailSentLogs { get; set; }
    }

    internal class MsClearanceWeekPeriodConfiguration : AuditEntityConfiguration<MsClearanceWeekPeriod>
    {
        public override void Configure(EntityTypeBuilder<MsClearanceWeekPeriod> builder)
        {
            builder.Property(x => x.IdBLPGroup)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdSurveyPeriod)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.WeekID)
                .IsRequired();

            builder.Property(x => x.StartDate)
                .HasColumnType(typeName: "datetime2")
                .IsRequired();

            builder.Property(x => x.EndDate)
               .HasColumnType(typeName: "datetime2")
               .IsRequired();

            builder.HasOne(x => x.BLPGroup)
                .WithMany(x => x.BLPWeekPeriods)
                .HasForeignKey(fk => fk.IdBLPGroup)
                .HasConstraintName("FK_MsClearanceWeekPeriod_LtBLPGroup")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.SurveyPeriod)
                .WithMany(x => x.ClearanceWeekPeriods)
                .HasForeignKey(fk => fk.IdSurveyPeriod)
                .HasConstraintName("FK_MsClearanceWeekPeriod_MsSurveyPeriod")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
