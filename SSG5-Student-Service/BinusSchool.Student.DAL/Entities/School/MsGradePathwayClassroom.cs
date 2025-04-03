using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsGradePathwayClassroom : AuditEntity, IStudentEntity
    {
        public string IdClassroom { get; set; }
        public string IdGradePathway { get; set; }
        public virtual MsClassroom Classroom { get; set; }
        public virtual MsGradePathway GradePathway { get; set; }
        public virtual ICollection<MsHomeroom> MsHomerooms { get; set; }
    }

    internal class MsGradePathwayClassroomConfiguration : AuditEntityConfiguration<MsGradePathwayClassroom>
    {
        public override void Configure(EntityTypeBuilder<MsGradePathwayClassroom> builder)
        {

            builder.HasOne(x => x.GradePathway)
               .WithMany(x => x.GradePathwayClassrooms)
               .HasForeignKey(fk => fk.IdGradePathway)
               .HasConstraintName("FK_MsGradePathwayClassroom_MsGradePathway")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            builder.HasOne(x => x.Classroom)
             .WithMany(x => x.GradePathwayClassrooms)
             .HasForeignKey(fk => fk.IdClassroom)
             .HasConstraintName("FK_MsGradePathwayClassroom_MsClassroom")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
