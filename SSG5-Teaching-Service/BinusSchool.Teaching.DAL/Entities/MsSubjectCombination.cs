using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities
{
    public class MsSubjectCombination : AuditEntity, ITeachingEntity
    {
        public string IdGradePathwayClassroom { get; set; }
        public string IdSubject { get; set; }
        public virtual MsSubject Subject { get; set; }
        public virtual MsGradePathwayClassroom GradePathwayClassroom { get; set; }
        public virtual TrTimeTablePrefHeader TimeTablePrefHeader { get; set; }

        public virtual ICollection<TrTeachingLoad> TeachingLoads { get; set; }
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

            builder.HasOne(x => x.TimeTablePrefHeader)
               .WithOne(x => x.SubjectCombination)
               .HasForeignKey<TrTimeTablePrefHeader>(x => x.Id)
               .HasConstraintName("FK_MsSubjectCombination_TrTimeTablePrefHeader")
               .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
