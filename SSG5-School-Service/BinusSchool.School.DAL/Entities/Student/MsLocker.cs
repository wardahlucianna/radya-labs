using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.SchoolDb.Entities.Student
{
    public class MsLocker : AuditEntity, ISchoolEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdBuilding { get; set; }
        public string IdFloor { get; set; }
        public string LockerName { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsBuilding Building { get; set; }
        public virtual MsFloor Floor { get; set; }
    }

    internal class MsLockerConfiguration : AuditEntityConfiguration<MsLocker>
    {
        public override void Configure(EntityTypeBuilder<MsLocker> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Lockers)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsAcademicYear_MsLocker")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Building)
                .WithMany(x => x.Lockers)
                .HasForeignKey(fk => fk.IdBuilding)
                .HasConstraintName("FK_MsBuilding_MsLocker")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Floor)
                .WithMany(x => x.Lockers)
                .HasForeignKey(fk => fk.IdFloor)
                .HasConstraintName("FK_MsFloor_MsLocker")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.LockerName)
                .HasMaxLength(36)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
