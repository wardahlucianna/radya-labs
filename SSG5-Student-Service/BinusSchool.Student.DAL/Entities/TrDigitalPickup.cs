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
    public class TrDigitalPickup : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public DateTime Date { get; set; }
        public string IdStudent { get; set; }
        public DateTime QrScanTime { get; set; }
        public DateTime? PickupTime { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsStudent Student { get; set; }
    }

    internal class TrDigitalPickupConfiguration : AuditEntityConfiguration<TrDigitalPickup>
    {
        public override void Configure(EntityTypeBuilder<TrDigitalPickup> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36);

            builder.Property(x => x.IdStudent)
                .HasMaxLength(36);

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.DigitalPickups)
                .IsRequired()
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsAcademicYear_TrDigitalPickup")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.DigitalPickups)
                .IsRequired()
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsStudent_TrDigitalPickup")
                .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
