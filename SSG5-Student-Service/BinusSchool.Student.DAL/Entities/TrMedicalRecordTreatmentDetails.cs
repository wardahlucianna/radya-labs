using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class TrMedicalRecordTreatmentDetails : AuditEntity, IStudentEntity
    {
        public string IdMedicalRecordEntry { get; set; }
        public string IdMedicalTreatment { get; set; }
        public virtual TrMedicalRecordEntry MedicalRecordEntry { get; set; }
        public virtual MsMedicalTreatment MedicalTreatment { get; set; }
    }
    
    internal class TrMedicalRecordTreatmentDetailsConfiguration : AuditEntityConfiguration<TrMedicalRecordTreatmentDetails>
    {
        public override void Configure(EntityTypeBuilder<TrMedicalRecordTreatmentDetails> builder)
        {
            builder.HasOne(x => x.MedicalRecordEntry)
                .WithMany(x => x.MedicalRecordTreatmentDetails)
                .HasForeignKey(fk => fk.IdMedicalRecordEntry)
                .HasConstraintName("FK_TrMedicalRecordEntry_TrMedicalRecordTreatmentDetails")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.MedicalTreatment)
                .WithMany(x => x.MedicalRecordTreatmentDetails)
                .HasForeignKey(fk => fk.IdMedicalTreatment)
                .HasConstraintName("FK_MsMedicalTreatment_TrMedicalRecordTreatmentDetails")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            base.Configure(builder);
        }
    }
}
