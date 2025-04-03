using System;
using System.Collections.Generic;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrEventDetail : AuditEntity, IDocumentEntity
    {
        public string IdEvent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual TrEvent Event { get; set; }
    }

    internal class MsEventDetailConfiguration : AuditEntityConfiguration<TrEventDetail>
    {
        public override void Configure(EntityTypeBuilder<TrEventDetail> builder)
        {
            builder.Property(x => x.StartDate).IsRequired();

            builder.Property(x => x.EndDate).IsRequired();

            builder.HasOne(x => x.Event)
                .WithMany(x => x.EventDetails)
                .HasForeignKey(fk => fk.IdEvent)
                .HasConstraintName("FK_TrEventDetail_TrEvent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
