using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.Scheduling
{
    public class MsLessonPathway : AuditEntity, IStudentEntity
    {
        public string IdLesson { get; set; }
        public string IdHomeroomPathway { get; set; }

        public virtual MsLesson Lesson { get; set; }
        public virtual MsHomeroomPathway HomeroomPathway { get; set; }
    }

    internal class MsLessonPathwayConfiguration : AuditEntityConfiguration<MsLessonPathway>
    {
        public override void Configure(EntityTypeBuilder<MsLessonPathway> builder)
        {
            builder.HasOne(x => x.Lesson)
                .WithMany(x => x.LessonPathways)
                .HasForeignKey(fk => fk.IdLesson)
                .HasConstraintName("FK_MsLessonPathway_MsLesson")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.HomeroomPathway)
                .WithMany(x => x.LessonPathways)
                .HasForeignKey(fk => fk.IdHomeroomPathway)
                .HasConstraintName("FK_MsLessonPathway_MsHomeroomPathway")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
