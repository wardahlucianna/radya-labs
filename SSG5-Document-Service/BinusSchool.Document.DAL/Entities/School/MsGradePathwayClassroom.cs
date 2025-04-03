using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.School
{
    public class MsGradePathwayClassroom : AuditEntity, IDocumentEntity
    {
        public string IdGradePathway { get; set; }
        public string IdClassroom { get; set; }

        public virtual MsGradePathway GradePathway { get; set; }
        public virtual MsClassroom Classroom { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
    }
    internal class MsGradePathwayClassroomConfiguration : AuditEntityConfiguration<MsGradePathwayClassroom>
    {
        public override void Configure(EntityTypeBuilder<MsGradePathwayClassroom> builder)
        {
            builder.HasOne(x => x.GradePathway)
                .WithMany(x => x.GradePathwayClassrooms)
                .HasForeignKey(fk => fk.IdGradePathway)
                .HasConstraintName("FK_MsGradePathwayClassroom_MsGradePathway")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Classroom)
               .WithMany(x => x.GradePathwayClassrooms)
               .HasForeignKey(fk => fk.IdClassroom)
               .HasConstraintName("FK_MsGradePathwayClassroom_MsClassRoom")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
