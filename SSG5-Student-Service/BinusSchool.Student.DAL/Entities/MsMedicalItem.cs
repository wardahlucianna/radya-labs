using System;
using System.Collections.Generic;
using System.Text;
using NPOI.SS.Formula.Functions;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsMedicalItem : AuditEntity, IStudentEntity
    {
        public string MedicalItemName { get; set; }
        public string IdMedicalItemType { get; set; }
        public bool IsCommonDrug { get; set; }
        public string IdMedicineType { get; set; }
        public string? IdMedicineCategory { get; set; }
        public string? IdDosageType { get; set; }
        public string IdSchool { get; set; }
        public virtual LtMedicalItemType MedicalItemType { get; set; }
        public virtual LtMedicineType MedicineType { get; set; }
        public virtual LtMedicineCategory MedicineCategory { get; set; }
        public virtual LtDosageType DosageType { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrMappingMedicalItemCondition> MappingMedicalItemConditions { get; set; }
        public virtual ICollection<TrMedicalRecordMedicationDetails> MedicalRecordMedicationDetails { get; set; }

    }

    public class MsMedicalItemConfiguration : AuditEntityConfiguration<MsMedicalItem>
    {
        public override void Configure(EntityTypeBuilder<MsMedicalItem> builder)
        {
            builder.HasOne(x => x.MedicalItemType)
                .WithMany(x => x.MedicalItems)
                .HasForeignKey(fk => fk.IdMedicalItemType)
                .HasConstraintName("FK_LtMedicalItemType_MsMedicalItem")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.MedicineType)
                .WithMany(x => x.MedicalItems)
                .HasForeignKey(fk => fk.IdMedicineType)
                .HasConstraintName("FK_LtMedicineType_MsMedicalItem")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.MedicineCategory)
                .WithMany(x => x.MedicalItems)
                .HasForeignKey(fk => fk.IdMedicineCategory)
                .HasConstraintName("FK_LtMedicineCategory_MsMedicalItem")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.DosageType)
                .WithMany(x => x.MedicalItems)
                .HasForeignKey(fk => fk.IdDosageType)
                .HasConstraintName("FK_LtDosageType_MsMedicalItem")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(x => x.School)
                .WithMany(x => x.MedicalItems)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsSchool_MsMedicalItem")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(x => x.MedicalItemName)
                .HasMaxLength(100);


            base.Configure(builder);
        }
    }
}
