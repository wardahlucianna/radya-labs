using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtCountry : AuditEntity, IStudentEntity
    {
        public string CountryName { get; set; }
        public virtual ICollection<MsStudent> Student { get; set; }
        public virtual ICollection<MsParent> Parent { get; set; }
        public virtual MsNationalityCountry NationalityCountry { get; set; }
        public virtual ICollection<LtCity> City { get; set; }
        public virtual ICollection<LtProvince> Province { get; set; }
    }
     internal class LtCountryConfiguration : AuditEntityConfiguration<LtCountry>
    {
        public override void Configure(EntityTypeBuilder<LtCountry> builder)
        {
             builder.Property(x => x.CountryName)
                .HasMaxLength(80);

            base.Configure(builder);
        }

    }
}
