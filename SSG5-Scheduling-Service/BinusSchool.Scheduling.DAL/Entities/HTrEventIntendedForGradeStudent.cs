using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventIntendedForGradeStudent : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdHomeroom { get; set; }
        public string IdEventIntendedFor { get; set; }

        public virtual HTrEventIntendedFor EventIntendedFor { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
    }

    internal class HTrEventIntendedForGradeStudentConfiguration : AuditNoUniqueEntityConfiguration<HTrEventIntendedForGradeStudent>
    {
        public override void Configure(EntityTypeBuilder<HTrEventIntendedForGradeStudent> builder)
        {
            builder.ToTable(typeof(HTrEventIntendedForGradeStudent).Name);

            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventIntendedForGradeStudent).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.EventIntendedFor)
              .WithMany(x => x.EventIntendedForGradeStudents)
              .HasForeignKey(fk => fk.IdEventIntendedFor)
              .HasConstraintName("FK_HTrEventIntendedForGradeStudent_HTrEventIntendedFor")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            builder.HasOne(x => x.Homeroom)
             .WithMany(x => x.HistoryEventIntendedForGradeStudents)
             .HasForeignKey(fk => fk.IdHomeroom)
             .HasConstraintName("FK_HTrEventIntendedForGradeStudent_MsHomeroom")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            base.Configure(builder);
        }

    }
}
