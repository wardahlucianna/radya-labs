using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class MsApprovalAttendanceAdministration : AuditEntity, IAttendanceEntity
    {
        public string IdSchool {get;set;}
        public string IdRole {get;set;}

        public virtual LtRole Role {get;set;}
        public virtual MsSchool School {get;set;}

    }

    internal class MsApprovalAttendanceAdministrationConfiguration : AuditEntityConfiguration<MsApprovalAttendanceAdministration>
    {
        public override void Configure(EntityTypeBuilder<MsApprovalAttendanceAdministration> builder)
        {
             builder.HasOne(x => x.Role)
                .WithMany(x => x.ApprovalAttendanceAdministrations)
                .HasForeignKey(fk => fk.IdRole)
                .HasConstraintName("FK_MsApprovalAttendanceAdministration_LtRole")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.ApprovalAttendanceAdministrations)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_TrAttendanceAdministration_MsSchool")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
