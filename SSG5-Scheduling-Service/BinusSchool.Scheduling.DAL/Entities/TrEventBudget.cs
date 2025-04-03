using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventBudget : AuditEntity, ISchedulingEntity
    {
        public string IdEvent { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public virtual TrEvent Event { get; set; }
    }

    internal class TrEventBudgetConfiguration : AuditEntityConfiguration<TrEventBudget>
    {
        public override void Configure(EntityTypeBuilder<TrEventBudget> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();

            builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired(true);

            builder.HasOne(x => x.Event)
               .WithMany(x => x.EventBudgets)
               .HasForeignKey(fk => fk.IdEvent)
               .HasConstraintName("FK_TrEventBudget_TrEvent")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
