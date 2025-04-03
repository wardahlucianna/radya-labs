using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtReligion : AuditEntity, IStudentEntity
    {
        public string ReligionName { get; set; }

        public virtual ICollection<MsStudent> Student { get; set; }
        public virtual ICollection<MsParent> Parent { get; set; }
    }

    internal class LtReligionConfiguration : AuditEntityConfiguration<LtReligion>
    {
        public override void Configure(EntityTypeBuilder<LtReligion> builder)
        {   
            builder.Property(x => x.ReligionName)                
                .HasMaxLength(36);
          
            base.Configure(builder);
        }

    }
}
