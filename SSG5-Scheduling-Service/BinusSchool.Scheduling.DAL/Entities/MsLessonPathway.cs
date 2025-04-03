using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsLessonPathway : AuditEntity, ISchedulingEntity
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
