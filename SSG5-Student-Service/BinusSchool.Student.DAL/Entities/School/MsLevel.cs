using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsLevel : CodeEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public int OrderNumber { get; set; }

        public virtual MsAcademicYear MsAcademicYear { get; set; }
        public virtual ICollection<MsGrade> MsGrades { get; set; }
        public virtual ICollection<MsMeritDemeritApprovalSetting> MeritDemeritApprovalSettings { get; set; }
        public virtual ICollection<TrPersonalWellBeingLevel> PersonalWellBeingLevel { get; set; }
        public virtual ICollection<MsCountryFactLevel> CountryFactLevel { get; set; }
        public virtual ICollection<TrHandbookLevel> HandbookViewLevel { get; set; }
        public virtual ICollection<MsDepartmentLevel> DepartmentLevels { get; set; }
        public virtual ICollection<MsSurveyRespondent> SurveyRespondents { get; set; }
        public virtual ICollection<MsLockerAllocation> LockerAllocations { get; set; }
        public virtual ICollection<MsLockerReservationPeriod> LockerReservationPeriods { get; set; }
        public virtual ICollection<TrGcReportStudentLevel> GcReportStudentLevels { get; set; }
    }

    internal class MsLevelConfiguration : CodeEntityConfiguration<MsLevel>
    {
        public override void Configure(EntityTypeBuilder<MsLevel> builder)
        {
            builder.HasOne(x => x.MsAcademicYear)
                .WithMany(x => x.MsLevels)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsAcademicYear_MsLevel")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
