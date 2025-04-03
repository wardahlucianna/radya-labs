using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup;
using FluentValidation;

namespace BinusSchool.School.FnSchool.TextbookPreparationSubjectGroup.Validator
{
   
    public class GetTextbookPreparationSubjectValidator : AbstractValidator<GetTextbookPreparationSubjectRequest>
    {
        public GetTextbookPreparationSubjectValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Id academic year cant empty");
            RuleFor(x => x.IdLevel).NotEmpty().WithMessage("Id level year cant empty");
            RuleFor(x => x.IdGrade).NotEmpty().WithMessage("Id grade cant empty");
        }
    }
}
