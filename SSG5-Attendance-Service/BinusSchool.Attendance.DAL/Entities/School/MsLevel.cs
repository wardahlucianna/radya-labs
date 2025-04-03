using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;

namespace BinusSchool.Persistence.AttendanceDb.Entities.School
{
    public class MsLevel : CodeEntity, IAttendanceEntity
    {
        public string IdAcademicYear { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsGrade> Grades { get; set; }
        public virtual ICollection<MsFormula> Formulas { get; set; }
        public virtual ICollection<MsMappingAttendance> MappingAttendances { get; set; }
        public virtual ICollection<MsMappingAttendanceQuota> MappingAttendanceQuotas {get;set;}
        public virtual ICollection<MsDepartmentLevel> DepartmentLevels { get; set; }
        public virtual ICollection<TrEventIntendedForLevelStudent> EventIntendedForLevelStudents { get; set; }
        public virtual ICollection<MsBlockingTypeAtdSetting> BlockingTypeAtdSetting { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<TrAttendanceSummaryTerm> AttendanceSummaryTerms { get; set; }
        public virtual ICollection<MsScheduleLesson> ScheduleLesson { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<MsQuota> Quota { get; set; }
        public virtual ICollection<MsLatenessSetting> LatenessSettings { get; set; }

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
