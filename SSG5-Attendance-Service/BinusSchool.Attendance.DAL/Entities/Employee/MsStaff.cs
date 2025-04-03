using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Employee
{
    public class MsStaff : AuditNoUniqueEntity, IAttendanceEntity
    {
        public string IdBinusian { get; set; }    
        public string ShortName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public virtual ICollection<MsHomeroomTeacher> HomeroomTeachers { get; set; }
        public virtual ICollection<MsLessonTeacher> LessonTeachers { get; set; }
        public virtual ICollection<MsSchedule> Schedules { get; set; }
        public virtual ICollection<TrAttendanceEntryV2> AttendanceEntryV2s { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2sSubtitutes { get; set; }
    }

    internal class MsStaffConfiguration : AuditNoUniqueEntityConfiguration<MsStaff>
    {
        public override void Configure(EntityTypeBuilder<MsStaff> builder)
        {

            builder.HasKey(x => x.IdBinusian);
          
            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);    

            builder.Property(x => x.ShortName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();
         
            builder.Property(x => x.LastName)
               .HasMaxLength(100);

            builder.Property(x => x.BinusianEmailAddress)
             .HasMaxLength(60);

            builder.Property(x => x.PersonalEmailAddress)
               .HasMaxLength(60);

            base.Configure(builder);
        }
    }
}
