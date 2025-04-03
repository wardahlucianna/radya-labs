using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.SendEmail.Validator
{
    public class SendEmailProfileApprovalUpdateToParentValidator : AbstractValidator<SendEmailProfileApprovalUpdateToParentRequest>
    {
        public SendEmailProfileApprovalUpdateToParentValidator()
        {
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.IdStudentInfoUpdateList).NotEmpty();
        }
    }
}
