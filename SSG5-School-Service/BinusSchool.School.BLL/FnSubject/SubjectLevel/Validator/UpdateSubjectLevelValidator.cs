using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSubject.SubjectLevel;
using FluentValidation;

namespace BinusSchool.School.FnSubject.SubjectLevel.Validator
{
    public class UpdateSubjectLevelValidator : AbstractValidator<UpdateSubjectLevelRequest>
    {
        public UpdateSubjectLevelValidator()
        {
            Include(new AddSubjectLevelValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
