using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Entities.School;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsSurveyPeriod : AuditEntity, IDocumentEntity
    {
        public string IdSurveyCategory { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool CustomSchedule { get; set; }

        public virtual LtSurveyCategory SurveyCategory { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<MsConsentCustomSchedule> ConsentCustomSchedules { get; set; }
        public virtual ICollection<MsRespondent> Respondents { get; set; }
        public virtual ICollection<MsClearanceWeekPeriod> ClearanceWeekPeriods { get; set; }
        public virtual ICollection<TrBLPEmailSentLog> BLPEmailSentLogs { get; set; }
    }

    internal class MsSurveyPeriodConfiguration : AuditEntityConfiguration<MsSurveyPeriod>
    {
        public override void Configure(EntityTypeBuilder<MsSurveyPeriod> builder)
        {
            builder.Property(x => x.IdSurveyCategory)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdGrade)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Semester)
                .IsRequired();

            builder.Property(x => x.StartDate)
                .HasColumnType(typeName: "datetime2")
                .IsRequired();

            builder.Property(x => x.EndDate)
                .HasColumnType(typeName: "datetime2")
                .IsRequired();

            builder.Property(x => x.CustomSchedule)
                .IsRequired();

            builder.HasOne(x => x.SurveyCategory)
                .WithMany(x => x.SurveyPeriods)
                .HasForeignKey(fk => fk.IdSurveyCategory)
                .HasConstraintName("FK_MsSurveyPeriod_LtSurveyCategory")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.SurveyPeriods)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsSurveyPeriod_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
