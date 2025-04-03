﻿using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class MsHomeroomPathway : AuditEntity, IAttendanceEntity
    {
        public string IdHomeroom { get; set; }
        public string IdGradePathwayDetail { get; set; }
        
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsGradePathwayDetail GradePathwayDetail { get; set; }
        public virtual ICollection<MsLessonPathway> LessonPathways { get; set; }
    }

    internal class MsHomeroomPathwayConfiguration : AuditEntityConfiguration<MsHomeroomPathway>
    {
        public override void Configure(EntityTypeBuilder<MsHomeroomPathway> builder)
        {
            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.HomeroomPathways)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_MsHomeroomPathway_MsHomeroom")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.GradePathwayDetail)
               .WithMany(x => x.HomeroomPathways)
               .HasForeignKey(fk => fk.IdGradePathwayDetail)
               .HasConstraintName("FK_MsHomeroomPathway_MsGradePathwayDetail")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
