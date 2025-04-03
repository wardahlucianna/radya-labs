using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtKITASStatus : AuditEntity, IEmployeeEntity
    {
        public string KITASStatusDescription { get; set; }
        public virtual ICollection<MsStaff> Staff { get; set; }
    }
    internal class LtKITASStatusConfiguration : AuditEntityConfiguration<LtKITASStatus>
    {
        public override void Configure(EntityTypeBuilder<LtKITASStatus> builder)
        {   
            builder.Property(x => x.KITASStatusDescription)                
                .HasMaxLength(50);

            base.Configure(builder);
        }
    }
}
