using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;

namespace BinusSchool.Persistence.SchedulingDb.Entities.School
{
    public class MsDepartmentLevel : AuditEntity, ISchedulingEntity
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
