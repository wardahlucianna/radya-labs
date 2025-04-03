using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventIntendedForPersonalStudent : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdStudent { get; set; }
        public string IdEventIntendedFor { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual HTrEventIntendedFor EventIntendedFor { get; set; }
    }

    internal class HTrEventIntendedForPersonalStudentConfiguration : AuditNoUniqueEntityConfiguration<HTrEventIntendedForPersonalStudent>
    {
        public override void Configure(EntityTypeBuilder<HTrEventIntendedForPersonalStudent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventIntendedForPersonalStudent).Name)
                .HasMaxLength(36);

            builder.HasOne(x => x.EventIntendedFor)
             .WithMany(x => x.EventIntendedForPersonalStudents)
             .HasForeignKey(fk => fk.IdEventIntendedFor)
             .HasConstraintName("FK_HTrEventIntendedForPersonalStudent_HTrEventIntendedFor")
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired();

            builder.HasOne(x => x.Student)
              .WithMany(x => x.HistoryEventIntendedForPersonalStudents)
              .HasForeignKey(fk => fk.IdStudent)
              .HasConstraintName("FK_HTrEventIntendedForPersonalStudent_MsStudent")
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

            base.Configure(builder);
        }
    }
}
