using System;
using System.Collections.Generic;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class TrEventDetail : AuditEntity, IAttendanceEntity
    {
        public string IdEvent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual TrEvent Event { get; set; }
        public virtual ICollection<TrUserEvent> UserEvents { get; set; }
    }

    internal class TrEventDetailDetailConfiguration : AuditEntityConfiguration<TrEventDetail>
    {
        public override void Configure(EntityTypeBuilder<TrEventDetail> builder)
        {
            builder.Property(x => x.StartDate).IsRequired();

            builder.Property(x => x.EndDate).IsRequired();

            builder.HasOne(x => x.Event)
                .WithMany(x => x.EventDetails)
                .HasForeignKey(fk => fk.IdEvent)
                .HasConstraintName("FK_TrEventDetail_TrEvent")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
