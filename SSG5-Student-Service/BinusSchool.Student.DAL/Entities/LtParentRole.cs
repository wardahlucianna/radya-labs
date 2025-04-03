using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtParentRole  : AuditEntity, IStudentEntity
    {
        public string ParentRoleName { get; set; }
        public string ParentRoleNameEng { get; set; }
        public virtual ICollection<MsParent> Parent { get; set; }
        public virtual ICollection<LtParentRelationship> ParentRelationship { get; set; }
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
