using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.TransferStudentData
{
    public class TransferStudentRequest
    {
        public string IdStudent { get; set; }
        public string IdRegistrant { get; set; }
        public string IdBinusian { get; set; }
        public string FirstName { get; set; }

        //public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int IdStudentStatus { get; set; }
        public Gender Gender { get; set; }      
        public string IdReligion { get; set;} 
        public string IdSchool { get; set; }     
        public string SiblingIdStudent { get; set; }       
        public string NISN { get; set; }
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        public string IdBirthCountry { get; set; }
        public string IdBirthStateProvince { get; set; }
        public string IdBirthCity { get; set; }
        public string IdNationality { get; set; }
        public string IdCountry { get; set;}
        public string FamilyCardNumber { get; set;}
        public string NIK { get; set;}
        public string KITASNumber { get; set;}
        public DateTime? KITASExpDate { get; set;}
        public string NSIBNumber { get; set;}
        public DateTime? NSIBExpDate { get; set;}
        public string PassportNumber { get; set;}
        public DateTime? PassportExpDate { get; set;}               
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
        public decimal DistanceHomeToSchool { get; set; }
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string MobilePhoneNumber2 { get; set; }
        public string MobilePhoneNumber3 { get; set; }
        public string EmergencyContactRole { get; set; }

        //public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string FutureDream { get; set; }
        public string Hobby { get; set; }

        public StudentPrevSchool PrevSchool { get; set; }
        public StudentAdmissionData AdmissionData { get; set; }
        public IEnumerable<StudentChargingAdmission> ChargingAdmission { get; set; }
        public IEnumerable<Parent> Parents { get; set; }

    }

    public class StudentPrevSchool
    {
        public string IdPrevSchool { get; set; }  // prev master school id
        public string Grade { get; set; }  
        public string YearAttended { get; set; } 
        public string YearWithdrawn{ get; set; } 
    }
    public class StudentAdmissionData
    {
        //public string IdSchool { get; set; }
        // public string IdRegistrant { get; set; }
        // public string IdStudent { get; set; }
        public DateTime? DateofEntry { get; set; }
        public DateTime? DateofFormPurchased { get; set; }
        public DateTime? DateofApplReceived { get; set; }
        public DateTime? DateofReregistration { get; set; }
        public DateTime? JoinToSchoolDate { get; set; }
        public string AdmissionYear { get; set; }
        public string AcademicYear { get; set; }
        public string IdAcademicSemester { get; set; }
        public decimal? TotalScore { get; set; }
        public int? Grade { get; set; }
        public int? IdSchoolSubject { get; set; }
        public int? IdSchoolTPKSStatus { get; set; }
        public string TPKSNotes { get; set; }       
        public string IdSchoolLevel { get; set; }
        public int IdYearLevel { get; set; }
        public int IsEnrolledForFirstTime  { get; set; }
    }
    public class StudentChargingAdmission
    {
        // IdChargingAdmission guidid
        // public string IdSchool { get; set; }
        // public string IdStudent { get; set; }
        public string FormNumber { get; set; }
        public string AdmissionYear { get; set; }
        public string AcademicYear { get; set; }
        public string IdAcademicSemester { get; set; }        
        public string IdSchoolLevel { get; set; }
        public string IdYearLevel { get; set; }
        public string ComponentClass { get; set; }
        public string FeeGroupName { get; set; }
        public decimal ChargingAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public Int16 ChargingStatus { get; set; }
    }

    public class Parent
    {
        //public string IdParent { get; set; }
        public string FirstName { get; set; }      
        public string LastName { get; set; }
        public Gender Gender { get; set; }      
        public string IdParentRole { get; set; } 
        public string POB { get; set; }
        public DateTime? DOB { get; set; }      
        public Int16 AliveStatus { get; set; } 
        public string IdReligion { get; set;}
        public int? IdLastEducationLevel { get; set;} 
        public string IdNationality { get; set; }
        public string IdCountry { get; set;} 
        public string FamilyCardNumber { get; set;}
        public string NIK { get; set;}
        public string PassportNumber { get; set;}
        public DateTime? PassportExpDate { get; set;}
        public string KITASNumber { get; set;}
        public DateTime? KITASExpDate { get; set;}
        public Int16 BinusianStatus { get; set;}
        public string IdBinusian { get; set;}

        //public string ParentNameForCertificate { get; set;}
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
    }
}
