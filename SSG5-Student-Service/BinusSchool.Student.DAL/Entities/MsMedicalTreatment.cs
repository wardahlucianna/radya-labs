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
    public class MsMedicalTreatment : AuditEntity, IStudentEntity
    {
        public string MedicalTreatmentName { get; set; }
        public string IdSchool { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrMappingTreatmentCondition> MappingTreatmentConditions { get; set; }
        public virtual ICollection<TrMedicalRecordTreatmentDetails> MedicalRecordTreatmentDetails { get; set; }

    }

    internal class MsMedicalTreatmentConfiguration : AuditEntityConfiguration<MsMedicalTreatment>
    {
        public override void Configure(EntityTypeBuilder<MsMedicalTreatment> builder)
        {
            builder.HasOne(x => x.School)
                .WithMany(x => x.MedicalTreatments)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchool_MsMedicalTreatment")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.MedicalTreatmentName)
                .HasMaxLength(100);

            base.Configure(builder);
        }
    }
}
