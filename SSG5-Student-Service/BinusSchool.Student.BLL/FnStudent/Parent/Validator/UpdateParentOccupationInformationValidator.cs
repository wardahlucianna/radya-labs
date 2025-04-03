using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Parent.Validator
{
    public class UpdateParentOccupationInformationValidator : AbstractValidator<UpdateParentOccupationInformationRequest>
    {
        public UpdateParentOccupationInformationValidator()
        {
            /*RuleFor(x => x.IdOccupationType).NotEmpty();
            RuleFor(x => x.OccupationPosition).NotEmpty();
            RuleFor(x => x.CompanyName).NotEmpty();
            RuleFor(x => x.IdParentSalaryGroup).NotEmpty();
            */
        }
    }
}
