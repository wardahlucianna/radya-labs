using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Student.Validator
{
    public class UpdatePersonalInformationValidator : AbstractValidator<UpdateStudentPersonalInformationRequest>
    {
        public UpdatePersonalInformationValidator()
        {
            //Validate Name
            /*RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50).WithName("First Name");
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50).WithName("Last Name");
            //Validate Id
            RuleFor(x => x.NISN).NotEmpty().MaximumLength(50).WithName("Nomor Induk Siswa Nasional");
            RuleFor(x => x.IdBinusian).NotEmpty().MaximumLength(36).WithName("ID Binusian");
            //Validate Birth
            RuleFor(x => x.POB).NotEmpty().MaximumLength(50).WithName("Place Of Birth");
            RuleFor(x => x.DOB).NotEmpty().WithName("Date Of Birth");
            RuleFor(x => x.IdBirthCountry).NotEmpty().MaximumLength(10).WithName("ID Birth Country");
            RuleFor(x => x.IdBirthStateProvince).NotEmpty().MaximumLength(10).WithName("ID BirthState Province");
            RuleFor(x => x.IdBirthCity).NotEmpty().MaximumLength(10).WithName("ID Birth City");
            RuleFor(x => x.IdNationality).NotEmpty().MaximumLength(50).WithName("ID Nationality");
            RuleFor(x => x.IdCountry).NotEmpty().MaximumLength(50).WithName("ID Nationality Country");
            //Validate Religion
            RuleFor(x => x.IdReligion).NotEmpty().MaximumLength(36).WithName("Religion");
            RuleFor(x => x.IdReligionSubject).NotEmpty().MaximumLength(36).WithName("Religion Subject");
            //Validate Card
            RuleFor(x => x.FamilyCardNumber).NotEmpty().MaximumLength(16).WithName("Family Card Number");
            RuleFor(x => x.NIK).NotEmpty().MaximumLength(30).WithName("Nomor Induk Kependudukan");
            RuleFor(x => x.PassportNumber).NotEmpty().MaximumLength(30).WithName("Passport Number");
            RuleFor(x => x.PassportExpDate).NotEmpty().WithName("PassportExpDate");
            //Validate Child in Family
            RuleFor(x => x.ChildNumber).NotEmpty().WithName("Child Number");
            RuleFor(x => x.TotalChildInFamily).NotEmpty().WithName("Total Child In Family");
            RuleFor(x => x.IdChildStatus).NotEmpty().WithName("ID Child Status");
            RuleFor(x => x.IsHavingKJP).NotEmpty();
            RuleFor(x => x.Id).NotEmpty();*/
        }
        
    }
}
