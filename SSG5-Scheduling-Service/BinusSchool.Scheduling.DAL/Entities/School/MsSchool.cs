using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsSchool : AuditEntity, ISchedulingEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Logo { get; set; }
        public string Telephone { get; set; }
        public string Ext { get; set; }
        public virtual ICollection<MsStudent> Students { get; set; }
        public virtual ICollection<MsAcademicYear> MsAcademicYears { get; set; }
        public virtual ICollection<MsClassroom> MsClassrooms { get; set; }
        public virtual ICollection<TrAscTimetable> AscTimetables { get; set; }
        public virtual ICollection<MsUserSchool> UserSchools { get; set; }
        public virtual ICollection<LtRole> Roles { get; set; }
        public virtual ICollection<MsBuilding> Buildings { get; set; }
        public virtual ICollection<MsSessionSet> SessionSets { get; set; }
        public virtual ICollection<MsSubjectLevel> SubjectLevels { get; set; }
        public virtual ICollection<MsTeacherPosition> TeacherPositions { get; set; }
        public virtual ICollection<MsSubjectGroup> SubjectGroups { get; set; }
        public virtual ICollection<MsExtracurricularScoreLegend> ExtracurricularScoreLegends { get; set; }
        public virtual ICollection<LtExtracurricularStatusAtt> ExtracurricularStatusAtts { get; set; }
        public virtual ICollection<MsActivity> Activities { get; set; }
        public virtual ICollection<MsEventApproverSetting> EventApproverSettings { get; set; }
        public virtual ICollection<MsAward> Awards { get; set; }
        public virtual ICollection<MsCurriculum> Curriculums { get; set; }
        public virtual ICollection<MsExtracurricularGroup> ExtracurricularGroups { get; set; }
        public virtual ICollection<TrAscTimetableProcess> AscTimetableProcess { get; set; }
        public virtual ICollection<TrGeneratedScheduleProcess> GeneratedScheduleProcess { get; set; }
        public virtual ICollection<MsExtracurricularScoreGrade> ExtracurricularScoreGrades { get; set; }
        public virtual ICollection<MsExtracurricularScoreCalculationType> ExtracurricularScoreCalculationTypes { get; set; }
        public virtual ICollection<MsExtracurricularExternalCoach> ExtracurricularExternalCoachs { get; set; }
        public virtual ICollection<MsExtracurricularScoreLegendCategory> ExtracurricularScoreLegendCategorys { get; set; }
        public virtual ICollection<MsSettingEmailScheduleRealization> SettingEmailScheduleRealizations { get; set; }
        public virtual ICollection<MsSettingSchedulePublishDate> MsSettingSchedulePublishDates { get; set; }
        public virtual ICollection<HTrStudentProgramme> HTrStudentProgrammes { get; set; }
        public virtual ICollection<TrStudentProgramme> StudentProgrammes { get; set; }
        public virtual ICollection<MsExtracurricularAttPresentStat> ExtracurricularAttPresentStats { get; set; }
        public virtual ICollection<MsExtracurricularType> ExtracurricularTypes { get; set; }
        public virtual ICollection<MsReservationOwner> ReservationOwners { get; set; }
        public virtual ICollection<LtVenueType> VenueTypes { get; set; }
        public virtual ICollection<MsVenueReservationRule> VenueReservationRules { get; set; }
        public virtual ICollection<MsEquipmentType> EquipmentTypes { get; set; }
        public virtual ICollection<TrLogQueueEvent> LogQueueEvents { get; set; }
    }

    internal class MsSchoolConfiguration : AuditEntityConfiguration<MsSchool>
    {
        public override void Configure(EntityTypeBuilder<MsSchool> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.Logo)
                .HasMaxLength(900);

            base.Configure(builder);
        }
    }
}
