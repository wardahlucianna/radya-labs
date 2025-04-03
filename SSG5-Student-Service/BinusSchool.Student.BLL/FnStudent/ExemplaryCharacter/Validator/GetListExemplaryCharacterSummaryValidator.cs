using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter.Validator
{
    public class GetListExemplaryCharacterSummaryValidator : AbstractValidator<GetListExemplaryCharacterSummaryRequest>
    {
        public GetListExemplaryCharacterSummaryValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IsPostedByMe).NotNull();
        }
    }
}
