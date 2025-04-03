using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtReligionHC :  AuditNoUniqueEntity, IEmployeeEntity
    {
        public string IdReligion { get; set; } 
        public string ReligionName { get; set; } 
        public virtual ICollection<MsStaff> Staff { get; set; }  
    }

    internal class LtReligionHCConfiguration : AuditNoUniqueEntityConfiguration<LtReligionHC>
    {
        public override void Configure(EntityTypeBuilder<LtReligionHC> builder)
        {
            builder.HasKey(x => x.IdReligion);

            builder.Property(x => x.IdReligion)
               .HasMaxLength(36);       

            builder.Property(x => x.ReligionName)
               .HasMaxLength(50);          

            base.Configure(builder);
        }
    }
}
