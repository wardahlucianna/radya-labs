using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrExemplaryValue : AuditEntity, IStudentEntity
    {
        public string IdExemplary { get; set; }
        public string IdLtExemplaryValue { get; set; }
        public virtual TrExemplary Exemplary { get; set; }
        public virtual LtExemplaryValue LtExemplaryValue { get; set; }
    }

    internal class TrExemplaryValueConfiguration : AuditEntityConfiguration<TrExemplaryValue>
    {
        public override void Configure(EntityTypeBuilder<TrExemplaryValue> builder)
        {

            builder.Property(x => x.IdExemplary)
               .HasMaxLength(36)
               .IsRequired();

            builder.Property(x => x.IdLtExemplaryValue)
               .HasMaxLength(36)
               .IsRequired();

            builder.HasOne(x => x.Exemplary)
                .WithMany(x => x.TrExemplaryValues)
                .IsRequired()
                .HasForeignKey(fk => fk.IdExemplary)
                .HasConstraintName("FK_TrExemplaryValue_TrExemplary")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.LtExemplaryValue)
                .WithMany(x => x.TrExemplaryValues)
                .IsRequired()
                .HasForeignKey(fk => fk.IdLtExemplaryValue)
                .HasConstraintName("FK_TrExemplaryValue_LtExemplaryValue")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
