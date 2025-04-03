using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Employee;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsTextbookUserPeriodDetail : AuditEntity, ISchoolEntity
    {
        public string IdTextbookUserPeriod { get; set; }
        public string IdBinusian { get; set; }
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public virtual MsTextbookUserPeriod TextbookUserPeriod { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
        public virtual MsStaff Staff { get; set; }
    }

    internal class MsTextBookUserPeriodDetailConfiguration : AuditEntityConfiguration<MsTextbookUserPeriodDetail>
    {
        public override void Configure(EntityTypeBuilder<MsTextbookUserPeriodDetail> builder)
        {
            builder.HasOne(x => x.TextbookUserPeriod)
                .WithMany(x => x.TextbookUserPeriodDetails)
                .HasForeignKey(fk => fk.IdTextbookUserPeriod)
                .HasConstraintName("FK_MsTextbookUserPeriodDetail_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Role)
                .WithMany(x => x.TextbookUserPeriodDetails)
                .HasForeignKey(fk => fk.IdRole)
                .HasConstraintName("FK_MsTextbookUserPeriodDetail_LtRole")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
                .WithMany(x => x.TextbookUserPeriodDetails)
                .HasForeignKey(fk => fk.IdTeacherPosition)
                .HasConstraintName("FK_MsTextbookUserPeriodDetail_MsTeacherPosition")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.TextbookUserPeriodDetails)
                .HasForeignKey(fk => fk.IdBinusian)
                .HasConstraintName("FK_MsTextbookUserPeriodDetail_MsStaff")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }

    }
}
