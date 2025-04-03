using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Student.DAL.Entities;
using BinusSchool.Student.DAL.Entities.School;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsSchool : AuditEntity, IStudentEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        //public string Address { get; set; }
        public virtual ICollection<MsAcademicYear> AcademicYears { get; set; }
        public virtual ICollection<TrAscTimetable> AscTimetables { get; set; }
        //public virtual ICollection<MsBuilding> Buildings { get; set; }
        //public virtual ICollection<MsCurriculum> Curriculums { get; set; }
        //public virtual ICollection<MsDivision> Divisions { get; set; }
        public virtual ICollection<MsClassroom> Classrooms { get; set; }
        //public virtual ICollection<MsSessionSet> SessionSets { get; set; }
        //public virtual ICollection<MsSubjectType> SubjectTypes { get; set; }
        //public virtual ICollection<MsSubjectGroup> SubjectGroups { get; set; }
        //public virtual ICollection<MsSubjectLevel> SubjectLevels { get; set; }
        public virtual ICollection<MsLevelOfInteraction> LevelOfInteractions { get; set; }
        public virtual ICollection<MsStudent> Students { get; set; }
        public virtual ICollection<MsStaff> Staffs { get; set; }
        public virtual ICollection<MsUniversityPortal> UniversityPortal { get; set; }
        public virtual ICollection<MsUniversityPortal> UniversityPortalFrom { get; set; }
        public virtual ICollection<MsUniversityPortalApproval> UniversityPortalApproval { get; set; }
        public virtual ICollection<MsCounselorOption> CounselorOption { get; set; }
        public virtual ICollection<LtExemplaryCategory> ExemplaryCategories { get; set; }
        public virtual ICollection<LtExemplaryValue> LtExemplaryValues { get; set; }
        public virtual ICollection<MsStudentEmailAdditionalReceiver> StudentEmailAdditionalReceivers { get; set; }
        public virtual ICollection<MsTeacherPosition> TeacherPositions { get; set; }
        public virtual ICollection<MsCounselingCategory> CounselingCategories { get; set; }
        public virtual ICollection<MsConcernCategory> ConcernCategories { get; set; }
        public virtual ICollection<MsBuilding> Buildings { get; set; }
        public virtual ICollection<LtStudentDemoReportType> StudentDemoReportTypes { get; set; }
        public virtual ICollection<MsMedicalItem> MedicalItems { get; set; }
        public virtual ICollection<MsMedicalTreatment> MedicalTreatments { get; set; }
        public virtual ICollection<MsMedicalCondition> MedicalConditions { get; set; }
        public virtual ICollection<MsMedicalHospital> MedicalHospitals { get; set; }
        public virtual ICollection<MsMedicalDoctor> MedicalDoctors { get; set; }
        public virtual ICollection<MsMedicalVaccine> MedicalVaccines { get; set; }
        public virtual ICollection<MsMedicalOtherUsers> MedicalOtherUsers { get; set; }
        public virtual ICollection<TrMedicalRecordEntry> MedicalRecordEntries { get; set; }
        public virtual ICollection<MsSubjectSelectionNextGrade> SubjectSelectionNextGrades { get; set; }
        public virtual ICollection<LtSubjectSelectionCurriculum> SubjectSelectionCurricula { get; set; }
        public virtual ICollection<MsSubjectLevel> SubjectLevels { get; set; }
        public virtual ICollection<LtSubjectSelectionGroup> SubjectSelectionGroups { get; set; }
        public virtual ICollection<LtSubjectSelectionMajor> SubjectSelectionMajors { get; set; }
        public virtual ICollection<LtSubjectSelectionCountry> SubjectSelectionCountries { get; set; }
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

            base.Configure(builder);
        }
    }
}
