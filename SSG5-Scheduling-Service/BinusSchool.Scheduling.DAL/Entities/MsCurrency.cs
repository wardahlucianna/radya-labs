using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsCurrency : AuditEntity, ISchedulingEntity
    {
        public string IdCountry { get; set; }
        public string Currency { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public virtual LtCountry Country { get; set; }
        public virtual ICollection<MsImmersion> Immersions { get; set; }
    }
    internal class MsCurrencyConfiguration : AuditEntityConfiguration<MsCurrency>
    {
        public override void Configure(EntityTypeBuilder<MsCurrency> builder)
        {
            builder.Property(x => x.IdCountry)
             .HasMaxLength(36)
             .IsRequired();

            builder.Property(x => x.Currency)
             .HasMaxLength(10)
             .IsRequired();

            builder.Property(x => x.Symbol)
             .HasMaxLength(10)
             .IsRequired();

            builder.Property(x => x.Name)
             .HasMaxLength(80)
             .IsRequired();

            builder.HasOne(x => x.Country)
                 .WithMany(y => y.Currencies)
                 .HasForeignKey(fk => fk.IdCountry)
                 .HasConstraintName("FK_MsImmersionRule_LtCountry")
                 .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
