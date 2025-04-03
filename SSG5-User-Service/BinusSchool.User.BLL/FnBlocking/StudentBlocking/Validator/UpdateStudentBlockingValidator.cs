using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.StudentBlocking.Validator
{
    public class UpdateStudentBlockingValidator : AbstractValidator<UpdateStudentBlockingRequest>
    {
        public UpdateStudentBlockingValidator()
        {
            RuleFor(x => x.IdUser).NotNull();
            RuleFor(x => x.IdBlockingCategory).NotNull();
            RuleFor(x => x.IdBlockingType).NotNull();
            RuleFor(x => x.IsBlock).NotNull();
        }
    }
}
