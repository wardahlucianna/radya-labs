using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrRolePermission : AuditEntity, IUserEntity
    {
        public string IdFeaturePermission { get; set; }
        public string IdRole { get; set; }
        public string Type { get; set; }

        public virtual MsFeaturePermission FeaturePermission { get; set; }
        public virtual LtRole Role { get; set; }
    }

    internal class TrRolePermissionConfiguration : AuditEntityConfiguration<TrRolePermission>
    {
        public override void Configure(EntityTypeBuilder<TrRolePermission> builder)
        {
            builder.HasOne(x => x.Role)
             .WithMany(x => x.RolePermissions)
             .HasForeignKey(fk => fk.IdRole)
             .HasConstraintName("FK_TrRolePermission_LtRole")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.HasOne(x => x.FeaturePermission)
             .WithMany(x => x.RolePermissions)
             .HasForeignKey(fk => fk.IdFeaturePermission)
             .HasConstraintName("FK_TrRolePermission_MsFeaturePermission")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.Property(x => x.Type)
               .HasMaxLength(40);

            base.Configure(builder);
        }

    }
}
