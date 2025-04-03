using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtMedicineCategory : AuditEntity, IStudentEntity
    {
        public string MedicineCategoryName { get; set; }
        public virtual ICollection<MsMedicalItem> MedicalItems { get; set; }
    }

    internal class LtMedicineCategoryConfiguration : AuditEntityConfiguration<LtMedicineCategory>
    {
        public override void Configure(EntityTypeBuilder<LtMedicineCategory> builder)
        {
            builder.Property(x => x.MedicineCategoryName)
                .HasMaxLength(100);

            base.Configure(builder);
        }
    }
}
