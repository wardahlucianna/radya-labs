using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using FluentValidation;

namespace BinusSchool.School.FnSubject.Subject.Validator
{
    public class CopySubjectValidator : AbstractValidator<CopySubjectRequest>
    {
        public CopySubjectValidator()
        {
            RuleFor(x => x.IdAcadyearFrom)
                .NotEmpty()
                .NotEqual(x => x.IdAcadyearTo);

            RuleFor(x => x.IdAcadyearTo)
                .NotEmpty()
                .NotEqual(x => x.IdAcadyearFrom);
        }
    }
}
