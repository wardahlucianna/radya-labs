using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsGrade : CodeEntity, IStudentEntity
    {
        public string IdLevel { get; set; }
        public int OrderNumber { get; set; }

        public virtual MsLevel MsLevel { get; set; }
        public virtual ICollection<MsStudentGrade> StudentGrades { get; set; }
        public virtual ICollection<MsPeriod> Periods { get; set; }
        public virtual ICollection<MsGradePathway> GradePathways { get; set; }
        public virtual ICollection<MsMeritDemeritComponentSetting> MeritDemeritComponentSettings { get; set; }
        public virtual ICollection<MsMeritDemeritMapping> MeritDemeritMappings { get; set; }
        public virtual ICollection<MsScoreContinuationSetting> ScoreContinuationSettings { get; set; }
        public virtual ICollection<MsCounselorGrade> CounselorGrade { get; set; }
        public virtual ICollection<MsGcLinkGrade> GcLinkGrades { get; set; }
        public virtual ICollection<MsUsefulLinkGrade> UsefulLinkGrade { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<MsSubject> Subjects { get; set; }
        public virtual ICollection<MsSurveyRespondent> SurveyRespondents { get; set; }
        public virtual ICollection<MsLockerAllocation> LockerAllocations { get; set; }
        public virtual ICollection<MsLockerReservationPeriod> LockerReservationPeriods { get; set; }
        public virtual ICollection<TrStudentLockerReservation> StudentLockerReservations { get; set; }
        public virtual ICollection<MsDigitalPickupSetting> DigitalPickupSettings { get; set; }
        public virtual ICollection<MsDigitalPickupQrCode> DigitalPickupQrCodes { get; set; }
        public virtual ICollection<TrGcReportStudentGrade> GcReportStudentGrades { get; set; }
        public virtual ICollection<MsMappingCurriculumGrade> MappingCurriculumGrades { get; set; }
        public virtual ICollection<MsSubjectSelectionRuleLimit> SubjectSelectionRuleLimits { get; set; }
        public virtual ICollection<MsSubjectSelectionRuleEnrollment> SubjectSelectionRuleEnrollments { get; set; }
        public virtual ICollection<MsSubjectSelectionPeriod> SubjectSelectionPeriods { get; set; }
        public virtual ICollection<TrStudentSubjectSelection> StudentSubjectSelections { get; set; }
        public virtual ICollection<MsMappingMajorGrade> MappingMajorGrades { get; set; }
        public virtual ICollection<TrStudentSubjectSelectionMajor> StudentSubjectSelectionMajors { get; set; }
        public virtual ICollection<MsMappingCountryGrade> MappingCountryGrades { get; set; }
        public virtual ICollection<TrStudentSubjectSelectionCountry> StudentSubjectSelectionCountries { get; set; }


    }

    internal class MsGradeConfiguration : CodeEntityConfiguration<MsGrade>
    {
        public override void Configure(EntityTypeBuilder<MsGrade> builder)
        {
            builder.HasOne(x => x.MsLevel)
                .WithMany(x => x.MsGrades)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsLevel_MsGrade")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
