using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtStaffStatus : AuditNoUniqueEntity, IEmployeeEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdStaffStatus { get; set; }
        public string StaffStatusDescription { get; set; }

        public virtual ICollection<MsStaff> Staff { get; set; }
    }
    internal class LtStaffStatusConfiguration : AuditNoUniqueEntityConfiguration<LtStaffStatus>
    {
        public override void Configure(EntityTypeBuilder<LtStaffStatus> builder)
        {
            builder.HasKey(x => x.IdStaffStatus);
      
            builder.Property(x => x.StaffStatusDescription)
                .HasMaxLength(20);

            base.Configure(builder);
        }
    }
}
