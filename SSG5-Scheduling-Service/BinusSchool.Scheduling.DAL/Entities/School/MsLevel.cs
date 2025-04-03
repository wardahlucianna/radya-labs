using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsLevel : CodeEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsGrade> MsGrades { get; set; }
        public virtual ICollection<TrEventIntendedForLevelStudent> EventIntendedForLevelStudents { get; set; }
        public virtual ICollection<HTrEventIntendedForLevelStudent> HistoryEventIntendedForLevelStudents { get; set; }
        public virtual ICollection<TrEventIntendedForGradeParent> TrEventIntendedForGradeParents { get; set; }
        public virtual ICollection<HTrEventIntendedForGradeParent> HistoryEventIntendedForGradeParents { get; set; }
        public virtual ICollection<TrInvitationBookingSettingDetail> InvitationBookingSettingDetails { get; set; }
        public virtual ICollection<MsMappingAttendance> MappingAttendances { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<MsScheduleLesson> ScheduleLesson { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<HTrScheduleRealization2> HistoryScheduleRealization2 { get; set; }
        public virtual ICollection<MsDepartmentLevel> DepartmentLevels { get; set; }
    }

    internal class MsLevelConfiguration : CodeEntityConfiguration<MsLevel>
    {
        public override void Configure(EntityTypeBuilder<MsLevel> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Levels)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsAcademicYear_MsLevel")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
