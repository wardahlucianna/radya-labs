using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtNationalityHC :  AuditNoUniqueEntity, IEmployeeEntity
    {
        public string IdNationality { get; set; } 
        public string NationalityName { get; set; } 
        public virtual ICollection<MsStaff> Staff { get; set; }  
    }
    internal class LtNationalityHCConfiguration : AuditNoUniqueEntityConfiguration<LtNationalityHC>
    {
        public override void Configure(EntityTypeBuilder<LtNationalityHC> builder)
        {
            builder.HasKey(x => x.IdNationality);

            builder.Property(x => x.IdNationality)
               .HasMaxLength(10);       

            builder.Property(x => x.NationalityName)
               .HasMaxLength(50);          

            base.Configure(builder);
        }
    }
}
