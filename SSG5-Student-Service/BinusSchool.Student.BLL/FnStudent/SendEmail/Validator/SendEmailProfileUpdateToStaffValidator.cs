using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.SendEmail.Validator
{
    public class SendEmailProfileUpdateToStaffValidator : AbstractValidator<SendEmailProfileUpdateToStaffRequest>
    {
        public SendEmailProfileUpdateToStaffValidator()
        {
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.IdStudentInfoUpdateList).NotEmpty();
        }
    }
}
