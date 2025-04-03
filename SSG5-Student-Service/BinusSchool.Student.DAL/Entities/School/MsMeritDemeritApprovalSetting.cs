using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.School
{
    public class MsMeritDemeritApprovalSetting : AuditEntity, IStudentEntity
    {
        public string IdLevel { get; set; }
        public string Approval1 { get; set; }
        public string Approval2 { get; set; }
        public string Approval3 { get; set; }
        public MsLevel Level { get; set; }
        public MsTeacherPosition PositionApproval1 { get; set; }
        public MsTeacherPosition PositionApproval2 { get; set; }
        public MsTeacherPosition PositionApproval3 { get; set; }
    }

    internal class MsDiciplineApprovalSettingConfiguration : AuditEntityConfiguration<MsMeritDemeritApprovalSetting>
    {
        public override void Configure(EntityTypeBuilder<MsMeritDemeritApprovalSetting> builder)
        {
            builder.HasOne(x => x.PositionApproval1)
                .WithMany(x => x.Approval1)
                .HasForeignKey(fk => fk.Approval1)
                .HasConstraintName("FK_MsDiciplineApproval_MsTeacherPosition_Approval1")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.PositionApproval2)
                .WithMany(x => x.Approval2)
                .HasForeignKey(fk => fk.Approval2)
                .HasConstraintName("FK_MsDiciplineApproval_MsTeacherPosition_Approval2")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PositionApproval3)
                .WithMany(x => x.Approval3)
                .HasForeignKey(fk => fk.Approval3)
                .HasConstraintName("FK_MsDiciplineApproval_MsTeacherPosition_Approval3")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Level)
                .WithMany(x => x.MeritDemeritApprovalSettings)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsDiciplineApproval_MsLevel")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
