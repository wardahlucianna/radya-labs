using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSubject.SubjectType;
using FluentValidation;

namespace BinusSchool.School.FnSubject.SubjectType.Validator
{
    public class UpdateSubjectTypeValidator : AbstractValidator<UpdateSubjectTypeRequest>
    {
        public UpdateSubjectTypeValidator()
        {
            Include(new AddSubjectTypeValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
