using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsTextbookUserPeriod : AuditEntity, ISchoolEntity
    {
        public string IdAcademicYear { get; set; }
        public string GroupName { get; set; }
        public TextBookPreparationUserPeriodAssignAs AssignAs { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual ICollection<MsTextbookUserPeriodDetail> TextbookUserPeriodDetails { get; set; }

    }

    internal class MsTextBookUserPeriodConfiguration : AuditEntityConfiguration<MsTextbookUserPeriod>
    {
        public override void Configure(EntityTypeBuilder<MsTextbookUserPeriod> builder)
        {
            builder.Property(e => e.AssignAs).HasMaxLength(maxLength: 20)
               .HasConversion(valueToDb =>
               valueToDb.ToString(),
               valueFromDb => (TextBookPreparationUserPeriodAssignAs)Enum.Parse(typeof(TextBookPreparationUserPeriodAssignAs), valueFromDb))
               .IsRequired();

            builder.Property(x => x.GroupName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.TextbookUserPeriods)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsTextBookUserPeriod_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
