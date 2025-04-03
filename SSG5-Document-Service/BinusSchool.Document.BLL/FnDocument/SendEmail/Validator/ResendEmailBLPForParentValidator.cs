using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.SendEmail;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.SendEmail.Validator
{
    public class ResendEmailBLPForParentValidator : AbstractValidator<List<ResendEmailBLPForParentRequest>>
    {
        public ResendEmailBLPForParentValidator()
        {
            RuleFor(model => model)
                .NotEmpty()
                .ForEach(data => data.ChildRules(data =>
                {
                    data.RuleFor(x => x.IdSchool).NotEmpty();
                    data.RuleFor(x => x.IdSurveyPeriod).NotEmpty();
                    data.RuleFor(x => x.IdStudent).NotEmpty();
                }));
        }
    }
}
