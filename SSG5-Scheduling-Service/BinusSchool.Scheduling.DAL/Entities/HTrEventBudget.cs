using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventBudget : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEvent { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public virtual HTrEvent Event { get; set; }
    }

    internal class HTrEventBudgetConfiguration : AuditNoUniqueEntityConfiguration<HTrEventBudget>
    {
        public override void Configure(EntityTypeBuilder<HTrEventBudget> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventBudget).Name)
                .HasMaxLength(36);

            builder.Property(x => x.Name).HasMaxLength(50).IsRequired();

            builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired(true);

            builder.HasOne(x => x.Event)
               .WithMany(x => x.EventBudgets)
               .HasForeignKey(fk => fk.IdEvent)
               .HasConstraintName("FK_HTrEventBudget_HTrEvent")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
