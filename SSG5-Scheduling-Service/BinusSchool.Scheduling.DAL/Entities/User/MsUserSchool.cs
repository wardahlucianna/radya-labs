using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.User
{
    public class MsUserSchool : AuditEntity, ISchedulingEntity
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
                .HasConstraintName("FK_MsUserSchool_MsShool")
                .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.UserSchools)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_MsUserSchool_MsUser")
                .OnDelete(DeleteBehavior.NoAction)
                 .IsRequired();

            base.Configure(builder);
        }
    }
}
