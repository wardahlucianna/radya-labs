using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.User
{
    public class LtRoleGroup : CodeEntity, IAttendanceEntity
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
