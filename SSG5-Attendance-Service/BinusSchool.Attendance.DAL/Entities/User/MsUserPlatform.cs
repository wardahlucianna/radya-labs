using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.User
{
    public class MsUserPlatform : AuditEntity, IAttendanceEntity
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
