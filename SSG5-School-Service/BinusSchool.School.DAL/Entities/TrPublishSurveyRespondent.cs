using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.SchoolDb.Abstractions;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrPublishSurveyRespondent : AuditEntity, ISchoolEntity
    {
        public string IdPublishSurvey { get; set; }
        public PublishSurveyRole Role { get; set; }
        public PublishSurveyOption? Option { get; set; }
        public virtual TrPublishSurvey PublishSurvey { get; set; }
        public virtual ICollection<TrPublishSurveyPosition> PublishSurveyPositions { get; set; }
        public virtual ICollection<TrPublishSurveyDepartment> PublishSurveyDepartments { get; set; }
        public virtual ICollection<TrPublishSurveyUser> PublishSurveyUsers { get; set; }
        public virtual ICollection<TrPublishSurveyGrade> PublishSurveyGrades { get; set; }
    }

    internal class TrPublishSurveyRespondentConfiguration : AuditEntityConfiguration<TrPublishSurveyRespondent>
    {
        public override void Configure(EntityTypeBuilder<TrPublishSurveyRespondent> builder)
        {
            builder.Property(e => e.Role).HasMaxLength(maxLength: 7)
              .HasConversion(valueToDb =>
                      valueToDb.ToString(),
                  valueFromDb =>
                      (PublishSurveyRole)Enum.Parse(typeof(PublishSurveyRole), valueFromDb))
              .IsRequired();

            builder.Property(e => e.Option).HasMaxLength(maxLength: 15)
              .HasConversion(valueToDb =>
                      valueToDb.ToString(),
                  valueFromDb =>
                      (PublishSurveyOption)Enum.Parse(typeof(PublishSurveyOption), valueFromDb));

            builder.HasOne(x => x.PublishSurvey)
              .WithMany(x => x.PublishSurveyRespondents)
              .HasForeignKey(fk => fk.IdPublishSurvey)
              .HasConstraintName("FK_TrPublishSurveyRespondent_TrSurveyTemplatePublish")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
