using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsStudentGradePathway : AuditEntity, IStudentEntity
    {
        public string IdStudentGrade { get; set; }
        public string IdPathway { get; set; }
        public string IdPathwayNextAcademicYear { get; set; }

        public virtual MsStudentGrade StudentGrade { get; set; }
        public virtual MsPathway Pathway { get; set; }
        public virtual MsPathway PathwayNextAcademicYear { get; set; }
    }

    internal class MsStudentGradePathwayConfiguration : AuditEntityConfiguration<MsStudentGradePathway>
    {
        public override void Configure(EntityTypeBuilder<MsStudentGradePathway> builder)
        {
            builder.HasOne(x => x.Pathway)
              .WithMany(x => x.StudentGradePathways)
              .IsRequired()
              .HasForeignKey(fk => fk.IdPathway)
              .HasConstraintName("FK_MsStudentGradePathway_MsPathway")
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PathwayNextAcademicYear)
             .WithMany(x => x.StudentGradePathwayNextAcademicYears)
             .HasForeignKey(fk => fk.IdPathwayNextAcademicYear)
             .HasConstraintName("FK_MsStudentGradePathway_MsPathwayNextAcademicYear")
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.StudentGrade)
                .WithMany(x => x.StudentGradePathways)
                .IsRequired()
                .HasForeignKey(fk => fk.IdStudentGrade)
                .HasConstraintName("FK_MsStudentGradePathway_MsStudentGrade")
                .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
