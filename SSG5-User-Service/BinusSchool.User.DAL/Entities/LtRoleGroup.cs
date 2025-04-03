using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.UserDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.UserDb.Entities
{
    public class LtRoleGroup : CodeEntity, IUserEntity
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
