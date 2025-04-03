using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrAscTimetableLesson : AuditEntity, ISchedulingEntity
    {
        public string IdLesson { get; set; }
        public string IdAscTimetable { get; set; }
        public bool IsFromMaster { get; set; }

        public virtual MsLesson Lesson { get; set; }
        public virtual TrAscTimetable AscTimetable { get; set; }
        public virtual ICollection<TrAscTimetableDayStructure> AscTimetableDayStructures { get; set; }
    }

    internal class TrAscTimetableLessonConfiguration : AuditEntityConfiguration<TrAscTimetableLesson>
    {
        public override void Configure(EntityTypeBuilder<TrAscTimetableLesson> builder)
        {

            builder.Property(x => x.IsFromMaster)
             .HasDefaultValue(false);

            builder.HasOne(x => x.Lesson)
                .WithMany(x => x.AscTimetableLessons)
                .HasForeignKey(fk => fk.IdLesson)
                .HasConstraintName("FK_TrAscTimetableLesson_MsLesson")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.AscTimetable)
                .WithMany(x => x.AscTimetableLessons)
                .HasForeignKey(fk => fk.IdAscTimetable)
                .HasConstraintName("FK_TrAscTimetableLesson_TrAscTimetable")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
