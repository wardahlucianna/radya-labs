using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities
{
    public class MsProjectInformationRoleAccess : AuditEntity, ISchoolEntity
    {
        public string IdRole { get; set; }
        public virtual LtRole Role { get; set; }
    }

    internal class MsProjectInformationRoleAccessConfiguration : AuditEntityConfiguration<MsProjectInformationRoleAccess>
    {
        public override void Configure(EntityTypeBuilder<MsProjectInformationRoleAccess> builder)
        {
            builder.Property(x => x.IdRole)
                .HasMaxLength(36);

            builder.HasOne(x => x.Role)
                .WithMany(x => x.ProjectInformationRoleAccesses)
                .HasForeignKey(x => x.IdRole)
                .HasConstraintName("FK_MsProjectInformationRoleAccess_LtRole")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
