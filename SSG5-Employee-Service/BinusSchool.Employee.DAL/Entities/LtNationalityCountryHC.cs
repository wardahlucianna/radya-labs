using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtNationalityCountryHC : AuditNoUniqueEntity, IEmployeeEntity
    {
        public string IdNationalityCountry { get; set; } 
        public string NationalityCountryName { get; set; } 
        public virtual ICollection<MsStaff> Staff { get; set; }        
    }
    internal class LtNationalityCountryHCConfiguration : AuditNoUniqueEntityConfiguration<LtNationalityCountryHC>
    {
        public override void Configure(EntityTypeBuilder<LtNationalityCountryHC> builder)
        {
            builder.HasKey(x => x.IdNationalityCountry);
            
            builder.Property(x => x.IdNationalityCountry)
               .HasMaxLength(50);            

             builder.Property(x => x.NationalityCountryName)
               .HasMaxLength(50);            


            base.Configure(builder);
        }
    }
}
