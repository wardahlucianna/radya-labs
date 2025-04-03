using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class TrSendEmailAttendanceEntry : AuditEntity, IAttendanceEntity
    {
        public string IdGeneratedScheduleLesson { get; set; }
        public AttendanceSendEmailType Type { get; set; }

        public virtual TrGeneratedScheduleLesson GeneratedScheduleLesson { get; set; }
    }
    internal class TrSendEmailAttendanceEntryDetailConfiguration : AuditEntityConfiguration<TrSendEmailAttendanceEntry>
    {
        public override void Configure(EntityTypeBuilder<TrSendEmailAttendanceEntry> builder)
        {
            builder.HasOne(x => x.GeneratedScheduleLesson)
               .WithMany(x => x.SendEmailAttendanceEntries)
               .HasForeignKey(fk => fk.IdGeneratedScheduleLesson)
               .HasConstraintName("FK_TrSendEmailAttendanceEntry_TrGenerateScheduleLesson")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();


            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(12)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
