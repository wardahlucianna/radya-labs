using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrStudentLockerReservation : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdLocker { get; set; }
        public string IdReserver { get; set; }
        public bool IsAgree { get; set; }
        public string Notes { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsStudent Student { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsHomeroom Homeroom { get; set; }
        public virtual MsLocker Locker { get; set; }
    }

    internal class TrStudentLockerReservationConfiguration : AuditEntityConfiguration<TrStudentLockerReservation>
    {
        public override void Configure(EntityTypeBuilder<TrStudentLockerReservation> builder)
        {
            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.StudentLockerReservations)
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_TrStudentLockerReservation_MsAcademicYear")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.StudentLockerReservations)
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_TrStudentLockerReservation_MsStudent")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.StudentLockerReservations)
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_TrStudentLockerReservation_MsGrade")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.Homeroom)
                .WithMany(x => x.StudentLockerReservations)
                .HasForeignKey(fk => fk.IdHomeroom)
                .HasConstraintName("FK_TrStudentLockerReservation_MsHomeroom")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.HasOne(x => x.Locker)
                .WithMany(x => x.StudentLockerReservations)
                .HasForeignKey(fk => fk.IdLocker)
                .HasConstraintName("FK_TrStudentLockerReservation_MsLocker")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.IdReserver)
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            base.Configure(builder);
        }
    }
}
