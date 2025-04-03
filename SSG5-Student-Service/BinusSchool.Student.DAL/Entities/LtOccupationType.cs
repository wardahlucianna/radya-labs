using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtOccupationType : AuditEntity, IStudentEntity
    {
         public string OccupationTypeName { get; set; }
         public string OccupationTypeNameEng { get; set; }
         public virtual ICollection<MsParent> Parent { get; set; }
    }

    internal class LtOccupationTypeConfiguration : AuditEntityConfiguration<LtOccupationType>
    {
        public override void Configure(EntityTypeBuilder<LtOccupationType> builder)
        {
            builder.Property(x => x.OccupationTypeName)
                .HasMaxLength(30);

            builder.Property(x => x.OccupationTypeNameEng)
                .HasMaxLength(30);
                
            base.Configure(builder);

        }
    }
}
