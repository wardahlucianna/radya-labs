using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrProfileDataFieldPrivilege : AuditNoUniqueEntity, IStudentEntity
    {
        public string Id { get; set; }
        public string IdProfileDataFieldGroup { get; set; }
        public string IdProfileDataField { get; set; }
        public string IdBinusian { get; set; }
        public virtual LtProfileDataFieldGroup ProfileDataFieldGroup { get; set; }
        public virtual MsProfileDataField ProfileDataField { get; set; }
        public virtual MsStaff Staff { get; set; }
    }

    internal class TrProfileDataFieldPrivilegeConfiguration : AuditNoUniqueEntityConfiguration<TrProfileDataFieldPrivilege>
    {
        public override void Configure(EntityTypeBuilder<TrProfileDataFieldPrivilege> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("IdTrProfileDataFieldPrivilege")
                .HasMaxLength(36);

            builder.Property(x => x.IdProfileDataFieldGroup)
                .HasMaxLength(36);

            builder.Property(x => x.IdProfileDataField)
                .HasMaxLength(36);

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);

            builder.HasOne(x => x.ProfileDataFieldGroup)
                .WithMany(x => x.ProfileDataFieldPrivileges)
                .IsRequired()
                .HasForeignKey(fk => fk.IdProfileDataFieldGroup)
                .HasConstraintName("FK_TrProfileDataFieldPrivilege_LtProfileDataFieldGroup")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ProfileDataField)
                .WithMany(x => x.ProfileDataFieldPrivileges)
                .IsRequired()
                .HasForeignKey(fk => fk.IdProfileDataField)
                .HasConstraintName("FK_TrProfileDataFieldPrivilege_MsProfileDataField")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Staff)
                .WithMany(x => x.ProfileDataFieldPrivileges)
                .IsRequired()
                .HasForeignKey(fk => fk.IdBinusian)
                .HasConstraintName("FK_TrProfileDataFieldPrivilege_MsStaff")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
