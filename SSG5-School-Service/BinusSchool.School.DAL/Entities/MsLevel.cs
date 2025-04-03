using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsLevel : CodeEntity, ISchoolEntity
    {
        public string IdAcademicYear { get; set; }
        public int OrderNumber { get; set; }

        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsDepartmentLevel> DepartmentLevels { get; set; }
        public virtual ICollection<MsGrade> Grades { get; set; }
        //public virtual ICollection<MsSubjectLevel> SubjectLevels { get; set; }
        public virtual ICollection<MsMeritDemeritApprovalSetting> MeritDemeritApprovalSetting { get; set; }
        public virtual ICollection<TrPublishSurveyGrade> PublishSurveyGrades { get; set; }
        public virtual ICollection<TrScheduleRealization2> ScheduleRealization2s { get; set; }

    }

    internal class MsLevelConfiguration : CodeEntityConfiguration<MsLevel>
    {
        public override void Configure(EntityTypeBuilder<MsLevel> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Levels)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsLevel_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            base.Configure(builder);
        }
    }
}
