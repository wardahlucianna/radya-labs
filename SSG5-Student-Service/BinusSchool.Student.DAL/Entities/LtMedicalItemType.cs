using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtMedicalItemType : AuditEntity, IStudentEntity
    {
        public string MedicalItemTypeName { get; set; }
        public virtual ICollection<MsMedicalItem> MedicalItems { get; set; }
    }

    internal class LtMedicalItemTypeConfiguration : AuditEntityConfiguration<LtMedicalItemType>
    {
        public override void Configure(EntityTypeBuilder<LtMedicalItemType> builder)
        {
            builder.Property(x => x.MedicalItemTypeName)
                .HasMaxLength(100);

            base.Configure(builder);
        }
    }
}
