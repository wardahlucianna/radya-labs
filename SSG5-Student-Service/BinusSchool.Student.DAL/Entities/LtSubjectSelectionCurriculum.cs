using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.DAL.Entities
{
    public class LtSubjectSelectionCurriculum : AuditEntity, IStudentEntity
    {
        public string IdSchool { get; set; }
        public string CurriculumName { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsMappingCurriculumGrade> MappingCurriculumGrades { get; set; }
    }

    internal class LtSubjectSelectionCurriculumConfiguration : AuditEntityConfiguration<LtSubjectSelectionCurriculum>
    {
        public override void Configure(EntityTypeBuilder<LtSubjectSelectionCurriculum> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.SubjectSelectionCurricula)
                .HasForeignKey(x => x.IdSchool)
                .HasConstraintName("FK_LtSubjectSelectionCurriculum_MsSchool")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}