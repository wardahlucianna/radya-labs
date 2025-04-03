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
    public class MsLocker : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdBuilding { get; set; }
        public string IdFloor { get; set; }
        public string LockerName { get; set; }
        public string IdLockerPosition { get; set; }
        public bool IsLocked { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsBuilding Building { get; set; }
        public virtual MsFloor Floor { get; set; }
        public virtual LtLockerPosition LockerPosition { get; set; }
        public virtual ICollection<TrStudentLockerReservation> StudentLockerReservations { get; set; }
    }

    internal class MsLockerConfiguration : AuditEntityConfiguration<MsLocker>
    {
        public override void Configure(EntityTypeBuilder<MsLocker> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Lockers)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsLocker_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Building)
                .WithMany(x => x.Lockers)
                .HasForeignKey(fk => fk.IdBuilding)
                .HasConstraintName("FK_MsLocker_MsBuilding")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Floor)
                .WithMany(x => x.Lockers)
                .HasForeignKey(fk => fk.IdFloor)
                .HasConstraintName("FK_MsLocker_MsFloor")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.LockerName)
                .HasMaxLength(36)
                .IsRequired();

            builder.HasOne(x => x.LockerPosition)
                .WithMany(x => x.Lockers)
                .HasForeignKey(fk => fk.IdLockerPosition)
                .HasConstraintName("FK_MsLocker_LtLockerPosition")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
