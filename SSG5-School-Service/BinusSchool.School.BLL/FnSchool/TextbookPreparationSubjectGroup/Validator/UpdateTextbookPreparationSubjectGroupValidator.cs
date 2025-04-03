using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup;
using FluentValidation;

namespace BinusSchool.School.FnSchool.TextbookPreparationSubjectGroup.Validator
{
    public class UpdateTextbookPreparationSubjectGroupValidator : AbstractValidator<UpdateTextbookPreparationSubjectGroupRequest>
    {
        public UpdateTextbookPreparationSubjectGroupValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cant empty");
            RuleFor(x => x.SubjectGroupName).NotEmpty().WithMessage("subject group name cant empty");
            RuleFor(x => x.IdSubject).NotEmpty().WithMessage("subject cant empty");
        }
    }

}
