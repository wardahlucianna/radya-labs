using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Finance.FnPayment.SendEmail;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.SendEmail;
using FluentValidation;

namespace BinusSchool.Finance.FnPayment.SendEmail.Validator
{
    public class SendEmailDeleteNotPaidExtracurricularToParentValidator : AbstractValidator<List<SendEmailDeleteNotPaidExtracurricularToParentRequest>>
    {
        public SendEmailDeleteNotPaidExtracurricularToParentValidator()
        {
            RuleFor(model => model)
                .NotEmpty()
                .ForEach(data => data.ChildRules(data =>
                {
                    data.RuleFor(x => x.School.Id).NotEmpty();
                    data.RuleFor(x => x.School.Name).NotEmpty();
                    data.RuleFor(x => x.Extracurricular.Id).NotEmpty();
                    data.RuleFor(x => x.Extracurricular.Name).NotEmpty();
                    data.RuleFor(x => x.Student.Id).NotEmpty();
                    data.RuleFor(x => x.Student.Name).NotEmpty();
                    data.RuleFor(x => x.DueHourPayment).GreaterThanOrEqualTo(0);
                }));
        }
    }
}
