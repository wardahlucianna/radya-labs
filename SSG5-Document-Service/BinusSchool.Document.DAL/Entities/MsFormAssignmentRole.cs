

using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.User;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class MsFormAssignmentRole : AuditEntity, IDocumentEntity
    {
        public string IdRole { get; set; }
        public virtual MsForm Form { get; set; }
        public virtual LtRole Role { get; set; }
        public virtual ICollection<MsFormAssignmentUser> FormAssignmentUsers { get; set; }
    }

    internal class MsFormAssignmentRoleConfiguration : AuditEntityConfiguration<MsFormAssignmentRole>
    {
        public override void Configure(EntityTypeBuilder<MsFormAssignmentRole> builder)
        {
            builder.Property(x => x.IdRole)
                .IsRequired()
                .HasMaxLength(36);

            builder.HasOne(x => x.Role)
              .WithMany(x => x.FormAssignmentRoles)
              .HasForeignKey(fk => fk.IdRole)
              .HasConstraintName("FK_MsFormAssignmentRole_LtRole")
              .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            builder.HasOne(x => x.Form)
              .WithOne(x => x.FormAssignmentRole)
              .HasForeignKey<MsFormAssignmentRole>(x => x.Id)
              .HasConstraintName("FK_MsFormAssignmentRole_MsForm")
              .OnDelete(DeleteBehavior.Cascade)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
