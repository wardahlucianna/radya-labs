using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventIntendedForLevelStudent : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdLevel { get; set; }
        public string IdEventIntendedFor { get; set; }

        public virtual HTrEventIntendedFor EventIntendedFor { get; set; }
        public virtual MsLevel Level { get; set; }
    }

    internal class HTrEventIntendedForLevelStudentConfiguration : AuditNoUniqueEntityConfiguration<HTrEventIntendedForLevelStudent>
    {
        public override void Configure(EntityTypeBuilder<HTrEventIntendedForLevelStudent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventIntendedForLevelStudent).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.EventIntendedFor)
             .WithMany(x => x.EventIntendedForLevelStudents)
             .HasForeignKey(fk => fk.IdEventIntendedFor)
             .HasConstraintName("FK_HTrEventIntendedForLevelStudent_HTrEventIntendedFor")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.Level)
             .WithMany(x => x.HistoryEventIntendedForLevelStudents)
             .HasForeignKey(fk => fk.IdLevel)
             .HasConstraintName("FK_HTrEventIntendedForLevelStudent_MsLevel")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }
    }
}
