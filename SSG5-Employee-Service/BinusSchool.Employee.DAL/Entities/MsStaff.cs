using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.EmployeeDb.Entities
{
    public class MsStaff : AuditNoUniqueEntity, IEmployeeEntity
    {
        public string IdBinusian { get; set; }    
        public string ShortName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        
        public string IdSchool { get; set; }
        public int IdDesignation { get; set; }
        public string IdEmployee { get; set; }
        public int IdStaffStatus { get; set; }
        public string InitialName { get; set; }
        public string POB { get; set; }
        public DateTime DOB { get; set; }
        public string GenderName { get; set; }
        public string IdNationality { get; set; }
        public string IdNationalityCountry { get; set; }
        public string IdReligion { get; set; }
        public string NIK { get; set; } //temporarily unused
        public string PassportNumber { get; set; }
        public DateTime? PassportExpDate { get; set; }
        public string MotherMaidenName { get; set; } // temporarily unused
        public string KITASNumber { get; set; }
        public string KITASSponsor { get; set; }
        public string IdKITASStatus { get; set; }
        public DateTime? KITASExpDate { get; set; }
        public string IMTANumber { get; set; }
        public string IdIMTASchoolLevel { get; set; }
        public string IdIMTAMajorAssignPosition { get; set; }
        public DateTime? IMTAExpDate { get; set; }
        public string ResidenceAddress { get; set; }
        public string AddressCity { get; set; }
        public string PostalCode { get; set; }
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }        
        public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public bool MaritalStatus { get; set; }

        public virtual ICollection<TrStaffEducationInformation> StaffEducationInformation { get; set; }
        public virtual MsStaffJobInformation StaffJobInformation { get; set; }
        public virtual ICollection<TrStaffCertificationInformation> StaffCertificationInformation { get; set; }
        public virtual LtDesignation Designation { get; set; }
        public virtual ICollection<TrStaffFamilyInformation> StaffFamilyInformation { get; set; }
        public virtual LtStaffStatus StaffStatus { get; set; }
        public virtual LtKITASStatus KITASStatus { get; set; }
        public virtual LtIMTAMajorAssignPosition IMTAMajorAssignPosition { get; set; }
        public virtual LtIMTASchoolLevel IMTASchoolLevel { get; set; }
        public virtual ICollection<TrStaffInfoUpdate> StaffInfoUpdate { get; set; }
        public virtual LtNationalityCountryHC NationalityCountryHC  { get; set; }
        public virtual LtNationalityHC NationalityHC { get; set; }
        public virtual LtReligionHC ReligionHC { get; set; }

    }

    internal class MsStaffConfiguration : AuditNoUniqueEntityConfiguration<MsStaff>
    {
        public override void Configure(EntityTypeBuilder<MsStaff> builder)
        {

            builder.HasKey(x => x.IdBinusian);
          
            builder.Property(x => x.IdBinusian)
                .HasMaxLength(36);    

            builder.Property(x => x.ShortName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.FirstName)
                .HasMaxLength(100)
                .IsRequired();
         
            builder.Property(x => x.LastName)
               .HasMaxLength(100);

            builder.Property(x => x.IdSchool)
               .HasMaxLength(36);

            builder.Property(x => x.IdEmployee)
               .HasMaxLength(36);   

            builder.Property(x => x.InitialName)
               .HasMaxLength(5);   

            builder.Property(x => x.POB)
               .HasMaxLength(50);  

            builder.Property(x => x.DOB)              
                .HasColumnType(typeName: "datetime2");    

            builder.Property(x => x.GenderName)
               .HasMaxLength(30);      

            builder.Property(x => x.IdNationality)
               .HasMaxLength(10);       

            builder.Property(x => x.IdNationalityCountry)
               .HasMaxLength(50);            

            builder.Property(x => x.IdReligion)
               .HasMaxLength(36);       

            builder.Property(x => x.NIK)
               .HasMaxLength(50);    

            builder.Property(x => x.PassportNumber)
               .HasMaxLength(50);     

            builder.Property(x => x.PassportExpDate)
               .HasColumnType(typeName: "datetime2");           
         
            builder.Property(x => x.MotherMaidenName)
               .HasMaxLength(200);     

            builder.Property(x => x.KITASNumber) 
                .HasMaxLength(50); 

            builder.Property(x => x.KITASSponsor) 
                .HasMaxLength(100);      

            builder.Property(x => x.IdKITASStatus) 
                .HasMaxLength(36); 

            builder.Property(x => x.KITASExpDate)
               .HasColumnType(typeName: "datetime2");         

            builder.Property(x => x.IMTANumber) 
                .HasMaxLength(50);

            builder.Property(x => x.IdIMTASchoolLevel) 
                .HasMaxLength(36);        

            builder.Property(x => x.IdIMTAMajorAssignPosition) 
                .HasMaxLength(36);    

            builder.Property(x => x.IMTAExpDate)
               .HasColumnType(typeName: "datetime2");       

            builder.Property(x => x.ResidenceAddress)
               .HasMaxLength(300);   

            builder.Property(x => x.AddressCity)
               .HasMaxLength(100);    

            builder.Property(x => x.PostalCode)
               .HasMaxLength(20);     

            builder.Property(x => x.ResidencePhoneNumber)
               .HasMaxLength(25);    

            builder.Property(x => x.MobilePhoneNumber1)
               .HasMaxLength(25);           

            builder.Property(x => x.BinusianEmailAddress)
               .HasMaxLength(60);         

            builder.Property(x => x.PersonalEmailAddress)
               .HasMaxLength(60);       

            builder.HasOne(x => x.Designation)
                .WithMany( y => y.Staff)
                .HasForeignKey( fk => fk.IdDesignation)
                .HasConstraintName("FK_MsStaff_LtDesignation")
                .OnDelete(DeleteBehavior.Restrict);        

            builder.HasOne(x => x.StaffStatus)
                .WithMany( y => y.Staff)
                .HasForeignKey( fk => fk.IdStaffStatus)
                .HasConstraintName("FK_MsStaff_LtStaffStatus")
                .OnDelete(DeleteBehavior.Restrict);  

            builder.HasOne(x => x.KITASStatus)
                .WithMany( y => y.Staff)
                .HasForeignKey( fk => fk.IdKITASStatus)
                .HasConstraintName("FK_MsStaff_LtKITASStatus")
                .OnDelete(DeleteBehavior.Restrict);    

            builder.HasOne(x => x.IMTAMajorAssignPosition)
                .WithMany( y => y.Staff)
                .HasForeignKey( fk => fk.IdIMTAMajorAssignPosition)
                .HasConstraintName("FK_MsStaff_LtIMTAMajorAssignPosition")
                .OnDelete(DeleteBehavior.Restrict);   

            builder.HasOne(x => x.IMTASchoolLevel)
                .WithMany( y => y.Staff)
                .HasForeignKey( fk => fk.IdIMTASchoolLevel)
                .HasConstraintName("FK_MsStaff_LtIMTASchoolLevel")
                .OnDelete(DeleteBehavior.Restrict);          

            builder.HasOne(x => x.NationalityCountryHC)
                .WithMany( y => y.Staff)
                .HasForeignKey( fk => fk.IdNationalityCountry)
                .HasConstraintName("FK_MsStaff_LtNationalityCountryHC")
                .OnDelete(DeleteBehavior.Restrict);        

            builder.HasOne(x => x.NationalityHC)
                .WithMany( y => y.Staff)
                .HasForeignKey( fk => fk.IdNationality)
                .HasConstraintName("FK_MsStaff_LtNationalityHC")
                .OnDelete(DeleteBehavior.Restrict);      

            builder.HasOne(x => x.ReligionHC)
                .WithMany( y => y.Staff)
                .HasForeignKey( fk => fk.IdReligion)
                .HasConstraintName("FK_MsStaff_LtReligionHC")
                .OnDelete(DeleteBehavior.Restrict);           

            base.Configure(builder);
        }
    }
}
