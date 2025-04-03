using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtMedicineType : AuditEntity, IStudentEntity
    {
        public string MedicineTypeName { get; set; }
        public virtual ICollection<MsMedicalItem> MedicalItems { get; set; }
    }

    internal class LtMedicineTypeConfiguration : AuditEntityConfiguration<LtMedicineType>
    {
        public override void Configure(EntityTypeBuilder<LtMedicineType> builder)
        {
            builder.Property(x => x.MedicineTypeName)
                .HasMaxLength(100);

            base.Configure(builder);
        }
    }
}
