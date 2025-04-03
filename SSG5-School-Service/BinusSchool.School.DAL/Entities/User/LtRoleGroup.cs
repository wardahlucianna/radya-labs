using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities.User
{
    public class LtRoleGroup : CodeEntity, ISchoolEntity
    {
        public virtual ICollection<LtRole> Roles { get; set; }
        public virtual ICollection<MsBannerRole> BannerRoles { get; set; }
    }

    internal class LtRoleGroupConfiguration : CodeEntityConfiguration<LtRoleGroup>
    {
        public override void Configure(EntityTypeBuilder<LtRoleGroup> builder)
        {
            
            base.Configure(builder);
        }
    }
}
