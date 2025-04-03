using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrPublishSurveyGrade : AuditEntity, ISchoolEntity
    {
        public string IdPublishSurveyRespondent { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public int? Semester { get; set; }
        public virtual TrPublishSurveyRespondent PublishSurveyRespondent { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
    }

    internal class TrPublishSurveyGradeConfiguration : AuditEntityConfiguration<TrPublishSurveyGrade>
    {
        public override void Configure(EntityTypeBuilder<TrPublishSurveyGrade> builder)
        {
            builder.HasOne(x => x.PublishSurveyRespondent)
              .WithMany(x => x.PublishSurveyGrades)
              .HasForeignKey(fk => fk.IdPublishSurveyRespondent)
              .HasConstraintName("FK_TrPublishSurveyGrade_TrSurveyTemplateRespondent")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Level)
              .WithMany(x => x.PublishSurveyGrades)
              .HasForeignKey(fk => fk.IdLevel)
              .HasConstraintName("FK_TrPublishSurveyGrade_MsLevel")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Grade)
             .WithMany(x => x.PublishSurveyGrades)
             .HasForeignKey(fk => fk.IdGrade)
             .HasConstraintName("FK_TrPublishSurveyGrade_MsGrade")
             .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Homeroom)
             .WithMany(x => x.PublishSurveyGrades)
             .HasForeignKey(fk => fk.IdHomeroom)
             .HasConstraintName("FK_TrPublishSurveyGrade_MsHomeroom")
             .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
