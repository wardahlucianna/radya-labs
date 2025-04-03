using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.User
{
    public class MsUserRole : AuditEntity, IAttendanceEntity
    {
        public string IdUser { get; set; }
        public string IdRole { get; set; }
        public string Username { get; set; }
        public bool IsDefault { get; set; }
        public virtual MsUser User { get; set; }
        public virtual LtRole Role { get; set; }
    }

    internal class MsUserRoleConfiguration : AuditEntityConfiguration<MsUserRole>
    {
        public override void Configure(EntityTypeBuilder<MsUserRole> builder)
        {
            builder.Property(x => x.Username)
            .HasMaxLength(50)
            .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsUserRole_MsUser")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();


            builder.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(fk => fk.IdRole)
                .HasConstraintName("FK_MsUserRole_MsRole")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
