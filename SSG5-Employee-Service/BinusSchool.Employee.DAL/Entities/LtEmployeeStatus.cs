using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtEmployeeStatus : AuditEntity, IEmployeeEntity
    {
        public string EmployeeStatusDesc { get; set; }
        public virtual ICollection<MsStaffJobInformation> StaffJobInformation { get; set; }

    }
    internal class LtEmployeeStatusConfiguration : AuditEntityConfiguration<LtEmployeeStatus>
    {
        public override void Configure(EntityTypeBuilder<LtEmployeeStatus> builder)
        {   
            builder.Property(x => x.EmployeeStatusDesc)                
                .HasMaxLength(50);
           
            base.Configure(builder);
        }
    }
}
