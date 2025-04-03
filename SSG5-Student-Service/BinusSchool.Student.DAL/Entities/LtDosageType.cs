using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtDosageType : AuditEntity, IStudentEntity
    {
        public string DosageTypeName { get; set; }
        public string DosageTypeMeasurement { get; set; }
        public virtual ICollection<MsMedicalItem> MedicalItems { get; set; }
        public virtual ICollection<MsMedicalVaccine> MedicalVaccines { get; set; }
    }

    internal class LtDosageTypeConfiguration : AuditEntityConfiguration<LtDosageType>
    {
        public override void Configure(EntityTypeBuilder<LtDosageType> builder)
        {
            builder.Property(x => x.DosageTypeName)
                .HasMaxLength(100);

            builder.Property(x => x.DosageTypeMeasurement)
                .HasMaxLength(36);

            base.Configure(builder);
        }
    }
}
