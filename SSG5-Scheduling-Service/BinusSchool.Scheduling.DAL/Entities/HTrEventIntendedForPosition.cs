using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventIntendedForPosition : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdHTrEventIntendedForPosition { get; set; }
        public string IdEventIntendedFor { get; set; }
        public string IdTeacherPosition { get; set; }
        public virtual HTrEventIntendedFor EventIntendedFor { get; set; }
        public virtual MsTeacherPosition TeacherPosition { get; set; }

    }

    internal class HTrEventIntendedForPositionConfiguration : AuditNoUniqueEntityConfiguration<HTrEventIntendedForPosition>
    {
        public override void Configure(EntityTypeBuilder<HTrEventIntendedForPosition> builder)
        {
            builder.HasKey(x => x.IdHTrEventIntendedForPosition);

            builder.Property(p => p.IdHTrEventIntendedForPosition)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.EventIntendedFor)
                .WithMany(x => x.EventIntendedForPositions)
                .HasForeignKey(fk => fk.IdEventIntendedFor)
                .HasConstraintName("FK_HTrEventIntendedForPosition_HTrEventIntendedFor")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();


            builder.HasOne(x => x.TeacherPosition)
               .WithMany(x => x.HistoryEventIntendedForPositions)
               .HasForeignKey(fk => fk.IdTeacherPosition)
               .HasConstraintName("FK_HTrEventIntendedForPosition_MsTeacherPosition")
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired();

            base.Configure(builder);
        }
    }
}
