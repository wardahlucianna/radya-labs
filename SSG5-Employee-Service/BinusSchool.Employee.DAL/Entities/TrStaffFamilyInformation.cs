using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class TrStaffFamilyInformation : AuditNoUniqueEntity, IEmployeeEntity
    {
        public string IdStaffFamily { get; set; } 
        public string IdBinusian { get; set; } 
        public string FamilyName { get; set; } 
        public string RelationshipStatus { get; set; } 
        public string OccupationName { get; set; } 

        public virtual MsStaff Staff { get; set; }

    }
    internal class TrStaffFamilyInformationConfiguration : AuditNoUniqueEntityConfiguration<TrStaffFamilyInformation>
    {
        public override void Configure(EntityTypeBuilder<TrStaffFamilyInformation> builder)
        {
            builder.HasKey(x => x.IdStaffFamily);
         
            builder.Property(x => x.IdStaffFamily)
                .HasMaxLength(36);

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);        

            builder.Property(x => x.FamilyName)
                .HasMaxLength(100);   

            builder.Property(x => x.RelationshipStatus)
                .HasMaxLength(50);   

            builder.Property(x => x.OccupationName)
                .HasMaxLength(50);      

            builder.HasOne(x => x.Staff)
                .WithMany( y => y.StaffFamilyInformation)
                .HasForeignKey( fk => fk.IdBinusian)
                .HasConstraintName("FK_TrStaffFamilyInformation_MsStaff")
                .OnDelete(DeleteBehavior.Cascade);                

            base.Configure(builder);          
        }
    }
}
