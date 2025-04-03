using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities.Student
{
    public class LtCountry : AuditEntity, ISchedulingEntity
    {
        public string CountryName { get; set; }
        public virtual ICollection<MsCurrency> Currencies { get; set; }
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
