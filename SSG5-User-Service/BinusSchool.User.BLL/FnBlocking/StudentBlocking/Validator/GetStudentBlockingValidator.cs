using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.StudentBlocking.Validator
{
    public class GetStudentBlockingValidator : AbstractValidator<GetContentDataStudentBlockingRequest>
    {
        public GetStudentBlockingValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.IdUser).NotEmpty();

            RuleFor(x => x.IdAcademicYear).NotEmpty();

            RuleFor(x => x.Semester).NotEmpty();

        }
    }
}
