using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class LtNationality : AuditEntity, IStudentEntity
    {
        public string NationalityName { get; set; }
        
        public virtual ICollection<MsStudent> Student { get; set; }
        public virtual ICollection<MsParent> Parent { get; set; }
    }
    internal class LtNationalityConfiguration : AuditEntityConfiguration<LtNationality>
    {
        public override void Configure(EntityTypeBuilder<LtNationality> builder)
        {   
            builder.Property(x => x.NationalityName)                
                .HasMaxLength(50);
          
            base.Configure(builder);
        }

    }
}
