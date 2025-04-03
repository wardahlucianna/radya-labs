using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtProfileDataFieldGroup : AuditEntity, IStudentEntity
    {
        public string Description { get; set; }
        public string Code { get; set; }
        public virtual ICollection<MsProfileDataField> ProfileDataFields { get; set; }
        public virtual ICollection<TrProfileDataFieldPrivilege> ProfileDataFieldPrivileges { get; set; }
    }

    internal class LtProfileDataFieldGroupConfiguration : AuditEntityConfiguration<LtProfileDataFieldGroup>
    {
        public override void Configure(EntityTypeBuilder<LtProfileDataFieldGroup> builder)
        {
            builder.Property(x => x.Description)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(x => x.Code)
                .HasMaxLength(36);

            base.Configure(builder);
        }
    }
}
