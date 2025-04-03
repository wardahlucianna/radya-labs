using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtPTKType : AuditEntity, IEmployeeEntity
    {
        public string PTKTypeEngName { get; set; }
        public string PTKTypeIndName { get; set; }
        public virtual ICollection<MsStaffJobInformation> StaffJobInformation { get; set; }
    }
    internal class LtPTKTypeConfiguration : AuditEntityConfiguration<LtPTKType>
    {
        public override void Configure(EntityTypeBuilder<LtPTKType> builder)
        {   
            builder.Property(x => x.PTKTypeEngName)                
                .HasMaxLength(50);

            builder.Property(x => x.PTKTypeIndName)                
                .HasMaxLength(50);    

            base.Configure(builder);
        }
    }
}
