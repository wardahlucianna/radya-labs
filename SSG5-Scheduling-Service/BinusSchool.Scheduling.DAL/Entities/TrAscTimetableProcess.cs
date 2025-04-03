using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrAscTimetableProcess : AuditEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public string Grades { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class TrAscTimetableProcessConfiguration : AuditEntityConfiguration<TrAscTimetableProcess>
    {
        public override void Configure(EntityTypeBuilder<TrAscTimetableProcess> builder)
        {
            builder.Property(p => p.IdSchool)
                .IsRequired()
                .HasMaxLength(36);

            builder.HasOne(x => x.School)
              .WithMany(x => x.AscTimetableProcess)
              .HasForeignKey(fk => fk.IdSchool)
              .HasConstraintName("FK_TrAscTimetableProcess_MsSchool")
              .OnDelete(DeleteBehavior.NoAction)
               .IsRequired();

            builder.Property(p => p.Grades)
                .HasMaxLength(1054);

            builder.Property(p => p.StartAt)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
