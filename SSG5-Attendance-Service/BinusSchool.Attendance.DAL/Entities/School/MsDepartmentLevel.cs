﻿using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.AttendanceDb.Entities.School
{
    public class MsDepartmentLevel : AuditEntity, IAttendanceEntity
    {
        public string IdDepartment { get; set; }
        public string IdLevel { get; set; }

        public virtual MsDepartment Department { get; set; }
        public virtual MsLevel Level { get; set; }
    }

    internal class MsDepartmentLevelConfiguration : AuditEntityConfiguration<MsDepartmentLevel>
    {
        public override void Configure(EntityTypeBuilder<MsDepartmentLevel> builder)
        {
            builder.HasOne(x => x.Department)
                .WithMany(x => x.DepartmentLevels)
                .HasForeignKey(fk => fk.IdDepartment)
                .HasConstraintName("FK_MsDepartmentLevel_MsDepartment")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();


            builder.HasOne(x => x.Level)
                .WithMany(x => x.DepartmentLevels)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsDepartmentLevel_MsLevel")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
