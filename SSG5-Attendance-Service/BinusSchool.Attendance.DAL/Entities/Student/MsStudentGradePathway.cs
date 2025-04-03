using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Student
{
    public class MsStudentGradePathway : AuditEntity, IAttendanceEntity
    {
        public string IdPathway { get; set; }
        public string IdStudentGrade { get; set; }

        public virtual MsStudentGrade StudentGrade { get; set; }
        public virtual MsPathway Pathway { get; set; }
    }

    internal class MsStudentGradePathwayConfiguration : AuditEntityConfiguration<MsStudentGradePathway>
    {
        public override void Configure(EntityTypeBuilder<MsStudentGradePathway> builder)
        {
            builder.HasOne(x => x.Pathway)
                .WithMany(x => x.StudentGradePathways)
                .HasForeignKey(fk => fk.IdPathway)
                .HasConstraintName("FK_MsStudentGradePathway_MsPathway")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.StudentGrade)
                .WithMany(x => x.StudentGradePathways)
                .HasForeignKey(fk => fk.IdStudentGrade)
                .HasConstraintName("FK_MsStudentGradePathway_MsStudentGrade")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
