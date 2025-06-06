﻿using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities
{
    public class MsMappingAttendanceWorkhabit : AuditEntity, IAttendanceEntity
    {
        public string IdMappingAttendance {get;set;}
        public string IdWorkhabit {get;set;}
        public virtual MsMappingAttendance MappingAttendance {get;set;}
        public virtual ICollection<TrAttendanceEntryWorkhabit> AttendanceEntryWorkhabits {get;set;}
        public virtual ICollection<TrUserEventAttendanceWorkhabit> UserEventAttendanceWorkhabits { get; set; }
        public virtual ICollection<TrUserEventAttendanceWorkhabit2> UserEventAttendanceWorkhabit2s { get; set; }
        public virtual ICollection<TrAttendanceEntryWorkhabitV2> AttendanceEntryWorkhabitV2s { get; set; }
        public virtual MsWorkhabit Workhabit {get;set;}
    }

    internal class MsMappingAttendanceWorkhabitConfiguration : AuditEntityConfiguration<MsMappingAttendanceWorkhabit>
    {
        public override void Configure(EntityTypeBuilder<MsMappingAttendanceWorkhabit> builder)
        {
            builder.HasOne(x => x.MappingAttendance)
                .WithMany(x => x.MappingAttendanceWorkhabits)
                .HasForeignKey(fk => fk.IdMappingAttendance)
                .HasConstraintName("FK_MsMappingAttendanceWorkHabit_MsMappingAttendance")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Workhabit)
                .WithMany(x => x.MappingAttendanceWorkhabits)
                .HasForeignKey(fk => fk.IdWorkhabit)
                .HasConstraintName("FK_MsMappingAttendanceWorkHabit_MsWorkhabit")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
