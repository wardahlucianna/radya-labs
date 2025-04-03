using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.AttendanceDb.Entities.User
{
    public class LtRole : AuditEntity, IAttendanceEntity
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string IdRoleGroup { get; set; }
        public virtual LtRoleGroup RoleGroup { get; set; }
        public virtual ICollection<MsApprovalAttendanceAdministration> ApprovalAttendanceAdministrations { get; set; }
        public virtual ICollection<MsUserRole> UserRoles { get; set; }
        public virtual ICollection<TrRolePosition> RolePositions { get; set; }
    }

    internal class LtRoleConfiguration : AuditEntityConfiguration<LtRole>
    {
        public override void Configure(EntityTypeBuilder<LtRole> builder)
        {

            builder.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.HasOne(x => x.RoleGroup)
             .WithMany(x => x.Roles)
             .HasForeignKey(fk => fk.IdRoleGroup)
             .HasConstraintName("FK_LtRole_LtRoleGroup")
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired();
            base.Configure(builder);
        }
    }
}
