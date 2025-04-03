using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrEventChange : AuditEntity, ISchedulingEntity
    {
        public string IdEvent { get; set; }
        public string ChangeNotes { get; set; }
        public TrEvent Event { get; set; }
        public HTrEvent HistoryEvent { get; set; }
    }
    internal class TrEventChangeConfiguration : AuditEntityConfiguration<TrEventChange>
    {
        public override void Configure(EntityTypeBuilder<TrEventChange> builder)
        {
            builder.HasOne(x => x.Event)
              .WithMany(x => x.EventChanges)
              .HasForeignKey(fk => fk.IdEvent)
              .HasConstraintName("FK_HsEvent_TrEvent")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.HistoryEvent)
              .WithOne(x => x.EventChange)
              .HasForeignKey<HTrEvent>(x => x.Id)
              .HasConstraintName("FK_HsEvent_TrEventChange")
              .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
