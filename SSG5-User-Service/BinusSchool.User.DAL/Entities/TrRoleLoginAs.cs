using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrRoleLoginAs : AuditEntity, IUserEntity
    {
        public string IdRole { get; set; }
        public string IdAuthorizedRole { get; set; }

        public virtual LtRole Role { get; set; }
        public virtual LtRole AuthorizedRole { get; set; }
    }

    internal class TrRoleLoginAsConfiguration : AuditEntityConfiguration<TrRoleLoginAs>
    {
        public override void Configure(EntityTypeBuilder<TrRoleLoginAs> builder)
        {
            builder.Property(x => x.IdRole)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.IdAuthorizedRole)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.Role)
                .WithMany(x => x.RoleLoginAsRole)
                .HasForeignKey(fk => fk.IdRole)
                .HasConstraintName("FK_TrRoleLoginAs_LtRole_Role")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.AuthorizedRole)
                .WithMany(x => x.RoleLoginAsAuthorizedRole)
                .HasForeignKey(fk => fk.IdAuthorizedRole)
                .HasConstraintName("FK_TrRoleLoginAs_LtRole_AuthorizedRole")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
