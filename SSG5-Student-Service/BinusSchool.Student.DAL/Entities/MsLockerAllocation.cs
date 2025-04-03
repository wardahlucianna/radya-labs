using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsLockerAllocation : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdBuilding { get; set; }
        public string IdFloor { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsBuilding Building { get; set; }
        public virtual MsFloor Floor { get; set; }
    }

    internal class MsLockerAllocationConfiguration : AuditEntityConfiguration<MsLockerAllocation>
    {
        public override void Configure(EntityTypeBuilder<MsLockerAllocation> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.LockerAllocations)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsLockerAllocation_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Level)
                .WithMany(x => x.LockerAllocations)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsLockerAllocation_MsLevel")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.LockerAllocations)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsLockerAllocation_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Building)
                .WithMany(x => x.LockerAllocations)
                .HasForeignKey(fk => fk.IdBuilding)
                .HasConstraintName("FK_MsLockerAllocation_MsBuilding")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Floor)
                .WithMany(x => x.LockerAllocations)
                .HasForeignKey(fk => fk.IdFloor)
                .HasConstraintName("FK_MsLockerAllocation_MsFloor")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
