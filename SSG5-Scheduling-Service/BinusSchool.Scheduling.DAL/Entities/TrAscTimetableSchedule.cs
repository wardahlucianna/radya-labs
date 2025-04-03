using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrAscTimetableSchedule : AuditEntity, ISchedulingEntity
    {
        public string IdSchedule { get; set; }
        public string IdAscTimetable { get; set; }
        public bool IsFromMaster { get; set; }

        public virtual MsSchedule Schedule { get; set; }
        public virtual TrAscTimetable AscTimetable { get; set; }
    }

    internal class TrAscTimetableScheduleConfiguration : AuditEntityConfiguration<TrAscTimetableSchedule>
    {
        public override void Configure(EntityTypeBuilder<TrAscTimetableSchedule> builder)
        {

            builder.Property(x => x.IsFromMaster)
                .HasDefaultValue(false);


            builder.HasOne(x => x.Schedule)
                .WithMany(x => x.AscTimetableSchedules)
                .HasForeignKey(fk => fk.IdSchedule)
                .HasConstraintName("FK_TrAscTimetableSchedule_MsSchedule")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            builder.HasOne(x => x.AscTimetable)
                .WithMany(x => x.AscTimetableSchedules)
                .HasForeignKey(fk => fk.IdAscTimetable)
                .HasConstraintName("FK_TrAscTimetableSchedule_TrAscTimetable")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
