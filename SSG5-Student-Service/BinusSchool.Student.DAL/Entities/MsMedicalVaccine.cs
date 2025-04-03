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
    public class MsMedicalVaccine : AuditEntity, IStudentEntity
    {
        public string MedicalVaccineName { get; set; }
        public string IdDosageType { get; set; }
        public int DosageAmount { get; set; }
        public string IdSchool { get; set; }
        public virtual LtDosageType DosageType { get; set; }
        public virtual MsSchool School { get; set; }
    }

    internal class MsMedicalVaccineConfiguration : AuditEntityConfiguration<MsMedicalVaccine>
    {
        public override void Configure(EntityTypeBuilder<MsMedicalVaccine> builder)
        {
            builder.HasOne(x => x.DosageType)
                .WithMany(x => x.MedicalVaccines)
                .HasForeignKey(fk => fk.IdDosageType)
                .HasConstraintName("FK_LtDosageType_MsMedicalVaccine")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.MedicalVaccines)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchool_MsMedicalVaccine")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.MedicalVaccineName)
                .HasMaxLength(100);

            base.Configure(builder);
        }
    }
}
