using System;
using System.Collections.Generic;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsParent : UserKindStudentParentEntity, IStudentEntity
    {
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        public string IdParentRole { get; set; } 
        public Int16 AliveStatus { get; set; } 
        public string IdReligion { get; set;}
        public int? IdLastEducationLevel { get; set;} 
        public string IdNationality { get; set; }
        public string IdCountry { get; set; }                
        public string FamilyCardNumber { get; set;}
        public string NIK { get; set;}
        public string PassportNumber { get; set;}
        public DateTime? PassportExpDate { get; set;}
        public string KITASNumber { get; set;}
        public DateTime? KITASExpDate { get; set;}
        public Int16 BinusianStatus { get; set;}        
        public string IdBinusian { get; set;}
        public string ParentNameForCertificate { get; set;}
        public string ResidenceAddress { get; set; }
        public string HouseNumber { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public string VillageDistrict { get; set; }
        public string SubDistrict { get; set; }
        public string IdAddressCity { get; set; }
        public string IdAddressStateProvince { get; set; }
        public string IdAddressCountry { get; set; }
        public string PostalCode { get; set; }
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string MobilePhoneNumber2 { get; set; }
        public string MobilePhoneNumber3 { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string WorkEmailAddress { get; set; }
        public string IdOccupationType { get; set; }
        public string OccupationPosition { get; set; }
        public string CompanyName { get; set; }
        public int? IdParentSalaryGroup { get; set; }
        public int? IdParentRelationship { get; set; }



        public virtual ICollection<MsStudentParent> StudentParents { get; set; }
        public virtual LtNationality Nationality { get; set; }
        public virtual LtCountry Country { get; set; }
        //public virtual MsNationalityCountry NationalityCountry { get; set; }
        public virtual LtParentRole ParentRole { get; set; }
        public virtual LtOccupationType OccupationType { get; set; }
        public virtual LtReligion Religion { get; set; }
        public virtual LtParentSalaryGroup ParentSalaryGroup { get; set; }
        public virtual LtAliveStatus LtAliveStatus { get; set; }
        public virtual LtBinusianStatus LtBinusianStatus { get; set; }
        public virtual LtLastEducationLevel LastEducationLevel { get; set; }
        public virtual LtParentRelationship ParentRelationship { get; set; }
        public virtual ICollection<TrCounselingServicesEntry> CounselingServicesEntryMother { get; set; }
        public virtual ICollection<TrCounselingServicesEntry> CounselingServicesEntryFather { get; set; }
        public virtual ICollection<MsSurveyRespondent> SurveyRespondents { get; set; }
        public virtual ICollection<TrStudentExit> StudentExitFathers { get; set; }
        public virtual ICollection<TrStudentExit> StudentExitMothers { get; set; }


    }

    internal class MsParentConfiguration : UserKindStudentParentEntityConfiguration<MsParent>
    {
        public override void Configure(EntityTypeBuilder<MsParent> builder)
        {
            builder.Property(x => x.POB)               
                .HasMaxLength(100);
             
            builder.Property(x => x.DOB)
                .HasColumnType(typeName: "datetime2")
                .IsRequired();        

            builder.Property(x => x.IdParentRole)            
                .HasMaxLength(36);
       
            builder.Property(x => x.IdReligion)                  
                .HasMaxLength(36);            

            // builder.Property(x => x.IdLastEducationLevel)                  
            //     .HasMaxLength(36);   

            builder.Property(x => x.IdNationality) 
                .HasMaxLength(36)
                .IsRequired();   
         
            builder.Property(x => x.FamilyCardNumber) 
                .HasColumnType("VARCHAR(16)")     
                .HasMaxLength(16);  
            
            builder.Property(x => x.NIK) 
                .HasColumnType("VARCHAR(30)")     
                .HasMaxLength(30);     

            builder.Property(x => x.PassportNumber) 
                .HasColumnType("VARCHAR(30)")     
                .HasMaxLength(30); 

            builder.Property(x => x.PassportExpDate)
                .HasColumnType(typeName: "datetime2");      

            builder.Property(x => x.KITASNumber) 
                .HasColumnType("VARCHAR(50)")     
                .HasMaxLength(50); 

            builder.Property(x => x.KITASExpDate)
                .HasColumnType(typeName: "datetime2");     

          
            builder.Property(x => x.ParentNameForCertificate) 
                .HasColumnType("VARCHAR(50)")     
                .HasMaxLength(50); 

            builder.Property(x => x.ResidenceAddress) 
                .HasColumnType("VARCHAR(300)")     
                .HasMaxLength(300);       

            builder.Property(x => x.HouseNumber) 
                .HasColumnType("VARCHAR(15)")     
                .HasMaxLength(15);    

            builder.Property(x => x.RT) 
                .HasColumnType("CHAR(3)")     
                .HasMaxLength(3);   

            builder.Property(x => x.RW) 
                .HasColumnType("CHAR(3)")     
                .HasMaxLength(3);      

             builder.Property(x => x.VillageDistrict) 
                .HasColumnType("VARCHAR(100)")     
                .HasMaxLength(100);     

            builder.Property(x => x.SubDistrict) 
                .HasColumnType("VARCHAR(100)")     
                .HasMaxLength(100);     

            builder.Property(x => x.IdAddressCity) 
                .HasColumnType("VARCHAR(10)")     
                .HasMaxLength(10);  

            builder.Property(x => x.IdAddressStateProvince) 
                .HasColumnType("VARCHAR(10)")     
                .HasMaxLength(10);     

            builder.Property(x => x.IdAddressCountry) 
                .HasColumnType("VARCHAR(10)")     
                .HasMaxLength(10);     

             builder.Property(x => x.PostalCode) 
                .HasColumnType("VARCHAR(10)")     
                .HasMaxLength(10);   

            builder.Property(x => x.ResidencePhoneNumber) 
                .HasColumnType("VARCHAR(25)")     
                .HasMaxLength(25);      

            builder.Property(x => x.MobilePhoneNumber1) 
                .HasColumnType("VARCHAR(25)")     
                .HasMaxLength(25);     

            builder.Property(x => x.MobilePhoneNumber2) 
                .HasColumnType("VARCHAR(25)")     
                .HasMaxLength(25);     

            builder.Property(x => x.MobilePhoneNumber3) 
                .HasColumnType("VARCHAR(25)")     
                .HasMaxLength(25);   

            builder.Property(x => x.PersonalEmailAddress) 
                .HasColumnType("VARCHAR(50)")     
                .HasMaxLength(50); 

            builder.Property(x => x.WorkEmailAddress) 
                .HasColumnType("VARCHAR(50)")     
                .HasMaxLength(50); 

            builder.Property(x => x.IdOccupationType)                
                .HasMaxLength(36); 

            builder.Property(x => x.OccupationPosition) 
                .HasColumnType("VARCHAR(100)")     
                .HasMaxLength(100); 

            builder.Property(x => x.CompanyName) 
                .HasColumnType("VARCHAR(50)")     
                .HasMaxLength(50); 

            // builder.Property(x => x.IdParentSalaryGroup)                
            //     .HasMaxLength(36); 

            builder.Property(x => x.IdBinusian)                
                .HasMaxLength(36);     
                
            builder.HasOne(x => x.Nationality)
                .WithMany( y => y.Parent)
                .HasForeignKey( fk => fk.IdNationality)
                .HasConstraintName("FK_MsParent_LtNasionality")
                .OnDelete(DeleteBehavior.NoAction);    

            // builder.HasOne(x => x.NationalityCountry)
            //     .WithMany( y => y.Parent)
            //     .HasForeignKey( fk => fk.IdNationalityCountry)
            //     .HasConstraintName("FK_MsParent_MsNationalityCountry")
            //     .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Country)
                .WithMany( y => y.Parent)
                .HasForeignKey( fk => fk.IdCountry)
                .HasConstraintName("FK_MsParent_LtCountry")
                .OnDelete(DeleteBehavior.SetNull);    

            builder.HasOne(x => x.ParentRole)
                .WithMany( y => y.Parent)
                .HasForeignKey( fk => fk.IdParentRole)
                .HasConstraintName("FK_MsParent_LtParentRole")
                .OnDelete(DeleteBehavior.SetNull);        

            builder.HasOne(x => x.OccupationType)
                .WithMany( y => y.Parent)
                .HasForeignKey( fk => fk.IdOccupationType)
                .HasConstraintName("FK_MsParent_LtOccupationType")
                .OnDelete(DeleteBehavior.SetNull);       

            builder.HasOne(x => x.Religion)
                .WithMany( y => y.Parent)
                .HasForeignKey( fk => fk.IdReligion)
                .HasConstraintName("FK_MsParent_LtReligion")
                .OnDelete(DeleteBehavior.SetNull);    

            builder.HasOne(x => x.ParentSalaryGroup)
                .WithMany( y => y.Parent)
                .HasForeignKey( fk => fk.IdParentSalaryGroup)
                .HasConstraintName("FK_MsParent_LtParentSalaryGroup")
                .OnDelete(DeleteBehavior.NoAction);     

            builder.HasOne(x => x.LtAliveStatus)
                .WithMany( y => y.Parent)
                .HasForeignKey( fk => fk.AliveStatus)
                .HasConstraintName("FK_MsParent_LtAliveStatus")
                .OnDelete(DeleteBehavior.NoAction);    

            builder.HasOne(x => x.LtBinusianStatus)
                .WithMany( y => y.Parent)
                .HasForeignKey( fk => fk.BinusianStatus)
                .HasConstraintName("FK_MsParent_LtBinusianStatus")
                .OnDelete(DeleteBehavior.NoAction);      

            builder.HasOne(x => x.LastEducationLevel)
                .WithMany( y => y.Parent)
                .HasForeignKey( fk => fk.IdLastEducationLevel)
                .HasConstraintName("FK_MsParent_LtLastEducationLevel")
                .OnDelete(DeleteBehavior.NoAction);  

            builder.HasOne(x => x.ParentRelationship)
                .WithMany( y => y.Parent)
                .HasForeignKey( fk => fk.IdParentRelationship)
                .HasConstraintName("FK_MsParent_LtParentRelationship")
                .OnDelete(DeleteBehavior.Restrict);                         

            base.Configure(builder);
        }
    }
}
