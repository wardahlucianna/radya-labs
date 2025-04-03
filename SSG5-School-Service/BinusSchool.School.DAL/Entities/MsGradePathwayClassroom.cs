﻿using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsGradePathwayClassroom : AuditEntity, ISchoolEntity
    {
        public string IdGradePathway { get; set; }
        public string IdClassroom { get; set; }

        public virtual MsGradePathway GradePathway { get; set; }
        public virtual MsClassroom Classroom { get; set; }
        public virtual ICollection<MsSubjectCombination> SubjectCombinations { get; set; }
        public virtual ICollection<MsClassroomDivision> ClassroomDivisions { get; set; }
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
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Classroom)
               .WithMany(x => x.GradePathwayClassrooms)
               .HasForeignKey(fk => fk.IdClassroom)
               .HasConstraintName("FK_MsGradePathwayClassroom_MsClassRoom")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
