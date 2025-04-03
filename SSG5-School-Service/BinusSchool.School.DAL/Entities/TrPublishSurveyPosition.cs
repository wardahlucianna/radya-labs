using System;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrPublishSurveyPosition : AuditEntity, ISchoolEntity
    {
        public string IdPublishSurveyRespondent { get; set; }
        public string IdTeacherPosition { get; set; }
        public virtual TrPublishSurveyRespondent PublishSurveyRespondent { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
    }

    internal class TrPublishSurveyPositionConfiguration : AuditEntityConfiguration<TrPublishSurveyPosition>
    {
        public override void Configure(EntityTypeBuilder<TrPublishSurveyPosition> builder)
        {
            builder.Property(x => x.IdTeacherPosition)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.PublishSurveyRespondent)
              .WithMany(x => x.PublishSurveyPositions)
              .HasForeignKey(fk => fk.IdPublishSurveyRespondent)
              .HasConstraintName("FK_TrPublishSurveyPosition_TrSurveyTemplateRespondent")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
              .WithMany(x => x.PublishSurveyPositions)
              .HasForeignKey(fk => fk.IdTeacherPosition)
              .HasConstraintName("FK_TrPublishSurveyPosition_TrPublishSurveyPosition")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
