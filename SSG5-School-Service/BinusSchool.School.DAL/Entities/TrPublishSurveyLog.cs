using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrPublishSurveyLog : AuditEntity, ISchoolEntity
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsDone { get; set; }
        public bool IsError { get; set; }
        public bool IsProcess { get; set; }
        public string ErrorMessage { get; set; }
        public string IdPublishSurvey { get; set; }
        public PublishSurveyLogType Type { get; set; }
        public TrPublishSurvey PublishSurvey { get; set; }
    }

    internal class TrPublishSurveyLogConfiguration : AuditEntityConfiguration<TrPublishSurveyLog>
    {
        public override void Configure(EntityTypeBuilder<TrPublishSurveyLog> builder)
        {
            builder.Property(x => x.Type)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(e => e.Type).HasMaxLength(maxLength: 10)
             .HasConversion(valueToDb =>
                     valueToDb.ToString(),
                 valueFromDb =>
                     (PublishSurveyLogType)Enum.Parse(typeof(PublishSurveyLogType), valueFromDb))
             .IsRequired();

            builder.HasOne(x => x.PublishSurvey)
               .WithMany(x => x.PublishSurveyLog)
               .HasForeignKey(fk => fk.IdPublishSurvey)
               .HasConstraintName("FK_TrPublishSurveyLog_TrPublishSurvey")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
