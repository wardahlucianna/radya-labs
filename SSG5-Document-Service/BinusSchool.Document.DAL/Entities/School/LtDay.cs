using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities.School
{
    public class LtDay : CodeEntity, IDocumentEntity
    {
        public virtual ICollection<MsConsentCustomSchedule> ToConsentCustomSchedules { get; set; }
        public virtual ICollection<MsConsentCustomSchedule> FromConsentCustomSchedules { get; set; }
    }

    internal class LtDayConfiguration : CodeEntityConfiguration<LtDay>
    {
        public override void Configure(EntityTypeBuilder<LtDay> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Code)
                .HasMaxLength(36);

            builder.Property(x => x.Description)
                .HasMaxLength(36);
        }
    }
}
