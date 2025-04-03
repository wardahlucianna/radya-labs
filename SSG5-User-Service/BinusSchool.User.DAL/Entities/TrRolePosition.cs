using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class TrRolePosition : AuditEntity, IUserEntity
    {
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public LtRole Role { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
        public virtual ICollection<TrRolePositionPermission> RolePositionPermissions { get; set; }
        public virtual ICollection<TrHierarchyMappingDetail> HierarchyMappingDetails { get; set; }
    }

    internal class TrRolePositionConfiguration : AuditEntityConfiguration<TrRolePosition>
    {
        public override void Configure(EntityTypeBuilder<TrRolePosition> builder)
        {
            builder.HasOne(x => x.Role)
              .WithMany(x => x.RolePositions)
              .HasForeignKey(fk => fk.IdRole)
              .HasConstraintName("FK_TrRolePosition_LtRole")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.HasOne(x => x.TeacherPosition)
              .WithMany(x => x.RolePositions)
              .HasForeignKey(fk => fk.IdTeacherPosition)
              .HasConstraintName("FK_TrRolePosition_MsTeacherPosition")
              .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();


            base.Configure(builder);
        }
    }
}
