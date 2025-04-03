using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsPathway : CodeEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsGradePathwayDetail> GradePathwayDetails { get; set; }

        public virtual ICollection<MsStudentGradePathway> StudentGradePathways { get; set; }
        public virtual ICollection<MsStudentGradePathway> StudentGradePathwayNextAcademicYears { get; set; }
    }

    internal class MsPathwayConfiguration : CodeEntityConfiguration<MsPathway>
    {
        public override void Configure(EntityTypeBuilder<MsPathway> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Pathways)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsPathway_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
