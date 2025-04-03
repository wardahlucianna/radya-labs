using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsAcademicYear : CodeEntity, IStudentEntity
    {
        public string IdSchool { get; set; }

        public virtual MsSchool MsSchool { get; set; }
        public virtual ICollection<MsLevel> MsLevels { get; set; }
        public virtual ICollection<MsPathway> Pathways { get; set; }
        public virtual ICollection<TrAscTimetable> AscTimetables { get; set; }
        public virtual ICollection<MsSanctionMapping> SanctionMappings { get; set; }
        public virtual ICollection<MsNonTeachingLoad> NonTeachingLoads { get; set; }
        public virtual ICollection<MsCounselor> Counselor { get; set; }
        public virtual ICollection<TrPersonalWellBeing> PersonalWellBeing { get; set; }
        public virtual ICollection<MsGcLink> GcLink { get; set; }
        public virtual ICollection<MsCountryFact> CountryFact { get; set; }
        public virtual ICollection<MsUsefulLink> UsefulLink { get; set; }
        public virtual ICollection<TrGcReportStudent> GcReportStudent { get; set; }
        public virtual ICollection<TrCounselingServicesEntry> CounselingServicesEntry { get; set; }
        public virtual ICollection<TrHandbook> Handbook { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
        public virtual ICollection<MsDepartment> Departments { get; set; }
        public virtual ICollection<TrStudentStatus> TrStudentStatuss { get; set; }
        public virtual ICollection<HTrStudentStatus> HTrStudentStatuss { get; set; }
        public virtual ICollection<TrExemplary> Exemplaries { get; set; }
        public virtual ICollection<MsLearningOutcome> LearningOutcomes { get; set; }
        public virtual ICollection<TrExperience> TrExperiences { get; set; }
        public virtual ICollection<TrCasAdvisor> TrCasAdvisors { get; set; }
        public virtual ICollection<TrExperienceStudent> TrExperienceStudents { get; set; }
        public virtual ICollection<TrLearningGoalStudent> LearningGoalStudents { get; set; }
        public virtual ICollection<TrStudentPhoto> StudentPhotos { get; set; }
        public virtual ICollection<TrCourseworkAnecdotalStudent> CourseworkAnecdotalStudents { get; set; }
        public virtual ICollection<TrReflectionStudent> ReflectionStudents { get; set; }
        public virtual ICollection<MsLockerAllocation> LockerAllocations { get; set; }
        public virtual ICollection<MsLocker> Lockers { get; set; }
        public virtual ICollection<MsLockerReservationPeriod> LockerReservationPeriods { get; set; }
        public virtual ICollection<TrStudentLockerReservation> StudentLockerReservations { get; set; }
        public virtual ICollection<MsDigitalPickupSetting> DigitalPickupSettings { get; set; }
        public virtual ICollection<MsDigitalPickupQrCode> DigitalPickupQrCodes { get; set; }
        public virtual ICollection<TrDigitalPickup> DigitalPickups { get; set; }
        public virtual ICollection<TrStudentExit> StudentExits { get; set; }
        public virtual ICollection<MsStudentExitSetting> StudentExitSettings { get; set; }
        public virtual ICollection<MsMappingLearningOutcome> MappingLearningOutcomes { get; set;}
        public virtual ICollection<TrServiceAsActionHeader> ServiceAsActionHeaders { get; set; }
        public virtual ICollection<TrStudentSubjectSelection> StudentSubjectSelections { get; set; }
        public virtual ICollection<TrStudentSubjectSelectionMajor> StudentSubjectSelectionMajors { get; set; }
        public virtual ICollection<TrStudentSubjectSelectionCountry> StudentSubjectSelectionCountries { get; set; }
        public virtual ICollection<MsSubjectSelectionSubjectAlias> SubjectSelectionSubjectAliases { get; set; }

    }

    internal class MsAcademicYearConfiguration : CodeEntityConfiguration<MsAcademicYear>
    {
        public override void Configure(EntityTypeBuilder<MsAcademicYear> builder)
        {


            builder.HasOne(x => x.MsSchool)
                .WithMany(x => x.AcademicYears)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsAcademicYear_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
