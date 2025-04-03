using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Parent.Validator
{
    public class UpdateParentContactInformationValidator : AbstractValidator<UpdateParentContactInformationRequest>
    {
        public UpdateParentContactInformationValidator()
        {
            /*RuleFor(x => x.ResidencePhoneNumber).NotEmpty();
            RuleFor(x => x.MobilePhoneNumber1).NotEmpty();
            RuleFor(x => x.PersonalEmailAddress).NotEmpty();
            */
        }
    }
}
