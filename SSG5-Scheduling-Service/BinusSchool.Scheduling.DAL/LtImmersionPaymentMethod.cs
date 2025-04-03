using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb
{
    public class LtImmersionPaymentMethod : CodeEntity, ISchedulingEntity
    {
        public virtual ICollection<MsImmersion> Immersions { get; set; }
    }

    internal class LtImmersionPaymentMethodConfiguration : CodeEntityConfiguration<LtImmersionPaymentMethod>
    {
        public override void Configure(EntityTypeBuilder<LtImmersionPaymentMethod> builder)
        {
            builder.Property(x => x.Code)
               .HasMaxLength(36);

            builder.Property(x => x.Description)
                .HasMaxLength(50);

            base.Configure(builder);
        }
    }
}
