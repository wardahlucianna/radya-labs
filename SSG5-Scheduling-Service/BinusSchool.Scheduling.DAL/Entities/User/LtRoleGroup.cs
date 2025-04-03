using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.User
{
    public class LtRoleGroup : CodeEntity, ISchedulingEntity
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
