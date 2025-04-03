using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtBloodType : AuditEntity, IStudentEntity
    {
        public string BloodTypeName { get; set; }
        public string Description { get; set; }
         public virtual ICollection<MsStudent> Student { get; set; }
    }

    internal class LtBloodTypeConfiguration : AuditEntityConfiguration<LtBloodType>
    {
        public override void Configure(EntityTypeBuilder<LtBloodType> builder)
        {
            builder.Property(x => x.BloodTypeName)
                .HasMaxLength(30);

            builder.Property(x => x.Description)
                .HasMaxLength(128);
                
            base.Configure(builder);

        }
    }
}
