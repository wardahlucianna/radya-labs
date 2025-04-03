using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class TrStaffEducationInformation : AuditNoUniqueEntity, IEmployeeEntity
    {
        public string IdStaffEducation { get; set; } 
        public string IdBinusian { get; set; } 
        public string IdEducationLevel { get; set; } 
        public string InstitutionName { get; set; } 
        public string MajorName { get; set; } 
        public string AttendingYear { get; set; } 
        public string GraduateYear { get; set; } 
        public decimal GPA { get; set; } 
        public virtual MsStaff Staff { get; set; }
        public virtual LtEducationLevel EducationLevel { get; set; }
    }
    internal class TrStaffEducationInformationConfiguration : AuditNoUniqueEntityConfiguration<TrStaffEducationInformation>
    {
        public override void Configure(EntityTypeBuilder<TrStaffEducationInformation> builder)
        {
            builder.HasKey(x => x.IdStaffEducation);
         
            builder.Property(x => x.IdStaffEducation)
                .HasMaxLength(36);

            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);    

            builder.Property(x => x.IdEducationLevel)
                .HasColumnType("VARCHAR(1)");   
          
            builder.Property(x => x.InstitutionName)
                .HasMaxLength(100);   

            builder.Property(x => x.MajorName)
                .HasMaxLength(100);   
                
            builder.Property(x => x.AttendingYear)
                .HasColumnType("VARCHAR(4)");   

            builder.Property(x => x.GraduateYear)
                .HasColumnType("VARCHAR(4)");   

            builder.Property(x => x.GPA) 
                .HasColumnType("DECIMAL(5,2)");   

            builder.HasOne(x => x.Staff)
                .WithMany( y => y.StaffEducationInformation)
                .HasForeignKey( fk => fk.IdBinusian)
                .HasConstraintName("FK_TrStaffEducationInformation_MsStaff")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.EducationLevel)
                .WithMany( y => y.StaffEducationInformation)
                .HasForeignKey( fk => fk.IdEducationLevel)
                .HasConstraintName("FK_TrStaffEducationInformation_LtEducationLevel")
                .OnDelete(DeleteBehavior.Restrict);
    
          

            base.Configure(builder);
        }
    }
}
