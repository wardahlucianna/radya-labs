using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class MsEventType : CodeEntity, ISchedulingEntity
    {
        public string IdAcademicYear { get; set; }
        public string Color { get; set; }

        public virtual ICollection<TrEvent> TrEvents { get; set; }
        public virtual ICollection<HTrEvent> HistoryEvents { get; set; }
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
