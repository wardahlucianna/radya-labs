using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsUserPlatform : AuditEntity, IUserEntity
    {
        public string IdUser { get; set; }
        public AppPlatform AppPlatform { get; set; }
        public string FirebaseToken { get; set; }

        public virtual MsUser User { get; set; }
    }

    internal class MsUserPlatformConfiguration : AuditEntityConfiguration<MsUserPlatform>
    {
        public override void Configure(EntityTypeBuilder<MsUserPlatform> builder)
        {
            builder.Property(x => x.AppPlatform)
                .HasMaxLength(7)
                .HasConversion<string>()
                .IsRequired();
            
            builder.Property(x => x.FirebaseToken)
                .HasMaxLength(512);
            
            builder.HasOne(x => x.User)
                .WithMany(x => x.UserPlatforms)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsUserPlatform_MsUser")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            
            base.Configure(builder);
        }
    }
}
