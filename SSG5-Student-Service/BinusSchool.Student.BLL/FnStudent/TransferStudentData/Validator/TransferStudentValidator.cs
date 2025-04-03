using System;
using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.TransferStudentData.Validator
{
    public class TransferStudentValidator : AbstractValidator<TransferStudentRequest>
    {
        public TransferStudentValidator()
        {   
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.IdRegistrant).NotEmpty();
            RuleFor(x => x.IdBinusian).NotEmpty();
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.Gender).IsInEnum();                  
            RuleFor(x => x.IdStudentStatus).NotEmpty();
            RuleFor(x => x.IdReligion).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();    
            //SiblingIdStudent       
            //RuleFor(x => x.NISN).NotEmpty();
            RuleFor(x => x.POB).NotEmpty();
            RuleFor(x => x.DOB).NotEmpty().LessThan(p => DateTime.UtcNow.AddHours(7));
            RuleFor(x => x.IdBirthCountry).NotEmpty();
            //RuleFor(x => x.IdBirthStateProvince).NotEmpty();
            //RuleFor(x => x.IdBirthCity).NotEmpty(); 
            RuleFor(x => x.IdNationality).NotEmpty(); 
            RuleFor(x => x.IdCountry).NotEmpty(); 
            RuleFor(x => x.FamilyCardNumber).NotEmpty(); 
            //RuleFor(x => x.NIK).NotEmpty();
            //KITASNumber
            //KITASExpDate
            //NSIBNumber
            //NSIBExpDate
            //PassportNumber
            //PassportExpDate
            RuleFor(x => x.IdReligionSubject).NotEmpty();
            RuleFor(x => x.ChildNumber).NotEmpty();
            RuleFor(x => x.TotalChildInFamily).NotEmpty();
            RuleFor(x => x.IsHavingKJP).Must(x => x == 1 || x == 0);                                    
            RuleFor(x => x.IsSpecialTreatment).Must(x => x == 1 || x == 0);                                
            //NotesForSpecialTreatments
            //RuleFor(x => x.Height).NotEmpty().GreaterThan(0);
            //RuleFor(x => x.Weight).NotEmpty().GreaterThan(0);
            RuleFor(x => x.ResidenceAddress).NotEmpty();
            //RuleFor(x => x.HouseNumber).NotEmpty();
            RuleFor(x => x.IdBloodType).NotEmpty();
            //RuleFor(x => x.RT).NotEmpty();
            //RuleFor(x => x.RW).NotEmpty();
            //RuleFor(x => x.IdStayingWith).NotEmpty();
            RuleFor(x => x.VillageDistrict).NotEmpty();
            RuleFor(x => x.SubDistrict).NotEmpty();
            RuleFor(x => x.IdAddressCity).NotEmpty();
            RuleFor(x => x.IdAddressStateProvince).NotEmpty();
            RuleFor(x => x.IdAddressCountry).NotEmpty();
            //RuleFor(x => x.PostalCode).NotEmpty();
            // RuleFor(x => x.DistanceHomeToSchool).NotEmpty();
            // ResidencePhoneNumber
            RuleFor(x => x.MobilePhoneNumber1).NotEmpty(); 
            // MobilePhoneNumber2
            // MobilePhoneNumber3
            RuleFor(x => x.EmergencyContactRole).NotEmpty();            
            RuleFor(x => x.PersonalEmailAddress).NotEmpty();
            //FutureDream
            //Hobby
            

            //prevschool
            RuleFor(x => x.PrevSchool).NotEmpty();
            RuleFor(x => x.PrevSchool.IdPrevSchool).NotEmpty();    
            //RuleFor(x => x.PrevSchool.Grade).NotEmpty();    
            //RuleFor(x => x.PrevSchool.YearAttended).NotEmpty();    
            //RuleFor(x => x.PrevSchool.YearWithdrawn).NotEmpty();    

            //StudentAdmissionData
            RuleFor(x => x.AdmissionData).NotEmpty();
            RuleFor(x => x.AdmissionData.DateofEntry).NotEmpty();
            RuleFor(x => x.AdmissionData.DateofFormPurchased).NotEmpty();
            RuleFor(x => x.AdmissionData.DateofApplReceived).NotEmpty();
            RuleFor(x => x.AdmissionData.DateofReregistration).NotEmpty();         
            RuleFor(x => x.AdmissionData.JoinToSchoolDate).NotEmpty();
            RuleFor(x => x.AdmissionData.AdmissionYear).NotEmpty();
            RuleFor(x => x.AdmissionData.AcademicYear).NotEmpty();
            RuleFor(x => x.AdmissionData.IdAcademicSemester).NotEmpty();
            RuleFor(x => x.AdmissionData.TotalScore).NotEmpty();
            RuleFor(x => x.AdmissionData.Grade).NotEmpty();
            RuleFor(x => x.AdmissionData.IdSchoolSubject).NotEmpty();
            RuleFor(x => x.AdmissionData.IdSchoolTPKSStatus).NotEmpty();    
            //TPKSNotes       
            RuleFor(x => x.AdmissionData.IdSchoolLevel).NotEmpty();
            RuleFor(x => x.AdmissionData.IdYearLevel).NotEmpty();
            //RuleFor(x => x.AdmissionData.IsEnrolledForFirstTime).NotEmpty();
            

              /*RuleFor(x => x.ChargingAdmission)
                .NotEmpty()
                .ForEach(std => std.ChildRules(std =>
                {
                    std.RuleFor(x => x.FormNumber).NotEmpty();
                    std.RuleFor(x => x.AdmissionYear).NotEmpty();
                    std.RuleFor(x => x.AcademicYear).NotEmpty();
                    std.RuleFor(x => x.IdAcademicSemester).NotEmpty();               
                    std.RuleFor(x => x.IdSchoolLevel).NotEmpty();
                    std.RuleFor(x => x.IdYearLevel).NotEmpty();
                    std.RuleFor(x => x.ComponentClass).NotEmpty();
                    std.RuleFor(x => x.FeeGroupName).NotEmpty();
                    std.RuleFor(x => x.ChargingAmount).NotEmpty();
                    std.RuleFor(x => x.DueDate).NotEmpty();
                    std.RuleFor(x => x.ChargingStatus).NotEmpty();

                }));*/

            RuleFor(x => x.Parents)
                .NotEmpty()
                .ForEach(std => std.ChildRules(std =>
                {                   
                    std.RuleFor(x => x.FirstName).NotEmpty();
                    std.RuleFor(x => x.LastName).NotEmpty();
                    std.RuleFor(x => x.Gender).IsInEnum();   
                    std.RuleFor(x => x.IdParentRole).NotEmpty();
                    //std.RuleFor(x => x.POB).NotEmpty();
                    //std.RuleFor(x => x.DOB).NotEmpty().LessThan(p => DateTime.Now);           
                    //std.RuleFor(x => x.AliveStatus).NotEmpty();
                    //std.RuleFor(x => x.IdReligion).NotEmpty();
                    //std.RuleFor(x => x.IdLastEducationLevel).NotEmpty();
                    //std.RuleFor(x => x.IdNationality).NotEmpty();
                    //std.RuleFor(x => x.IdCountry).NotEmpty();
                    //std.RuleFor(x => x.FamilyCardNumber).NotEmpty();
                    //std.RuleFor(x => x.NIK).NotEmpty();
                    //PassportNumber
                    //PassportExpDate
                    //KITASNumber
                    //KITASExpDate
                    //std.RuleFor(x => x.BinusianStatus).NotEmpty();
                    //IdBinusian
                    //std.RuleFor(x => x.ResidenceAddress).NotEmpty();
                    //std.RuleFor(x => x.HouseNumber).NotEmpty();
                    //std.RuleFor(x => x.RT).NotEmpty();
                    //std.RuleFor(x => x.RW).NotEmpty();
                    //std.RuleFor(x => x.VillageDistrict).NotEmpty();
                    //std.RuleFor(x => x.SubDistrict).NotEmpty();
                    //std.RuleFor(x => x.IdAddressCity).NotEmpty();
                    //std.RuleFor(x => x.IdAddressStateProvince).NotEmpty();
                    //std.RuleFor(x => x.IdAddressCountry).NotEmpty();
                    //std.RuleFor(x => x.PostalCode).NotEmpty();
                    //ResidencePhoneNumber
                    //std.RuleFor(x => x.MobilePhoneNumber1).NotEmpty();
                    //MobilePhoneNumber2
                    //MobilePhoneNumber3
                    //std.RuleFor(x => x.PersonalEmailAddress).NotEmpty();
                    //WorkEmailAddress
                    //std.RuleFor(x => x.IdOccupationType).NotEmpty();
                    //std.RuleFor(x => x.OccupationPosition).NotEmpty(); 
                    //CompanyName
                    //std.RuleFor(x => x.IdParentSalaryGroup).NotEmpty();

                }));
        }
        
    }
}
