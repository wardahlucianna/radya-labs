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
    public class TrMedicalRecordEntry : AuditEntity, IStudentEntity
    {
        public string IdUser { get; set; }
        public DateTime CheckInDateTime { get; set; }
        public DateTime? CheckOutDateTime { get; set; }
        public bool DismissedHome { get; set; }
        public string? IdMedicalHospital { get; set; }
        public string? TeacherInCharge { get; set; }
        public string? Location { get; set; }
        public string? DetailsNotes { get; set; }
        public string IdSchool { get; set; }
        public virtual MsMedicalHospital MedicalHospital { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrMedicalRecordConditionDetails> MedicalRecordConditionDetails { get; set; }
        public virtual ICollection<TrMedicalRecordTreatmentDetails> MedicalRecordTreatmentDetails { get; set; }
        public virtual ICollection<TrMedicalRecordMedicationDetails> MedicalRecordMedicationDetails { get; set; }
    }

    internal class TrMedicalRecordEntryConfiguration : AuditEntityConfiguration<TrMedicalRecordEntry>
    {
        public override void Configure(EntityTypeBuilder<TrMedicalRecordEntry> builder)
        {
            //builder.Property(x => x.CheckInDateTime)
            //    .HasColumnType("datetime2")
            //    .HasMaxLength(7)
            //    .IsRequired();

            builder.Property(x => x.TeacherInCharge)
                .HasMaxLength(36);

            builder.Property(x => x.Location)
                .HasMaxLength(100);

            builder.Property(x => x.DetailsNotes)
                .HasMaxLength(500);

            builder.HasOne(x => x.MedicalHospital)
                .WithMany(x => x.MedicalRecordEntries)
                .HasForeignKey(fk => fk.IdMedicalHospital)
                .HasConstraintName("FK_MsMedicalHospital_TrMedicalRecordEntry")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.School)
                .WithMany(x => x.MedicalRecordEntries)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchool_TrMedicalRecordEntry")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
