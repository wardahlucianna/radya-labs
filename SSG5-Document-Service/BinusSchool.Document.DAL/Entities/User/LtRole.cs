using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.DocumentDb.Entities;
using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.DocumentDb.Entities.User
{
    public class LtRole : CodeEntity, IDocumentEntity
    {
        public string IdSchool { get; set; }
        public string IdRoleGroup { get; set; }
        public virtual LtRoleGroup RoleGroup { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<MsFormAssignmentRole> FormAssignmentRoles { get; set; }
        public virtual ICollection<HMsFormDocApproval> FormDocApprovals { get; set; }
        public virtual ICollection<HMsFormDocApproval> FormDocApprovalsActionNext { get; set; }
        public virtual ICollection<MsUserRole> UserRoles { get; set; }
        public virtual ICollection<MsDocumentReqDefaultPICGroup> DocumentReqDefaultPICGroups { get; set; }

        public virtual ICollection<HMsFormDocChangeApproval> DocChangeApprovals { get; set; }
        public virtual ICollection<HMsFormDocChangeApproval> DocChangeApprovalsActionNext { get; set; }
    }

    internal class LtRoleConfiguration : CodeEntityConfiguration<LtRole>
    {
        public override void Configure(EntityTypeBuilder<LtRole> builder)
        {
            builder.Property(x => x.IdRoleGroup)
              .HasMaxLength(36)
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
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
