using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsSchedule : AuditEntity, ISchedulingEntity
    {
        public string IdLesson { get; set; }
        public string IdVenue { get; set; }
        public string IdWeekVarianDetail { get; set; }
        public string IdSession { get; set; }
        public string IdUser { get; set; }
        public int SessionNo { get; set; }
        public int Semester { get; set; }
        public string IdDay { get; set; }
        public string IdWeek { get; set; }

        public virtual MsLesson Lesson { get; set; }
        public virtual MsWeekVariantDetail WeekVarianDetail { get; set; }
        public virtual ICollection<TrAscTimetableSchedule> AscTimetableSchedules { get; set; }
        public virtual MsVenue Venue { get; set; }
        public virtual MsStaff User { get; set; }
        public virtual MsSession Sessions { get; set; }
        public virtual LtDay Day { get; set; }
        public virtual MsWeek Week { get; set; }

    }

    internal class MsScheduleConfiguration : AuditEntityConfiguration<MsSchedule>
    {
        public override void Configure(EntityTypeBuilder<MsSchedule> builder)
        {
            builder.HasOne(x => x.Venue)
                .WithMany(x => x.Schedules)
                .HasForeignKey(fk => fk.IdVenue)
                .HasConstraintName("FK_MsSchedule_MsVenue")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Week)
               .WithMany(x => x.Schedules)
               .HasForeignKey(fk => fk.IdWeek)
               .HasConstraintName("FK_MsSchedule_MsWeek")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.HasOne(x => x.Sessions)
              .WithMany(x => x.Schedules)
              .HasForeignKey(fk => fk.IdSession)
              .HasConstraintName("FK_MsSchedule_MsSession")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.Day)
              .WithMany(x => x.Schedules)
              .HasForeignKey(fk => fk.IdDay)
              .HasConstraintName("FK_MsSchedule_LtDay")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.User)
               .WithMany(x => x.Schedules)
               .HasForeignKey(fk => fk.IdUser)
               .HasConstraintName("FK_MsSchedule_MsStaff")
               .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            builder.HasOne(x => x.Lesson)
                .WithMany(x => x.Schedules)
                .HasForeignKey(fk => fk.IdLesson)
                .HasConstraintName("FK_MsSchedule_MsLesson")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.WeekVarianDetail)
                .WithMany(x => x.Schedules)
                .HasForeignKey(fk => fk.IdWeekVarianDetail)
                .HasConstraintName("FK_MsSchedule_MsWeekVarianDetail")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
