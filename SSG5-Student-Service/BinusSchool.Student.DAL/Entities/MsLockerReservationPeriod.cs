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
    public class MsLockerReservationPeriod : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PolicyMessage { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsLevel Level { get; set; }
        public virtual MsGrade Grade { get; set; }
    }

    internal class MsLockerReservationPeriodConfiguration : AuditEntityConfiguration<MsLockerReservationPeriod>
    {
        public override void Configure(EntityTypeBuilder<MsLockerReservationPeriod> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.LockerReservationPeriods)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsLockerReservationPeriod_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Level)
                .WithMany(x => x.LockerReservationPeriods)
                .HasForeignKey(fk => fk.IdLevel)
                .HasConstraintName("FK_MsLockerReservationPeriod_MsLevel")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.LockerReservationPeriods)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsLockerReservationPeriod_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.StartDate)
                .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.EndDate)
                .HasColumnType(typeName: "datetime2");

            builder.Property(x => x.PolicyMessage)
                .HasMaxLength(int.MaxValue);

            base.Configure(builder);
        }
    }
}
