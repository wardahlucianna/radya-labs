using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEvIntendedForAtdCheckStudent : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdHTrEventIntendedForAtdStudent { get; set; }
        public string CheckName { get; set; }
        public TimeSpan Time { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public virtual HTrEventIntendedForAtdStudent EventIntendedForAttendanceStudent { get; set; }

    }

    internal class HTrEvIntendedForAtdCheckStudentConfiguration : AuditNoUniqueEntityConfiguration<HTrEvIntendedForAtdCheckStudent>
    {
        public override void Configure(EntityTypeBuilder<HTrEvIntendedForAtdCheckStudent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEvIntendedForAtdCheckStudent).Name)
                .HasMaxLength(36);

            builder.Property(x => x.CheckName).IsRequired().HasMaxLength(50);

            builder.HasOne(x => x.EventIntendedForAttendanceStudent)
           .WithMany(x => x.EvIntendedForAtdCheckStudent)
           .HasForeignKey(fk => fk.IdHTrEventIntendedForAtdStudent)
           .HasConstraintName("FK_HTrEventIntendedForAtdCheckStudent_HTrEventIntendedForAtdStudent")
           .OnDelete(DeleteBehavior.Restrict)
           .IsRequired();

            base.Configure(builder);
        }
    }
}
