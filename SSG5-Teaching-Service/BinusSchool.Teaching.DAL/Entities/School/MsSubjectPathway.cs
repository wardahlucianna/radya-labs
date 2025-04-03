using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.School
{
    public class MsSubjectPathway : AuditEntity, ITeachingEntity
    {
        public string IdSubject { get; set; }
        public string IdGradePathwayDetail { get; set; }

        public virtual MsSubject Subject { get; set; }
        public virtual MsGradePathwayDetail GradePathwayDetail { get; set; }
    }

    internal class MsSubjectPathwayConfiguration : AuditEntityConfiguration<MsSubjectPathway>
    {
        public override void Configure(EntityTypeBuilder<MsSubjectPathway> builder)
        {
            builder.HasOne(x => x.Subject)
                .WithMany(x => x.SubjectPathways)
                .HasForeignKey(fk => fk.IdSubject)
                .HasConstraintName("FK_MsSubjectPathway_MsSubject")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            builder.HasOne(x => x.GradePathwayDetail)
                .WithMany(x => x.SubjectPathways)
                .HasForeignKey(fk => fk.IdGradePathwayDetail)
                .HasConstraintName("FK_MsSubjectPathway_MsGradePathwayDetail")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
