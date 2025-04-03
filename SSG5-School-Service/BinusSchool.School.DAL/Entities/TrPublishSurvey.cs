using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.SchoolDb.Abstractions;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrPublishSurvey : AuditEntity, ISchoolEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string Title { get; set; }
        public PublishSurveyType SurveyType { get; set; }
        public string Description { get; set; }
        public string IdSurveyTemplate { get; set; }
        public string IdSurveyTemplateChild { get; set; }
        public string IdPublishSurveyLink { get; set; }
        public string IdSurveyTemplateLink { get; set; }
        public string IdSurveyTemplateChildLink { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsGrapicExtender { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsEntryOneTime { get; set; }
        public PublishSurveySubmissionOption? SubmissionOption { get; set; }
        public string AboveSubmitText { get; set; }
        public string ThankYouMessage { get; set; }
        public string AfterSurveyCloseText { get; set; }
        public PublishSurveyStatus Status { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsSurveyTemplate SurveyTemplate { get; set; }
        public virtual MsSurveyTemplate SurveyTemplateLink { get; set; }
        public virtual TrPublishSurvey PublishSurveyLink { get; set; }
        public virtual ICollection<TrSurvey> Surveys { get; set; }
        public virtual ICollection<TrPublishSurveyRespondent> PublishSurveyRespondents { get; set; }
        public virtual ICollection<TrPublishSurveyMapping> PublishSurveyMappings { get; set; }
        public virtual ICollection<TrPublishSurvey> PublishSurveyLinks { get; set; }
        public virtual ICollection<TrPublishSurveyLog> PublishSurveyLog { get; set; }
    }

    internal class TrPublishSurveyConfiguration : AuditEntityConfiguration<TrPublishSurvey>
    {
        public override void Configure(EntityTypeBuilder<TrPublishSurvey> builder)
        {
            builder.Property(x => x.Semester)
                .IsRequired();

            builder.Property(x => x.IdSurveyTemplateChild)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdSurveyTemplateChildLink)
                .HasMaxLength(36);

            builder.Property(x => x.Title)
                .HasMaxLength(1054)
                .IsRequired();

            builder.Property(e => e.SurveyType).HasMaxLength(maxLength: 100)
              .HasConversion(valueToDb =>
                      valueToDb.ToString(),
                  valueFromDb =>
                      (PublishSurveyType)Enum.Parse(typeof(PublishSurveyType), valueFromDb))
              .IsRequired();

            builder.Property(e => e.Status).HasMaxLength(maxLength: 100)
              .HasConversion(valueToDb =>
                      valueToDb.ToString(),
                  valueFromDb =>
                      (PublishSurveyStatus)Enum.Parse(typeof(PublishSurveyStatus), valueFromDb))
              .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(1054);

            builder.Property(x => x.StartDate)
                .IsRequired();

            builder.Property(x => x.EndDate)
                .IsRequired();

            builder.Property(x => x.IsGrapicExtender)
                .HasDefaultValue(false);

            builder.Property(x => x.IsMandatory)
                .HasDefaultValue(false);

            builder.Property(x => x.IsEntryOneTime)
                .HasDefaultValue(false);

            builder.Property(e => e.SubmissionOption)
                .HasMaxLength(maxLength: 100)
              .HasConversion(valueToDb =>
                      valueToDb.ToString(),
                  valueFromDb =>
                      (PublishSurveySubmissionOption)Enum.Parse(typeof(PublishSurveySubmissionOption), valueFromDb));

            builder.Property(x => x.AboveSubmitText);

            builder.Property(x => x.ThankYouMessage);

            builder.Property(x => x.AfterSurveyCloseText);

            builder.HasOne(x => x.AcademicYear)
              .WithMany(x => x.PublishSurveys)
              .HasForeignKey(fk => fk.IdAcademicYear)
              .HasConstraintName("FK_TrPublishSurvey_MsAcademicYear")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.SurveyTemplate)
             .WithMany(x => x.PublishSurveys)
             .HasForeignKey(fk => fk.IdSurveyTemplate)
             .HasConstraintName("FK_TrPublishSurvey_MsSurveyTemplate")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.HasOne(x => x.SurveyTemplateLink)
              .WithMany(x => x.PublishSurveyLinks)
              .HasForeignKey(fk => fk.IdSurveyTemplateLink)
              .HasConstraintName("FK_TrPublishSurvey_MsSurveyTemplateLink")
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.PublishSurveyLink)
              .WithMany(x => x.PublishSurveyLinks)
              .HasForeignKey(fk => fk.IdPublishSurveyLink)
              .HasConstraintName("FK_TrPublishSurvey_TrPublishSurvey")
              .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
