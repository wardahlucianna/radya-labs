using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtEducationLevel : AuditNoUniqueEntity, IEmployeeEntity
    {
        public string IdEducationLevel { get; set; } 
        public string EducationLevelName { get; set; }
        public virtual ICollection<TrStaffEducationInformation> StaffEducationInformation { get; set; }
    }
    internal class LtEducationLevelConfiguration : AuditNoUniqueEntityConfiguration<LtEducationLevel>
    {
        public override void Configure(EntityTypeBuilder<LtEducationLevel> builder)
        {   
            builder.HasKey(x => x.IdEducationLevel);

            builder.Property(x => x.IdEducationLevel)
                .HasColumnType("VARCHAR(1)");   
                
            builder.Property(x => x.EducationLevelName)                
                .HasMaxLength(50);
        
            base.Configure(builder);
        }
    }
}
