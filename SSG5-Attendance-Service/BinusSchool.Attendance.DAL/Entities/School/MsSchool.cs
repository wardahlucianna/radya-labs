using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.User;

namespace BinusSchool.Persistence.AttendanceDb.Entities.School
{
    public class MsSchool : AuditEntity, IAttendanceEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public virtual ICollection<MsAcademicYear> AcademicYears { get; set; }
        public virtual ICollection<MsClassroom> Classrooms { get; set; }
        public virtual ICollection<MsTeacherPosition> TeacherPositions { get; set; }
        public virtual ICollection<MsApprovalAttendanceAdministration> ApprovalAttendanceAdministrations { get; set; }
        public virtual ICollection<MsSchoolMappingEA> SchoolMappingEAs { get; set; }
        public virtual ICollection<MsSessionSet> SessionSets { get; set; }
        public virtual ICollection<MsBlockingCategory> BlockingCategorys { get; set; }
        public virtual ICollection<MsBlockingMessage> BlockingMessages { get; set; }
        public virtual ICollection<MsBlockingType> BlockingTypes { get; set; }
        public virtual ICollection<TrAttendanceSummaryTerm> AttendanceSummaryTerms { get; set; }
        public virtual ICollection<TrAttdSummaryLogSch> AttdSummaryLogSch { get; set; }
        public virtual ICollection<MsSubjectLevel> SubjectLevels { get; set; }
        public virtual ICollection<MsUserSchool> UserSchools { get; set; }
        public virtual ICollection<MsSubjectGroup> SubjectGroups { get; set; }
    }

    internal class MsSchoolConfiguration : AuditEntityConfiguration<MsSchool>
    {
        public override void Configure(EntityTypeBuilder<MsSchool> builder)
        {
            builder.Property(x => x.Logo)
            .HasMaxLength(900);
            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
