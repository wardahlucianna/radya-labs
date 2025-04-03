using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.TeachingDb.Entities.User
{
    public class TrRolePosition : AuditEntity, ITeachingEntity
    {
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public LtRole Role { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }
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
