using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtSpecialTreatmentsSkillsLevel : AuditNoUniqueEntity, IEmployeeEntity
    {
        public string IdExpSpecialTreatments { get; set; }
        public string ExpSpecialTreatmentsEngName { get; set; }
        public string ExpSpecialTreatmentsIndName { get; set; }

        public virtual ICollection<MsStaffJobInformation> StaffJobInformation { get; set; }
    }
    internal class LtSpecialTreatmentsSkillsLevelConfiguration : AuditNoUniqueEntityConfiguration<LtSpecialTreatmentsSkillsLevel>
    {
        public override void Configure(EntityTypeBuilder<LtSpecialTreatmentsSkillsLevel> builder)
        {
            builder.HasKey(x => x.IdExpSpecialTreatments);

            builder.Property(x => x.IdExpSpecialTreatments)                
                .HasMaxLength(36);

            builder.Property(x => x.ExpSpecialTreatmentsEngName)                
                .HasMaxLength(50);

            builder.Property(x => x.ExpSpecialTreatmentsIndName)                
                .HasMaxLength(50);        

            base.Configure(builder);
        }

    }
}
