using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrBLPEmailSentLog : AuditEntity, IDocumentEntity
    {
        public string IdStudent { get; set; }
        public string IdSurveyPeriod { get; set; }
        public string IdClearanceWeekPeriod { get; set; }
        public string EmailSubject { get; set; }
        public string HTMLDescription { get; set; }
        public string PrimaryToAddress { get; set; }
        public string AdditionalToAddress { get; set; }
        public string AdditionalCCAddress { get; set; }
        public string AdditionalBCCAddress { get; set; }
        public DateTime? ResendDate { get; set; }

        public virtual MsStudent Student { get; set; }
        public virtual MsSurveyPeriod SurveyPeriod { get; set; }
        public virtual MsClearanceWeekPeriod ClearanceWeekPeriod { get; set; }
    }

    internal class TrBLPEmailSentLogConfiguration : AuditEntityConfiguration<TrBLPEmailSentLog>
    {
        public override void Configure(EntityTypeBuilder<TrBLPEmailSentLog> builder)
        {
            builder.Property(p => p.IdStudent)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdSurveyPeriod)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(p => p.IdClearanceWeekPeriod)
                .HasMaxLength(36);

            builder.Property(p => p.EmailSubject)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.HTMLDescription)
                .IsRequired();

            builder.Property(p => p.PrimaryToAddress)
                .HasMaxLength(128)
                .IsRequired();

            //builder.Property(p => p.AdditionalToAddress)
            //    .HasMaxLength(128);

            //builder.Property(p => p.AdditionalCCAddress)
            //    .HasMaxLength(128);

            //builder.Property(p => p.AdditionalBCCAddress)
            //    .HasMaxLength(128);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.BLPEmailSentLogs)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrBLPEmailSentLog_MsStudent")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.SurveyPeriod)
                .WithMany(x => x.BLPEmailSentLogs)
                .HasForeignKey(fk => fk.IdSurveyPeriod)
                .HasConstraintName("FK_TrBLPEmailSentLog_MsSurveyPeriod")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ClearanceWeekPeriod)
                .WithMany(x => x.BLPEmailSentLogs)
                .HasForeignKey(fk => fk.IdClearanceWeekPeriod)
                .HasConstraintName("FK_TrBLPEmailSentLog_MsClearanceWeekPeriod")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
