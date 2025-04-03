using System;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class TrStaffCertificationInformation : AuditNoUniqueEntity, IEmployeeEntity
    {
        public string IdCertificationStaff { get; set; } 
        public string IdBinusian { get; set; } 
        public int IdCertificationType { get; set; }
        public string CertificationNumber { get; set; }
        public string CertificationName { get; set; }
        public string CertificationYear { get; set; }
        public string IssuedCertifInstitution { get; set; }
        public DateTime CertificationExpDate { get; set; }

        public virtual MsStaff Staff { get; set; }
        public virtual LtCertificationType CertificationType { get; set; }

    }

    internal class TrStaffCertificationInformationConfiguration : AuditNoUniqueEntityConfiguration<TrStaffCertificationInformation>
    {
        public override void Configure(EntityTypeBuilder<TrStaffCertificationInformation> builder)
        {
            builder.HasKey(x => x.IdCertificationStaff);
         
            builder.Property(x => x.IdCertificationStaff)
                .HasMaxLength(36);

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);    

            builder.Property(x => x.CertificationNumber)
                .HasMaxLength(50);

            builder.Property(x => x.CertificationName)
                .HasMaxLength(100);    

            builder.Property(x => x.CertificationYear)
                .HasColumnType("VARCHAR(4)");   

            builder.Property(x => x.IssuedCertifInstitution)
                .HasMaxLength(50);

            builder.HasOne(x => x.Staff)
                .WithMany( y => y.StaffCertificationInformation)
                .HasForeignKey( fk => fk.IdBinusian)
                .HasConstraintName("FK_TrStaffCertificationInformation_MsStaff")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CertificationType)
                .WithMany( y => y.StaffCertificationInformation)
                .HasForeignKey( fk => fk.IdCertificationType)
                .HasConstraintName("FK_TrStaffCertificationInformation_LtCertificationType")
                .OnDelete(DeleteBehavior.Restrict);    
        

            base.Configure(builder);      
        }

    }
}
