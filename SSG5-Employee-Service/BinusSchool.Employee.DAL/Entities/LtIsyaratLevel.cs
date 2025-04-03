using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtIsyaratLevel : AuditEntity, IEmployeeEntity
    {
        public string IsyaratLevelDescEngName { get; set; }
        public string IsyaratLevelDescIndName { get; set; }
        public virtual ICollection<MsStaffJobInformation> StaffJobInformation { get; set; }

    }

    internal class LtIsyaratLevelConfiguration : AuditEntityConfiguration<LtIsyaratLevel>
    {
        public override void Configure(EntityTypeBuilder<LtIsyaratLevel> builder)
        {   
            builder.Property(x => x.IsyaratLevelDescEngName)                
                .HasMaxLength(50);

            builder.Property(x => x.IsyaratLevelDescIndName)                
                .HasMaxLength(50);    

            base.Configure(builder);
        }
    }
}
