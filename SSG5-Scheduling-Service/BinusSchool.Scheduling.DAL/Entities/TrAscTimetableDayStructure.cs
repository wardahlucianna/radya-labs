using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrAscTimetableDayStructure : AuditEntity, ISchedulingEntity
    {
        public string IdAscTimetable { get; set; }
        public string IdAscTimetableLesson { get; set; }
        public string IdSession { get; set; }

        public virtual TrAscTimetable AscTimetable { get; set; }
        public virtual TrAscTimetableLesson AscTimetableLesson { get; set; }
        public virtual MsSession Session { get; set; }
    }

    internal class TrAscTimetableDayStructureConfiguration : AuditEntityConfiguration<TrAscTimetableDayStructure>
    {
        public override void Configure(EntityTypeBuilder<TrAscTimetableDayStructure> builder)
        {
            builder.Property(x => x.IdSession)
                .HasMaxLength(36)
                .IsRequired();
                
            builder.HasOne(x => x.AscTimetable)
                .WithMany(x => x.AscTimetableDayStructures)
                .HasForeignKey(fk => fk.IdAscTimetable)
                .HasConstraintName("FK_TrAscTimetableDayStructure_TrAscTimetable")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.AscTimetableLesson)
                .WithMany(x => x.AscTimetableDayStructures)
                .HasForeignKey(fk => fk.IdAscTimetableLesson)
                .HasConstraintName("FK_TrAscTimetableDayStructure_TrAscTimetableLesson")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Session)
                .WithMany(x => x.AscTimetableDayStructures)
                .HasForeignKey(fk => fk.IdSession)
                .HasConstraintName("FK_TrAscTimetableDayStructure_MsSession")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
