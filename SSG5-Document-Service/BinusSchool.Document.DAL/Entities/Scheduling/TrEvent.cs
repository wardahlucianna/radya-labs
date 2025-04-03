using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.DocumentDb.Entities
{
    public class TrEvent : AuditEntity, IDocumentEntity
    {
        public string IdEventType { get; set; }
        public string IdAcademicYear { get; set; }
        public string Name { get; set; }
        //public EventAttendanceType AttendanceType { get; set; }
        //public bool IsShowOnCalendarAcademic { get; set; }
        public string StatusEvent { get; set; }

        public virtual MsEventType EventType { get; set; }
        public virtual ICollection<TrEventDetail> EventDetails { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class MsEventConfiguration : AuditEntityConfiguration<TrEvent>
    {
        public override void Configure(EntityTypeBuilder<TrEvent> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.StatusEvent)
                .HasMaxLength(50);

            //builder.Property(x => x.AttendanceType)
            //    .HasConversion<string>()
            //    .HasMaxLength(9)
            //    .IsRequired();

            builder.HasOne(x => x.EventType)
                .WithMany(x => x.Events)
                .HasForeignKey(fk => fk.IdEventType)
                .HasConstraintName("FK_TrEvent_MsEventType")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.AcademicYear)
              .WithMany(x => x.Events)
              .HasForeignKey(fk => fk.IdAcademicYear)
              .HasConstraintName("FK_TrEvent_MsAcademicYear")
              .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
