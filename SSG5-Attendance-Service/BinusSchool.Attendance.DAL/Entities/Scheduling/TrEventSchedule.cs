//using System;
//using System.Collections.Generic;
//using System.Text;
//using BinusSchool.Persistence.Entities;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Microsoft.EntityFrameworkCore;
//using BinusSchool.Persistence.AttendanceDb.Abstractions;

//namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
//{
//    public class TrEventSchedule : AuditEntity, IAttendanceEntity
//    {
//        public string IdEvent { get; set; }
//        public string IdScheduleLesson { get; set; }
//        public bool IsSyncAttendance { get; set; }
//        public virtual TrEvent Event { get; set; }
//        public virtual MsScheduleLesson ScheduleLesson { get; set; }
//    }

//    internal class TrEventScheduleConfiguration : AuditEntityConfiguration<TrEventSchedule>
//    {
//        public override void Configure(EntityTypeBuilder<TrEventSchedule> builder)
//        {
//            builder.HasOne(x => x.Event)
//                .WithMany(x => x.EventSchedules)
//                .HasForeignKey(fk => fk.IdEvent)
//                .HasConstraintName("FK_TrEventSchedule_TrEvent")
//                .OnDelete(DeleteBehavior.Restrict)
//                .IsRequired();

//            builder.HasOne(x => x.ScheduleLesson)
//               .WithMany(x => x.EventSchedules)
//               .HasForeignKey(fk => fk.IdScheduleLesson)
//               .HasConstraintName("FK_TrEventSchedule_MsScheduleLesson")
//               .OnDelete(DeleteBehavior.Restrict)
//               .IsRequired();

//            base.Configure(builder);
//        }
//    }
//}
