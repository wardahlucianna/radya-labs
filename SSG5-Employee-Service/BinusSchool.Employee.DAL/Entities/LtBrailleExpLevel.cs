using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtBrailleExpLevel : AuditEntity, IEmployeeEntity
    {
        public string BrailleExpDescEngName { get; set; }
        public string BrailleExpDescIndName { get; set; }

        public virtual ICollection<MsStaffJobInformation> StaffJobInformation { get; set; }
    }

    internal class LtBrailleExpLevelConfiguration : AuditEntityConfiguration<LtBrailleExpLevel>
    {
        public override void Configure(EntityTypeBuilder<LtBrailleExpLevel> builder)
        {   
            builder.Property(x => x.BrailleExpDescEngName)                
                .HasMaxLength(50);

            builder.Property(x => x.BrailleExpDescIndName)                
                .HasMaxLength(50);    

            base.Configure(builder);
        }
    }

}
