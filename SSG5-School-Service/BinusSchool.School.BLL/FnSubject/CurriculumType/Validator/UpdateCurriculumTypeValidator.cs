using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSubject.CurriculumType;
using FluentValidation;

namespace BinusSchool.School.FnSubject.CurriculumType.Validator
{
    public class UpdateCuricullumTypeValidator : AbstractValidator<UpdateCurriculumTypeRequest>
    {
        public UpdateCuricullumTypeValidator()
        {
            Include(new AddCurriculumTypeValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
