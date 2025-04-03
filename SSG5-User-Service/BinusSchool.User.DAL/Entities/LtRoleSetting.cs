using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class LtRoleSetting : AuditEntity, IUserEntity
    {
        public string IdRole { get; set; }
        public bool IsArrangeUsernameFormat { get; set; }
        public string UsernameFormat { get; set; }
        public string UsernameExample { get; set; }
        public LtRole Role { get; set; }
    }

    internal class LtRoleSettingConfiguration : AuditEntityConfiguration<LtRoleSetting>
    {
        public override void Configure(EntityTypeBuilder<LtRoleSetting> builder)
        {
            builder.HasOne(x => x.Role)
             .WithMany(x => x.RoleSettings)
             .HasForeignKey(fk => fk.IdRole)
             .HasConstraintName("FK_LtRoleSetting_LtRole")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();

            builder.Property(x => x.UsernameExample).HasMaxLength(450);

            builder.Property(x => x.UsernameFormat).HasMaxLength(450);

            //builder.Property(x => x.Alias).HasMaxLength(128);

            base.Configure(builder);
        }
    }
}
