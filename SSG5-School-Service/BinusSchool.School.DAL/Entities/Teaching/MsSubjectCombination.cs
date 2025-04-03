using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities.Teaching
{
    public class MsSubjectCombination : AuditEntity, ISchoolEntity
    {
        public string IdGradePathwayClassroom { get; set; }
        public string IdSubject { get; set; }

        public virtual MsGradePathwayClassroom GradePathwayClassroom { get; set; }
        public virtual MsSubject Subject { get; set; }
    }

    internal class MsSubjectCombinationConfiguration : AuditEntityConfiguration<MsSubjectCombination>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectCombination> builder)
        {
            builder.HasOne(x => x.GradePathwayClassroom)
                .WithMany(x => x.SubjectCombinations)
                .HasForeignKey(fk => fk.IdGradePathwayClassroom)
                .HasConstraintName("FK_MsSubjectCombination_MsGradePathwayClassroom")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.SubjectCombinations)
                .HasForeignKey(fk => fk.IdSubject)
                .HasConstraintName("FK_MsSubjectCombination_MsSubject")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
