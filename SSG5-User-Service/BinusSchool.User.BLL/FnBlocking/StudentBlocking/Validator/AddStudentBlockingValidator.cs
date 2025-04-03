using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnBlocking.BlockingType;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.StudentBlocking.Validator
{
    public class AddStudentBlockingValidator : AbstractValidator<AddStudentBlockingRequest>
    {
        public AddStudentBlockingValidator()
        {

            RuleFor(x => x.StudentBlocking).NotEmpty();



        }

    }
}
