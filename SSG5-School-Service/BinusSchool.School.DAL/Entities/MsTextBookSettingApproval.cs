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
    public class MsTextbookSettingApproval : AuditEntity, ISchoolEntity
    {
        public string IdBinusian { get; set; }
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdSchool { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public int ApproverTo { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
        public virtual MsStaff Staff { get; set; }
    }

    internal class MsTextBookSettingApprovalConfiguration : AuditEntityConfiguration<MsTextbookSettingApproval>
    {
        public override void Configure(EntityTypeBuilder<MsTextbookSettingApproval> builder)
        {
            builder.HasOne(x => x.Role)
                .WithMany(x => x.TextbookSettingApprovals)
                .HasForeignKey(fk => fk.IdRole)
                .HasConstraintName("FK_MsTextbookSettingApproval_LtRole")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
                .WithMany(x => x.TextbookSettingApprovals)
                .HasForeignKey(fk => fk.IdTeacherPosition)
                .HasConstraintName("FK_MsTextbookSettingApproval_MsTeacherPosition")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.TextbookSettingApprovals)
                .HasForeignKey(fk => fk.IdBinusian)
                .HasConstraintName("FK_MsTextbookSettingApproval_MsStaff")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.TextbookSettingApprovals)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsTextbookSettingApproval_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
