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
    public class MsSurveyTemplate : AuditEntity, ISchoolEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdTemplateChild { get; set; }
        public SurveyTemplateLanguage Language { get; set; }
        public string Title { get; set; }
        public SurveyTemplateStatus Status { get; set; }
        public SurveyTemplateType Type { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<TrPublishSurvey> PublishSurveys { get; set; }
        public virtual ICollection<TrPublishSurvey> PublishSurveyLinks { get; set; }
    }

    internal class MsSurveyTemplateConfiguration : AuditEntityConfiguration<MsSurveyTemplate>
    {
        public override void Configure(EntityTypeBuilder<MsSurveyTemplate> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.SurveyTemplates)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsTemplateSurvey_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.Property(x => x.IdTemplateChild)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(1054);

            builder.Property(e => e.Language).HasMaxLength(maxLength: 10)
              .HasConversion(valueToDb =>
                      valueToDb.ToString(),
                  valueFromDb =>
                      (SurveyTemplateLanguage)Enum.Parse(typeof(SurveyTemplateLanguage), valueFromDb))
              .IsRequired();

            builder.Property(e => e.Type).HasMaxLength(maxLength: 25)
             .HasConversion(valueToDb =>
                     valueToDb.ToString(),
                 valueFromDb =>
                     (SurveyTemplateType)Enum.Parse(typeof(SurveyTemplateType), valueFromDb))
             .IsRequired();

            builder.Property(e => e.Status).HasMaxLength(maxLength: 10)
               .HasConversion(valueToDb =>
                       valueToDb.ToString(),
                   valueFromDb =>
                       (SurveyTemplateStatus)Enum.Parse(typeof(SurveyTemplateStatus), valueFromDb))
               .IsRequired();

            base.Configure(builder);
        }
    }
}
