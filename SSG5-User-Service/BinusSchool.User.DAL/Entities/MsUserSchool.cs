using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class MsUserSchool : AuditEntity, IUserEntity
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }

        public virtual MsUser User { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsUserSchoolConfiguration : AuditEntityConfiguration<MsUserSchool>
    {
        public override void Configure(EntityTypeBuilder<MsUserSchool> builder)
        {
            builder.HasOne(x => x.School)
                 .WithMany(x => x.UserSchools)
                 .HasForeignKey(fk => fk.IdSchool)
                 .HasConstraintName("FK_MsUserSchool_MsSchool")
                 .OnDelete(DeleteBehavior.Cascade)
                 .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserSchools)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsUserSchool_MsUser")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
