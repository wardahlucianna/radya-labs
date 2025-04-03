using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class LtCertificationType : AuditNoUniqueEntity, IEmployeeEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdCertificationType { get; set; }
        public string CertificationTypeDescriptionEng { get; set; }
        public string CertificationTypeDescriptionInd { get; set; }

        public virtual ICollection<TrStaffCertificationInformation> StaffCertificationInformation { get; set; }
    }

    internal class LtCertificationTypeConfiguration : AuditNoUniqueEntityConfiguration<LtCertificationType>
    {
        public override void Configure(EntityTypeBuilder<LtCertificationType> builder)
        {
            builder.HasKey(x => x.IdCertificationType);

            builder.Property(x => x.CertificationTypeDescriptionEng)
                .HasMaxLength(50);

            builder.Property(x => x.CertificationTypeDescriptionInd)
                .HasMaxLength(50);

            base.Configure(builder);
        }

    }
}
