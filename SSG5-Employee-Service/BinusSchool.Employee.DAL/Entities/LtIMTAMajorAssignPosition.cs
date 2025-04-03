using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtIMTAMajorAssignPosition  : AuditEntity, IEmployeeEntity
    {
        public string IMTAMajorAssignPosition { get; set; }
        public virtual ICollection<MsStaff> Staff { get; set; }
    }
    internal class LtIMTAMajorAssignPositionConfiguration : AuditEntityConfiguration<LtIMTAMajorAssignPosition>
    {
        public override void Configure(EntityTypeBuilder<LtIMTAMajorAssignPosition> builder)
        {   
            builder.Property(x => x.IMTAMajorAssignPosition)                
                .HasMaxLength(50);

            base.Configure(builder);
        }
    }
}
