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
    public class MsDigitalPickupSetting : AuditEntity, IStudentEntity
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public virtual MsAcademicYear AcademicYear { get; set; }
        public virtual MsGrade Grade { get; set; }
    }

    internal class MsDigitalPickupSettingConfiguration : AuditEntityConfiguration<MsDigitalPickupSetting>
    {
        public override void Configure(EntityTypeBuilder<MsDigitalPickupSetting> builder)
        {
            builder.Property(x => x.IdAcademicYear)
                .HasMaxLength(36);

            builder.Property(x => x.IdGrade)
                .HasMaxLength(36);

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.DigitalPickupSettings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdAcademicYear)
                .HasConstraintName("FK_MsAcademicYear_MsDigitalPickupSetting")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Grade)
                .WithMany(x => x.DigitalPickupSettings)
                .IsRequired()
                .HasForeignKey(fk => fk.IdGrade)
                .HasConstraintName("FK_MsGrade_MsDigitalPickupSetting")
                .OnDelete(DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
