using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class TrPublishSurveyDepartment : AuditEntity, ISchoolEntity
    {
        public string IdPublishSurveyRespondent { get; set; }
        public string IdDepartement { get; set; }
        public virtual TrPublishSurveyRespondent PublishSurveyRespondent { get; set; }
        public virtual MsDepartment Department { get; set; }
    }

    internal class TrPublishSurveyDepartmentConfiguration : AuditEntityConfiguration<TrPublishSurveyDepartment>
    {
        public override void Configure(EntityTypeBuilder<TrPublishSurveyDepartment> builder)
        {
            builder.Property(x => x.IdDepartement)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.PublishSurveyRespondent)
              .WithMany(x => x.PublishSurveyDepartments)
              .HasForeignKey(fk => fk.IdPublishSurveyRespondent)
              .HasConstraintName("FK_TrPublishSurveyDepartment_TrSurveyTemplateRespondent")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Department)
              .WithMany(x => x.PublishSurveyDepartments)
              .HasForeignKey(fk => fk.IdDepartement)
              .HasConstraintName("FK_TrPublishSurveyDepartment_MsDepartment")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
