using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrMedicalRecordMedicationDetails : AuditEntity, IStudentEntity
    {
        public string IdMedicalRecordEntry { get; set; }
        public string IdMedicalItem { get; set; }
        public int DosageAmount { get; set; }
        public virtual TrMedicalRecordEntry MedicalRecordEntry { get; set; }
        public virtual MsMedicalItem MedicalItem { get; set; }
    }
    
    internal class TrMedicalRecordMedicationDetailsConfiguration : AuditEntityConfiguration<TrMedicalRecordMedicationDetails>
    {
        public override void Configure(EntityTypeBuilder<TrMedicalRecordMedicationDetails> builder)
        {
            builder.HasOne(x => x.MedicalRecordEntry)
                .WithMany(x => x.MedicalRecordMedicationDetails)
                .HasForeignKey(fk => fk.IdMedicalRecordEntry)
                .HasConstraintName("FK_TrMedicalRecordEntry_TrMedicalRecordMedicationDetails")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.MedicalItem)
                .WithMany(x => x.MedicalRecordMedicationDetails)
                .HasForeignKey(fk => fk.IdMedicalItem)
                .HasConstraintName("FK_MsMedicalItem_TrMedicalRecordMedicationDetails")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
