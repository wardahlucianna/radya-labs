using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.TeachingDb.Entities.User
{
    public class LtRoleGroup : CodeEntity, ITeachingEntity
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
