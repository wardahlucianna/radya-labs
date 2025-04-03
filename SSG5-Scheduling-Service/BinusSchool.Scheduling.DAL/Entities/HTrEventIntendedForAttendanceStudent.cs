using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventIntendedForAtdStudent : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string Id { get; set; }
        public string IdEventIntendedFor { get; set; }
        public EventIntendedForAttendanceStudent Type { get; set; }
        public bool IsSetAttendance { get; set; }
        public bool IsRepeat { get; set; }
        public virtual HTrEventIntendedFor EventIntendedFor { get; set; }
        public virtual ICollection<HTrEventIntendedForAtdPICStudent> HTrEventIntendedForAtdPICStudent { get; set; }
        public virtual ICollection<HTrEvIntendedForAtdCheckStudent> EvIntendedForAtdCheckStudent { get; set; }
    }
    internal class HTrEventIntendedForAtdStudentConfiguration : AuditNoUniqueEntityConfiguration<HTrEventIntendedForAtdStudent>
    {
        public override void Configure(EntityTypeBuilder<HTrEventIntendedForAtdStudent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Id)
                .HasColumnName("Id" + typeof(HTrEventIntendedForAtdStudent).Name)
                .HasMaxLength(36);

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            builder.HasOne(x => x.EventIntendedFor)
             .WithMany(x => x.EventIntendedForAttendanceStudents)
             .HasForeignKey(fk => fk.IdEventIntendedFor)
             .HasConstraintName("FK_HTrEventIntendedForAtdStudent_HTrEventIntendedFor")
             .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            base.Configure(builder);
        }
    }
}
