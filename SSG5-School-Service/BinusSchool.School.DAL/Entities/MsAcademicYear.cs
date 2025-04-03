using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using BinusSchool.Persistence.SchoolDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsAcademicYear : CodeEntity, ISchoolEntity
    {
        public string IdSchool { get; set; }
        public int OrderNumber { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsDepartment> Departments { get; set; }
        public virtual ICollection<MsLevel> Levels { get; set; }
        public virtual ICollection<MsPathway> Pathways { get; set; }
        public virtual ICollection<MsSanctionMapping> SanctionMappings { get; set; }
        public virtual ICollection<MsTextbookUserPeriod> TextbookUserPeriods { get; set; }
        public virtual ICollection<MsTextbookSubjectGroup> TextbookSubjectGroups { get; set; }
        public virtual ICollection<TrTextbook> Textbooks { get; set; }
        public virtual ICollection<MsAnswerSet> AnswerSets { get; set; }
        public virtual ICollection<MsSurveyTemplate> SurveyTemplates { get; set; }
        public virtual ICollection<TrPublishSurvey> PublishSurveys { get; set; }
        public virtual ICollection<MsHomeroom> Homerooms { get; set; }
        public virtual ICollection<MsLesson> Lessons { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }
        public virtual ICollection<MsVenueMapping> VenueMappings { get; set; }
        public virtual ICollection<MsLocker> Lockers { get; set; }
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
