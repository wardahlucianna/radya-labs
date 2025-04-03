using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.Student
{
    public class LtParentRole : AuditEntity, IDocumentEntity
    {
        public string ParentRoleName { get; set; }
        public string ParentRoleNameEng { get; set; }
        public virtual ICollection<MsParent> Parent { get; set; }
    }

    internal class LtParentRoleConfiguration : AuditEntityConfiguration<LtParentRole>
    {
        public override void Configure(EntityTypeBuilder<LtParentRole> builder)
        {
            builder.Property(x => x.ParentRoleName)
                .HasMaxLength(30);

            builder.Property(x => x.ParentRoleNameEng)
                .HasMaxLength(30);

            base.Configure(builder);

        }
    }
}
