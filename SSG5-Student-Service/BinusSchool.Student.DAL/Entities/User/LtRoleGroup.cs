using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities.User
{
    public class LtRoleGroup : CodeEntity, IStudentEntity
    {
        public virtual ICollection<LtRole> Roles { get; set; } 
    }

    internal class LtRoleGroupConfiguration : CodeEntityConfiguration<LtRoleGroup>
    {
        public override void Configure(EntityTypeBuilder<LtRoleGroup> builder)
        {
            
            base.Configure(builder);
        }
    }
}
