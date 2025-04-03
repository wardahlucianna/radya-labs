using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Student
{
    public class MsStudent : UserKindStudentParentEntity, IAttendanceEntity
    {
        public int IdStudentStatus { get; set; }
        public string IdBinusian { get; set; }
        public virtual LtStudentStatus StudentStatus { get; set; }
        public virtual ICollection<MsStudentParent> StudentParents { get; set; }
        public virtual ICollection<MsStudentGrade> StudentGrades { get; set; }
        public virtual ICollection<TrGeneratedScheduleStudent> GenerateScheduleStudents { get; set; }
        public virtual ICollection<MsHomeroomStudent> HomeroomStudents { get; set; }
        //public virtual ICollection<MsEventIntendedForPersonalStudent> EventIntendedForPersonalStudents { get; set; }
        public virtual ICollection<MsStudentBlocking> StudentBlockings { get; set; }
        public virtual ICollection<TrEventIntendedForPersonalStudent> TrEventIntendedForPersonalStudents { get; set; }
        public virtual ICollection<TrAttendanceSummary> AttendanceSummaries { get; set; }
        public virtual ICollection<TrStudentStatus> TrStudentStatuss { get; set; }
        public virtual ICollection<HMsStudentBlocking> HistoryStudentBlockings { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<TrAttendanceSummaryTerm> AttendanceSummaryTerms { get; set; }
        public virtual ICollection<TrAttdSummaryLogSchGrdStu> AttdSummaryLogSchGrdStu { get; set; }
        public virtual ICollection<TrEmergencyAttendance> EmergencyAttendances { get; set; }
        public virtual ICollection<HTrEmergencyAttendance> HistoryEmergencyAttendances { get; set; }
        
    }

    internal class MsStudentConfiguration : UserKindStudentParentEntityConfiguration<MsStudent>
    {
        public override void Configure(EntityTypeBuilder<MsStudent> builder)
        {
            builder.Property(x => x.IdBinusian)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.StudentStatus)
               .WithMany(y => y.Students)
               .HasForeignKey(fk => fk.IdStudentStatus)
               .HasConstraintName("FK_MsStudent_LtStudentStatus")
               .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
