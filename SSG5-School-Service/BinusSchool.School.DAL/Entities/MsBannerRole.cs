using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsBannerRole : CodeEntity, ISchoolEntity
    {
        public string IdBanner { get; set; }
        public string IdRoleGroup { get; set; }
        public virtual MsBanner Banner { get; set; }
        public virtual LtRoleGroup RoleGroup { get; set; }
    }

    internal class MsBannerRoleConfiguration : CodeEntityConfiguration<MsBannerRole>
    {
        public override void Configure(EntityTypeBuilder<MsBannerRole> builder)
        {
            builder.HasOne(x => x.Banner)
               .WithMany(x => x.BannerRoles)
               .HasForeignKey(fk => fk.IdBanner)
               .HasConstraintName("FK_MsBannerRole_MsBenner")
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            builder.HasOne(x => x.RoleGroup)
               .WithMany(x => x.BannerRoles)
               .HasForeignKey(fk => fk.IdRoleGroup)
               .HasConstraintName("FK_MsBannerRole_LtRoleGroup")
               .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();


            base.Configure(builder);
        }
    }
}
