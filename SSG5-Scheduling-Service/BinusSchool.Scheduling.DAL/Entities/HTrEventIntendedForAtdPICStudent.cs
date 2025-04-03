using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchedulingDb.Entities
{
    public class HTrEventIntendedForAtdPICStudent : AuditNoUniqueEntity, ISchedulingEntity
    {
        public string IdHTrEventIntendedForAtdPICStudent { get; set; }

        public string IdEventIntendedForAttendanceStudent { get; set; }
        public EventIntendedForAttendancePICStudent Type { get; set; }
        public string IdUser { get; set; }
        public MsUser User { get; set; }

        public virtual HTrEventIntendedForAtdStudent EventIntendedForAttendanceStudent { get; set; }
    }

    internal class HsTrEventIntendedForAtdPICStudentConfiguration : AuditNoUniqueEntityConfiguration<HTrEventIntendedForAtdPICStudent>
    {
        public override void Configure(EntityTypeBuilder<HTrEventIntendedForAtdPICStudent> builder)
        {
            builder.HasKey(x => x.IdHTrEventIntendedForAtdPICStudent);

            builder.Property(p => p.IdHTrEventIntendedForAtdPICStudent)
                .HasColumnName("Id" + typeof(HTrEventIntendedForAtdPICStudent).Name)
                .HasMaxLength(36);

            builder.Property(x => x.Type)
               .HasConversion<string>()
               .HasMaxLength(16)
               .IsRequired();

            builder.HasOne(x => x.EventIntendedForAttendanceStudent)
                .WithMany(x => x.HTrEventIntendedForAtdPICStudent)
                .HasForeignKey(fk => fk.IdEventIntendedForAttendanceStudent)
                .HasConstraintName("FK_HTrEventIntendedForAtdPICStudent_HTrEventIntendedForAtdStudent")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(x => x.HTrEventIntendedForAtdPICStudent)
                .HasForeignKey(fk => fk.IdUser)
                .HasConstraintName("FK_HTrEventIntendedForAtdPICStudent_MsUser")
                .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
