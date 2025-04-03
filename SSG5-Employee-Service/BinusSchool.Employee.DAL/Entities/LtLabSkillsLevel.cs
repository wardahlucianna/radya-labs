using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtLabSkillsLevel : AuditEntity, IEmployeeEntity
    {
        public string LabSkillsLevelIndName { get; set; }
        public string LabSkillsLevelEngName { get; set; }

         public virtual ICollection<MsStaffJobInformation> StaffJobInformation { get; set; }
    }
    internal class LtLabSkillsLevelConfiguration : AuditEntityConfiguration<LtLabSkillsLevel>
    {
        public override void Configure(EntityTypeBuilder<LtLabSkillsLevel> builder)
        {   
            builder.Property(x => x.LabSkillsLevelIndName)                
                .HasMaxLength(50);

            builder.Property(x => x.LabSkillsLevelEngName)                
                .HasMaxLength(50);    

            base.Configure(builder);
        }
    }
}
