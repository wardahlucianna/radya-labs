using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsProfileDataField : AuditEntity, IStudentEntity
    {
        public string FieldDataProfile { get; set; }
        public string AliasName { get; set; }
        public string FlowTable { get; set; }
        public string IdProfileDataFieldGroup { get; set; }
        public int OrderNumber { get; set; }
        public virtual LtProfileDataFieldGroup ProfileDataFieldGroup { get; set; }
        public virtual ICollection<TrProfileDataFieldPrivilege> ProfileDataFieldPrivileges { get; set; }
    }

    internal class MsProfileDataFieldConfiguration : AuditEntityConfiguration<MsProfileDataField>
    {
        public override void Configure(EntityTypeBuilder<MsProfileDataField> builder)
        {
            builder.Property(x => x.FieldDataProfile)
                .HasMaxLength(150);

            builder.Property(x => x.AliasName)
                .HasMaxLength(150);

            builder.Property(x => x.FlowTable)
                .HasMaxLength(150);

            builder.Property(x => x.IdProfileDataFieldGroup)
                .HasMaxLength(36);

            builder.Property(x => x.OrderNumber)
                .HasColumnType(typeName: "int")
                .IsRequired();

            builder.HasOne(x => x.ProfileDataFieldGroup)
                .WithMany(x => x.ProfileDataFields)
                .IsRequired()
                .HasForeignKey(fk => fk.IdProfileDataFieldGroup)
                .HasConstraintName("FK_MsProfileDataField_LtProfileDataFieldGroup")
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }
    }
}
