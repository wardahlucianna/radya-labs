using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;

namespace BinusSchool.Persistence.AttendanceDb.Entities.Scheduling
{
    public class MsEventType : CodeEntity, IAttendanceEntity
    {
        public string IdAcademicYear { get; set; }
        public string Color { get; set; }

        // public virtual ICollection<MsEvent> Events { get; set; }
        public virtual ICollection<TrEvent> TrEvents { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
    }

    internal class MsEventTypeConfiguration : CodeEntityConfiguration<MsEventType>
    {
        public override void Configure(EntityTypeBuilder<MsEventType> builder)
        {
            builder.HasOne(x => x.AcademicYear)
               .WithMany(x => x.EventTypes)
               .HasForeignKey(fk => fk.IdAcademicYear)
               .HasConstraintName("FK_MsEventType_MsAcademicYear")
               .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            builder.Property(x => x.Color)
                .HasMaxLength(7)
                .IsRequired();



            base.Configure(builder);
        }
    }
}
