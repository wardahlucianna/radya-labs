using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsGradePathway : AuditEntity, IStudentEntity
    {
        public string IdGrade { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual ICollection<MsGradePathwayDetail> GradePathwayDetails { get; set; }
        public virtual ICollection<MsGradePathwayClassroom> GradePathwayClassrooms { get; set; }
    }
    internal class MsGradePathwayConfiguration : AuditEntityConfiguration<MsGradePathway>
    {
        public override void Configure(EntityTypeBuilder<MsGradePathway> builder)
        {
            builder.HasOne(x => x.Grade)
               .WithMany(x => x.GradePathways)
               .HasForeignKey(fk => fk.IdGrade)
               .HasConstraintName("FK_MsGradePathway_MsGrade")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
