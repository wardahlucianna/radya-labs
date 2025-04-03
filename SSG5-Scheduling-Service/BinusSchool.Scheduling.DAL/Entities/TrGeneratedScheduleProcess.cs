using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class TrGeneratedScheduleProcess : AuditEntity, ISchedulingEntity
    {
        public string IdSchool { get; set; }
        public string Grades { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int Version { get; set; }

        public virtual MsSchool School { get; set; }
    }

    internal class TrGeneratedScheduleProcessConfiguration : AuditEntityConfiguration<TrGeneratedScheduleProcess>
    {
        public override void Configure(EntityTypeBuilder<TrGeneratedScheduleProcess> builder)
        {
            builder.Property(p => p.IdSchool)
                .IsRequired()
                .HasMaxLength(36);

            builder.Property(p => p.Version)
                .IsRequired()
                .HasMaxLength(2);

            builder.HasOne(x => x.School)
             .WithMany(x => x.GeneratedScheduleProcess)
             .HasForeignKey(fk => fk.IdSchool)
             .HasConstraintName("FK_TrGeneratedScheduleProcess_MsSchool")
             .OnDelete(DeleteBehavior.NoAction)
              .IsRequired();

            builder.Property(p => p.Grades)
                .HasMaxLength(255);

            builder.Property(p => p.StartAt)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
