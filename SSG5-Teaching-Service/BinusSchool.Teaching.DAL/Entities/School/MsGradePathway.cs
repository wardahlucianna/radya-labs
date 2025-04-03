using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.School
{
    public class MsGradePathway : AuditEntity, ITeachingEntity
    {
        public string IdGrade { get; set; }

        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<MsGradePathwayClassroom> GradePathwayClassrooms { get; set; }
        public virtual ICollection<MsGradePathwayDetail> GradePathwayDetails { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
    }

    internal class MsGradePathwayConfiguration : AuditEntityConfiguration<MsGradePathway>
    {
        public override void Configure(EntityTypeBuilder<MsGradePathway> builder)
        {
            builder.HasOne(x => x.Grade)
                .WithMany(x => x.GradePathways)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsGradePathway_MsGrade")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
