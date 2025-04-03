using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtIMTASchoolLevel  : AuditEntity, IEmployeeEntity
    {
        public string IMTASchoolLevelIndName { get; set; }
        public string IMTASchoolLevelEngName { get; set; }
        public virtual ICollection<MsStaff> Staff { get; set; }
    }
    internal class LtIMTASchoolLevelConfiguration : AuditEntityConfiguration<LtIMTASchoolLevel>
    {
        public override void Configure(EntityTypeBuilder<LtIMTASchoolLevel> builder)
        {   
            builder.Property(x => x.IMTASchoolLevelIndName)                
                .HasMaxLength(50);

            builder.Property(x => x.IMTASchoolLevelEngName)                
                .HasMaxLength(50);    

            base.Configure(builder);
        }
    }
}
