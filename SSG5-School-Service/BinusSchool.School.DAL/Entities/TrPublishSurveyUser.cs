using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrPublishSurveyUser : AuditEntity, ISchoolEntity
    {
        public string IdPublishSurveyRespondent { get; set; }
        public string IdUser { get; set; }
        public string IdTeacherPosition { get; set; }
        public virtual TrPublishSurveyRespondent PublishSurveyRespondent { get; set; }
        public virtual MsUser User { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
    }

    internal class TrPublishSurveyUserConfiguration : AuditEntityConfiguration<TrPublishSurveyUser>
    {
        public override void Configure(EntityTypeBuilder<TrPublishSurveyUser> builder)
        {
            builder.Property(x => x.IdUser)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.PublishSurveyRespondent)
              .WithMany(x => x.PublishSurveyUsers)
              .HasForeignKey(fk => fk.IdPublishSurveyRespondent)
              .HasConstraintName("FK_TrPublishSurveyUser_TrSurveyTemplateRespondent")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.User)
              .WithMany(x => x.PublishSurveyUsers)
              .HasForeignKey(fk => fk.IdUser)
              .HasConstraintName("FK_TrPublishSurveyUser_MsUser")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
              .WithMany(x => x.PublishSurveyUsers)
              .HasForeignKey(fk => fk.IdTeacherPosition)
              .HasConstraintName("FK_TrPublishSurveyUser_MsTeacherPosition")
              .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
