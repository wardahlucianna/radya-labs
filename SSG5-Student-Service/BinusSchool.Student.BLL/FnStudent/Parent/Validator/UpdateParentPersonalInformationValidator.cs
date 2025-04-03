using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Parent.Validator
{
    public class UpdateParentPersonalInformationValidator : AbstractValidator<UpdateParentPersonalInformationRequest>
    {
        public UpdateParentPersonalInformationValidator()
        {
            /*RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.POB).NotEmpty();
            RuleFor(x => x.DOB).NotEmpty();
            RuleFor(x => x.IdParentRole).NotEmpty();
            RuleFor(x => x.AliveStatus).NotEmpty();
            RuleFor(x => x.IdReligion).NotEmpty();
            RuleFor(x => x.IdLastEducationLevel).NotEmpty();
            RuleFor(x => x.IdNationality).NotEmpty();
            RuleFor(x => x.IdCountry).NotEmpty();
            RuleFor(x => x.FamilyCardNumber).NotEmpty();
            RuleFor(x => x.NIK).NotEmpty();
            RuleFor(x => x.PassportNumber).NotEmpty();
            RuleFor(x => x.PassportExpDate).NotEmpty();
            RuleFor(x => x.KITASNumber).NotEmpty();
            RuleFor(x => x.KITASExpDate).NotEmpty();
            RuleFor(x => x.BinusianStatus).NotEmpty();
            RuleFor(x => x.IdBinusian).NotEmpty();
            RuleFor(x => x.ParentNameForCertificate).NotEmpty();
            */
        }
    }
}
