using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtDesignation : AuditNoUniqueEntity, IEmployeeEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdDesignation { get; set; }
        public string DesignationDescription { get; set; }

        public virtual ICollection<MsStaff> Staff { get; set; }
    }
    internal class LtDesignationConfiguration : AuditNoUniqueEntityConfiguration<LtDesignation>
    {
        public override void Configure(EntityTypeBuilder<LtDesignation> builder)
        {
            builder.HasKey(x => x.IdDesignation);
      
            builder.Property(x => x.DesignationDescription)
                .HasMaxLength(20);

            base.Configure(builder);
        }
    }
}
