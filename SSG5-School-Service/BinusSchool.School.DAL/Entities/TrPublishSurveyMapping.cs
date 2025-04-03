using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Employee;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrPublishSurveyMapping : AuditEntity, ISchoolEntity
    {
        public string IdPublishSurvey { get; set; }
        public string IdLesson { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdBinusian { get; set; }
        public bool IsMapping { get; set; }
        public virtual TrPublishSurvey PublishSurvey { get; set; }
        public virtual MsStaff Staff { get; set; }
        public virtual MsHomeroomStudent HomeroomStudent { get; set; }
        public virtual MsLesson Lesson { get; set; }
    }

    internal class TrPublishSurveyMappingConfiguration : AuditEntityConfiguration<TrPublishSurveyMapping>
    {
        public override void Configure(EntityTypeBuilder<TrPublishSurveyMapping> builder)
        {
            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.PublishSurvey)
              .WithMany(x => x.PublishSurveyMappings)
              .HasForeignKey(fk => fk.IdPublishSurvey)
              .HasConstraintName("FK_TrPublishSurveyMapping_TrSurveyTemplatePublish")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Staff)
             .WithMany(x => x.PublishSurveyMappings)
             .HasForeignKey(fk => fk.IdBinusian)
             .HasConstraintName("FK_TrPublishSurveyMapping_MsStaff")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.HasOne(x => x.HomeroomStudent)
             .WithMany(x => x.PublishSurveyMappings)
             .HasForeignKey(fk => fk.IdHomeroomStudent)
             .HasConstraintName("FK_TrPublishSurveyMapping_MsHomeroomStudent")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.HasOne(x => x.Lesson)
            .WithMany(x => x.PublishSurveyMappings)
            .HasForeignKey(fk => fk.IdLesson)
            .HasConstraintName("FK_TrPublishSurveyMapping_MsLesson")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
