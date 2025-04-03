using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BinusSchool.Persistence.StudentDb.Entities
{
    public class MsStudent : UserKindStudentParentEntity, IStudentEntity
    {
        public string IdRegistrant { get; set; }
        public string IdSchool { get; set; }
        public string IdBinusian { get; set; }
        public string NISN { get; set; }
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        public string IdBirthCountry { get; set; }
        public string IdBirthStateProvince { get; set; }
        public string IdBirthCity { get; set; }
        public string IdNationality { get; set; }      
        public string IdCountry { get; set; }
        public string FamilyCardNumber { get; set;}
        public string NIK { get; set;}
        public string KITASNumber { get; set;}
        public DateTime? KITASExpDate { get; set;}
        public string NSIBNumber { get; set;}
        public DateTime? NSIBExpDate { get; set;}
        public string PassportNumber { get; set;}
        public DateTime? PassportExpDate { get; set;}
        public string IdReligion { get; set;}
        public string IdReligionSubject { get; set;}
        public int ChildNumber { get; set;}
        public int TotalChildInFamily { get; set;}
        public string IdChildStatus { get; set;}
        public Int16 IsHavingKJP { get; set;}
        public Int16 IsSpecialTreatment { get; set;}
        public string NotesForSpecialTreatments { get; set;}
        public string IdBloodType { get; set;} 
        public int? Height { get; set;}
        public int? Weight { get; set;}
        public string ResidenceAddress { get; set; }
        public string HouseNumber { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public Int16 IdStayingWith { get; set; }
        public string VillageDistrict { get; set; }
        public string SubDistrict { get; set; }
        public string IdAddressCity { get; set; }
        public string IdAddressStateProvince { get; set; }
        public string IdAddressCountry { get; set; }
        public string PostalCode { get; set; }
        public decimal? DistanceHomeToSchool { get; set; }
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string MobilePhoneNumber2 { get; set; }
        public string MobilePhoneNumber3 { get; set; }
        public string EmergencyContactRole { get; set; }
        public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string FutureDream { get; set; }
        public string Hobby { get; set; }
        public string SchoolVANumber { get; set; }
        public string SchoolVAName { get; set; }
        public int IdStudentStatus { get; set; }

        public virtual ICollection<MsStudentParent> StudentParents { get; set; }
        public virtual ICollection<MsStudentGrade> StudentGrades { get; set; }
        public virtual MsSiblingGroup SiblingGroup { get; set; }
        public virtual ICollection<MsStudentLicenseEmail> StudentLicenseEmail { get; set; }
        public virtual LtReligion Religion { get; set; }
        public virtual TrStudentHistoryEmail StudentHistoryEmail { get; set; }
        public virtual LtReligionSubject ReligionSubject { get; set; }
        public virtual LtChildStatus ChildStatus { get; set; }
        public virtual LtNationality Nationality { get; set; }
        public virtual LtCountry Country { get; set; }      
        public virtual MsStudentFirstLoginConfirmation StudentFirstLoginConfirmation { get; set; }
        public virtual ICollection<MsBankAccountInformation> BankAccountInformation { get; set; }
        public virtual LtBloodType BloodType { get; set; }        
        public virtual MsStudentPrevSchoolInfo StudentPrevSchoolInfo { get; set; }
        public virtual MsAdmissionData AdmissionData { get; set; }
        public virtual ICollection<TrStudentChargingAdmission> StudentChargingAdmission { get; set; }
        public virtual TrEmailGenerate EmailGenerate { get; set; }
        public virtual LtStayingWith StayingWith { get; set; }
        public virtual TrStudentProfileConfirmation StudentProfileConfirmation { get; set; }
        public virtual ICollection<TrStudentSafeReport> TrStudentSafeReports { get; set; }
        public virtual ICollection<MsHomeroomStudent> MsHomeroomStudents { get; set; }
        public virtual LtStudentStatus StudentStatus { get; set; }
        public virtual MsSchool School { get; set; }
        public virtual ICollection<TrCounselingServicesEntry> CounselingServicesEntry { get; set; }
        public virtual ICollection<TrGcReportStudent> GcReportStudent { get; set; }
        public virtual ICollection<TrLearningGoalStudent> LearningGoalStudents { get; set; }
        public virtual ICollection<TrCourseworkAnecdotalStudent> CourseworkAnecdotalStudents { get; set; }
        public virtual ICollection<TrReflectionStudent> ReflectionStudent { get; set; }
        public virtual ICollection<TrStudentStatus> TrStudentStatuss { get; set; }
        public virtual ICollection<HTrStudentStatus> HTrStudentStatuss { get; set; }
        public virtual ICollection<TrExemplaryStudent> ExemplaryStudents { get; set; }
        public virtual ICollection<MsExemplaryStudentBlock> ExemplaryStudentBlocks { get; set; }
        public virtual ICollection<TrExperienceStudent> TrExperienceStudents { get; set; }
        public virtual ICollection<TrStudentPhoto> StudentPhotos { get; set; }
        public virtual ICollection<MsSurveyRespondent> SurveyRespondents { get; set; }
        public virtual ICollection<TrStudentLockerReservation> StudentLockerReservations { get; set; }
        public virtual ICollection<MsDigitalPickupQrCode> DigitalPickupQrCodes { get; set; }
        public virtual ICollection<TrDigitalPickup> DigitalPickups { get; set; }
        public virtual ICollection<TrServiceAsActionHeader> ServiceAsActionHeaders { get; set; }
        public virtual ICollection<TrStudentSubjectSelection> StudentSubjectSelections { get; set; }
        public virtual ICollection<TrStudentSubjectSelectionMajor> StudentSubjectSelectionMajors { get; set; }
        public virtual ICollection<TrStudentSubjectSelectionCountry> StudentSubjectSelectionCountries { get; set; }

    }

    internal class MsStudentConfiguration : UserKindStudentParentEntityConfiguration<MsStudent>
    {
        public override void Configure(EntityTypeBuilder<MsStudent> builder)
        {
            builder.Property(x => x.IdRegistrant)             
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(x => x.IdBinusian)               
                .HasMaxLength(36)
                .IsRequired();

            builder.Property(x => x.NISN)               
                .HasMaxLength(50);

            builder.Property(x => x.POB)               
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.DOB)              
                .HasColumnType(typeName: "datetime2");    

            builder.Property(x => x.IdBirthCountry)            
                .HasMaxLength(36);

            builder.Property(x => x.IdBirthStateProvince)   
                .HasMaxLength(36);

            builder.Property(x => x.IdBirthCity)   
                .HasMaxLength(36);

            builder.Property(x => x.IdNationality) 
                .HasMaxLength(36)
                .IsRequired(); 

            builder.Property(x => x.IdCountry) 
                .HasMaxLength(36);     
       
            builder.Property(x => x.FamilyCardNumber) 
                .HasColumnType("VARCHAR(16)")     
                .HasMaxLength(16);     

            builder.Property(x => x.NIK) 
                .HasColumnType("VARCHAR(30)")     
                .HasMaxLength(30);     

            builder.Property(x => x.KITASNumber) 
                .HasColumnType("VARCHAR(50)")     
                .HasMaxLength(50); 

            builder.Property(x => x.NSIBNumber) 
                .HasColumnType("VARCHAR(50)")     
                .HasMaxLength(50); 

            builder.Property(x => x.PassportNumber) 
                .HasColumnType("VARCHAR(30)")     
                .HasMaxLength(30); 

            builder.Property(x => x.IdReligion)                  
                .HasMaxLength(36);     

            builder.Property(x => x.IdReligionSubject)                  
                .HasMaxLength(36);     

            builder.Property(x => x.IdChildStatus)                  
                .HasMaxLength(36);    

            builder.Property(x => x.NotesForSpecialTreatments) 
                .HasColumnType("VARCHAR(300)")     
                .HasMaxLength(300); 

             builder.Property(x => x.IdBloodType)                  
                .HasMaxLength(36);       

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
                .HasMaxLength(36);  

            builder.Property(x => x.IdAddressStateProvince) 
                .HasMaxLength(36);     

            builder.Property(x => x.IdAddressCountry) 
                .HasMaxLength(36);    

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

            builder.Property(x => x.EmergencyContactRole) 
                .HasColumnType("CHAR(1)")     
                .HasMaxLength(1);    

            builder.Property(x => x.BinusianEmailAddress) 
                .HasColumnType("VARCHAR(200)")     
                .HasMaxLength(200);   

            builder.Property(x => x.PersonalEmailAddress) 
                .HasColumnType("VARCHAR(200)")     
                .HasMaxLength(200); 

            builder.Property(x => x.FutureDream) 
                .HasColumnType("VARCHAR(20)")     
                .HasMaxLength(20); 

            builder.Property(x => x.Hobby) 
                .HasColumnType("VARCHAR(20)")     
                .HasMaxLength(20);       

            builder.Property(x => x.SchoolVANumber)                  
                .HasMaxLength(50); 

            builder.Property(x => x.SchoolVAName)            
                .HasMaxLength(50);    

            builder.Property(x => x.IdSchool)            
                .HasMaxLength(36);       

            //builder.Property(x => x.IdStudentStatus)            
            //    .HasMaxLength(36);            

            builder.HasOne(x => x.Religion)
                .WithMany( y => y.Student)
                .HasForeignKey( fk => fk.IdReligion)
                .HasConstraintName("FK_MsStudent_LtReligion")
                .OnDelete(DeleteBehavior.SetNull);
                
             builder.HasOne(x => x.ReligionSubject)
                .WithMany( y => y.Student)
                .HasForeignKey( fk => fk.IdReligionSubject)
                .HasConstraintName("FK_MsStudent_LtReligionSubject")
                .OnDelete(DeleteBehavior.SetNull);

             builder.HasOne(x => x.ChildStatus)
                .WithMany( y => y.Student)
                .HasForeignKey( fk => fk.IdChildStatus)
                .HasConstraintName("FK_MsStudent_LtChildStatus")
                .OnDelete(DeleteBehavior.SetNull);

             builder.HasOne(x => x.Nationality)
                .WithMany( y => y.Student)
                .HasForeignKey( fk => fk.IdNationality)
                .HasConstraintName("FK_MsStudent_LtNasionality")
                .OnDelete(DeleteBehavior.NoAction);

            // builder.HasOne(x => x.NationalityCountry)
            //     .WithMany( y => y.Student)
            //     .HasForeignKey( fk => fk.IdNationalityCountry)
            //     .HasConstraintName("FK_MsStudent_MsNationalityCountry")
            //     .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Country)
                .WithMany( y => y.Student)
                .HasForeignKey( fk => fk.IdCountry)
                .HasConstraintName("FK_MsStudent_LtCountry")
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.BloodType)
                .WithMany( y => y.Student)
                .HasForeignKey( fk => fk.IdBloodType)
                .HasConstraintName("FK_MsStudent_LtBloodType")
                .OnDelete(DeleteBehavior.SetNull);    

            builder.HasOne(x => x.StayingWith)
                .WithMany( y => y.Student)
                .HasForeignKey( fk => fk.IdStayingWith)
                .HasConstraintName("FK_MsStudent_LStayingWith")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.StudentStatus)
               .WithMany(y => y.Students)
               .HasForeignKey(fk => fk.IdStudentStatus)
               .HasConstraintName("FK_MsStudent_LtStudentStatus")
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.School)
                .WithMany(y => y.Students)
                .HasForeignKey(fk => fk.IdSchool)
                .HasConstraintName("FK_MsStudent_MsSchool")
                .OnDelete(DeleteBehavior.NoAction);

            base.Configure(builder);
        }
    }
}
