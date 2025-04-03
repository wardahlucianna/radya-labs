using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup;
using FluentValidation;

namespace BinusSchool.School.FnSchool.TextbookPreparationSubjectGroup.Validator
{
    public class AddTextbookPreparationSubjectGroupValidator : AbstractValidator<AddTextbookPreparationSubjectGroupRequest>
    {
        public AddTextbookPreparationSubjectGroupValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Id academic year cant empty");
            RuleFor(x => x.SubjectGroupName).NotEmpty().WithMessage("subject group name cant empty");
            RuleFor(x => x.IdSubject).NotEmpty().WithMessage("subject cant empty");
        }
    }
}
