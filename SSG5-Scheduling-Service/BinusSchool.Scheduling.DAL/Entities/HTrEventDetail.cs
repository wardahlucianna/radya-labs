using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventDetail : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEvent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual HTrEvent Event { get; set; }
        public virtual ICollection<HTrUserEvent> UserEvents { get; set; }
    }

    internal class HTrEventDetailConfiguration : AuditNoUniqueEntityConfiguration<HTrEventDetail>
    {
        public override void Configure(EntityTypeBuilder<HTrEventDetail> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventDetail).Name)
                .HasMaxLength(36);

            builder.Property(x => x.StartDate).IsRequired();

            builder.Property(x => x.EndDate).IsRequired();

            builder.HasOne(x => x.Event)
                .WithMany(x => x.EventDetails)
                .HasForeignKey(fk => fk.IdEvent)
                .HasConstraintName("FK_HTrEventDetail_HTrEvent")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
