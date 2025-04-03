﻿using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsGradePathwayClassroom : AuditEntity, ISchedulingEntity
    {
        public string IdGradePathway { get; set; }
        public string IdClassroom { get; set; }
        
        public virtual MsGradePathway MsGradePathway { get; set; }
        public virtual MsClassroom Classroom { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
    }

    internal class MsGradePathwayClassroomConfiguration : AuditEntityConfiguration<MsGradePathwayClassroom>
    {
        public override void Configure(EntityTypeBuilder<MsGradePathwayClassroom> builder)
        {
            builder.HasOne(x => x.MsGradePathway)
                .WithMany(x => x.MsGradePathwayClassrooms)
                .HasForeignKey(fk => fk.IdGradePathway)
                .HasConstraintName("FK_MsGradePathwayClassroom_MsGradePathway")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Classroom)
                .WithMany(x => x.MsGradePathwayClassrooms)
                .HasForeignKey(fk => fk.IdClassroom)
                .HasConstraintName("FK_MsGradePathwayClassroom_MsClassroom")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
