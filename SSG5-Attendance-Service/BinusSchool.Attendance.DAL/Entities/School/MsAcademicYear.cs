using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;

namespace BinusSchool.Persistence.AttendanceDb.Entities.School
{
    public class MsAcademicYear : CodeEntity, IAttendanceEntity
    {
        public string IdSchool { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsLevel> Levels { get; set; }
        public virtual ICollection<MsAttendance> Attendances { get; set; }
        public virtual ICollection<MsWorkhabit> Workhabits { get; set; }

        public virtual ICollection<MsPathway> Pathways { get; set; }
        //public virtual ICollection<MsEvent> Events { get; set; }
        public virtual ICollection<MsEventType> EventTypes { get; set; }
        public virtual ICollection<MsDepartment> Departments { get; set; }
        public virtual ICollection<MsNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
        public virtual ICollection<TrEvent> TrEvents { get; set; }
        public virtual ICollection<TrStudentStatus> TrStudentStatuss { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<TrAttendanceSummaryTerm> AttendanceSummaryTerms { get; set; }
        public virtual ICollection<MsBlockingTypeAtdSetting> BlockingTypeAtdSetting { get; set; }

        public virtual ICollection<MsScheduleLesson> ScheduleLesson { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<MsQuota> Quota { get; set; }
        public virtual ICollection<TrEmergencyReport> EmergencyReports { get; set; }


    }

    internal class MsAcademicYearConfiguration : CodeEntityConfiguration<MsAcademicYear>
    {
        public override void Configure(EntityTypeBuilder<MsAcademicYear> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.AcademicYears)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsAcademicYear_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
