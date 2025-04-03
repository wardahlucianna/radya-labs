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
    public class MsMedicalDoctor : AuditEntity, IStudentEntity
    {
        public string DoctorName { get; set; }
        public string DoctorAddress { get; set; }
        public string DoctorEmail { get; set; }
        public string DoctorPhoneNumber { get; set; }
        public string IdMedicalHospital { get; set; }
        public virtual MsMedicalHospital MedicalHospital { get; set; }
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsMedicalDoctorConfiguration : AuditEntityConfiguration<MsMedicalDoctor>
    {
        public override void Configure(EntityTypeBuilder<MsMedicalDoctor> builder)
        {
            builder.HasOne(x => x.MedicalHospital)
                .WithMany(x => x.MedicalDoctors)
                .HasForeignKey(fk => fk.IdMedicalHospital)
                .HasConstraintName("FK_MsMedicalHospital_MsMedicalDoctor")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.MedicalDoctors)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchool_MsMedicalDoctor")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.DoctorName)
                .HasMaxLength(100);

            builder.Property(x => x.DoctorAddress)
                .HasMaxLength(100);

            builder.Property(x => x.DoctorEmail)
                .HasMaxLength(100);

            builder.Property(x => x.DoctorPhoneNumber)
                .HasMaxLength(100);

            base.Configure(builder);
        }
    }
}
