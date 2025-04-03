using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Common.Model.Enums;
using System;
using Azure.Storage.Blobs.Models;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrSurvey : AuditEntity, ISchoolEntity
    {
        public string IdSurveyChild { get; set; }
        public string IdUser { get; set; }
        public string IdHomeroomStudent { get; set; }
        public MySurveyStatus Status { get; set; }
        public string IdSurveyTemplateChild { get; set; }
        public string IdPublishSurvey { get; set; }
        public bool IsAllInOne { get; set; }
        public string IdGeneratedAllInOne { get; set; }
        public virtual TrPublishSurvey PublishSurvey { get; set; }
        public virtual MsUser User { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
    }

    internal class TrSurveyConfiguration : AuditEntityConfiguration<TrSurvey>
    {
        public override void Configure(EntityTypeBuilder<TrSurvey> builder)
        {
            builder.Property(e => e.Status).HasMaxLength(maxLength: 10)
              .HasConversion(valueToDb =>
                      valueToDb.ToString(),
                  valueFromDb =>
                      (MySurveyStatus)Enum.Parse(typeof(MySurveyStatus), valueFromDb))
              .IsRequired();


            builder.HasOne(x => x.PublishSurvey)
              .WithMany(x => x.Surveys)
              .HasForeignKey(fk => fk.IdPublishSurvey)
              .HasConstraintName("FK_TrSurvey_TrPublishSurvey")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.User)
              .WithMany(x => x.Surveys)
              .HasForeignKey(fk => fk.IdUser)
              .HasConstraintName("FK_TrSurvey_MsUser")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
              .WithMany(x => x.Surveys)
              .HasForeignKey(fk => fk.IdHomeroomStudent)
              .HasConstraintName("FK_TrSurvey_MsHomeroomStudent")
              .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
