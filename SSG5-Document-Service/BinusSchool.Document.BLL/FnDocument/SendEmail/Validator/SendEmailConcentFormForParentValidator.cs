using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.SendEmail;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.SendEmail.Validator
{
    public class SendEmailConcentFormForParentValidator : AbstractValidator<SendEmailConcentFormForParentRequest>
    {
        public SendEmailConcentFormForParentValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();      
        }
    }
}
