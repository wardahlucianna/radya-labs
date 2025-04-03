using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchoolDb.Abstractions;

namespace BinusSchool.Persistence.SchoolDb.Entities.User
{
    public class LtRole : AuditEntity, ISchoolEntity
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string IdRoleGroup { get; set; }
        public string IdSchool { get; set; }
        public virtual LtRoleGroup RoleGroup { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsSanctionMappingAttentionBy> SanctionMappingAttentionBies { get; set; }
        public virtual ICollection<MsTextbookUserPeriodDetail> TextbookUserPeriodDetails { get; set; }
        public virtual ICollection<MsTextbookSettingApproval> TextbookSettingApprovals { get; set; }
        public virtual ICollection<TrRolePosition> RolePositions { get; set; }
        public virtual ICollection<MsProjectInformationRoleAccess> ProjectInformationRoleAccesses { get; set; }
    }

    internal class LtRoleConfiguration : AuditEntityConfiguration<LtRole>
    {
        public override void Configure(EntityTypeBuilder<LtRole> builder)
        {
            builder.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.HasOne(x => x.RoleGroup)
             .WithMany(x => x.Roles)
             .HasForeignKey(fk => fk.IdRoleGroup)
             .HasConstraintName("FK_LtRole_LtRoleGroup")
             .OnDelete(DeleteBehavior.Cascade)
             .IsRequired();

            builder.HasOne(x => x.School)
             .WithMany(x => x.Roles)
             .HasForeignKey(fk => fk.IdSchool)
             .HasConstraintName("FK_LtRole_MsSchool")
             .OnDelete(DeleteBehavior.NoAction)
             .IsRequired();
            base.Configure(builder);
        }
    }
}
