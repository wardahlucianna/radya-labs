using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsAcademicYear : CodeEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsLevel> Levels { get; set; }
        public virtual ICollection<MsPathway> Pathways { get; set; }
        public virtual ICollection<TrEvent> TrEvents { get; set; }
        public virtual ICollection<HTrEvent> HistoryEvent { get; set; }
        public virtual ICollection<MsEventType> EventTypes { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
        public virtual ICollection<TrAscTimetable> AscTimetables { get; set; }
        public virtual ICollection<MsDepartment> Departments { get; set; }
        public virtual ICollection<MsExtracurricularRule> ExtracurricularRules { get; set; }
        public virtual ICollection<MsCertificateTemplate> CertificateTemplates { get; set; }
        public virtual ICollection<MsNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<MsClassDiaryTypeSetting> ClassDiaryTypeSettings { get; set; }
        public virtual ICollection<MsImmersionPeriod> MsImmersionPeriods { get; set; }
        public virtual ICollection<MsExtracurricularScoreComponent> ExtracurricularScoreComponents { get; set; }
        public virtual ICollection<TrInvitationBookingSetting> TrInvitationBookingSettings { get; set; }
        public virtual ICollection<TrAvailabilitySetting> AvailabilitySettings { get; set; }
        public virtual ICollection<TrPersonalInvitation> PersonalInvitations { get; set; }
        public virtual ICollection<TrVisitorSchool> VisitorSchoolsBook { get; set; }
        public virtual ICollection<MsExtracurricularScoreCompCategory> ExtracurricularScoreCompCategorys { get; set; }
        public virtual ICollection<TrExtracurricularExternalCoachAtt> ExtracurricularExternalCoachAtts { get; set; }
        public virtual ICollection<MsWorkhabit> Workhabits { get; set; }
        public virtual ICollection<MsAttendance> Attendances { get; set; }
        
        public virtual ICollection<TrStudentStatus> TrStudentStatuss { get; set; }
        public virtual ICollection<TrGeneratedScheduleLesson> GeneratedScheduleLessons { get; set; }
        public virtual ICollection<MsScheduleLesson> ScheduleLesson { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<HTrScheduleRealization2> HistoryScheduleRealization2 { get; set; }
        public virtual ICollection<MsSettingSchedulePublishDate> MsSettingSchedulePublishDates { get; set; }
        public virtual ICollection<MsVenueMapping> VenueMappings { get; set; }
    }

    internal class MsAcademicYearConfiguration : CodeEntityConfiguration<MsAcademicYear>
    {
        public override void Configure(EntityTypeBuilder<MsAcademicYear> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.MsAcademicYears)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsAcademicYear_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
