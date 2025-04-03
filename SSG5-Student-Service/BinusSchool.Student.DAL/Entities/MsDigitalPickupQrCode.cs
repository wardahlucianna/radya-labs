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
    public class MsDigitalPickupQrCode : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdStudent { get; set; }
        public DateTime ActiveDate { get; set; }
        public DateTime? LastActiveDate { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsGrade Grade { get; set; }
        public virtual MsStudent Student { get; set; }
    }

    internal class MsDigitalPickupQrCodeConfiguration : AuditEntityConfiguration<MsDigitalPickupQrCode>
    {
        public override void Configure(EntityTypeBuilder<MsDigitalPickupQrCode> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36);

            builder.Property(x => x.IdGrade)
                .HasMaxLength(36);

            builder.Property(x => x.IdStudent)
                .HasMaxLength(36);

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.DigitalPickupQrCodes)
                .IsRequired()
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsAcademicYear_MsDigitalPickupQrCode")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.DigitalPickupQrCodes)
                .IsRequired()
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsGrade_MsDigitalPickupQrCode")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.DigitalPickupQrCodes)
                .IsRequired()
                .HasForeignKey(fk => fk.IdStudent)
                .HasConstraintName("FK_MsStudent_MsDigitalPickupQrCode")
                .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
