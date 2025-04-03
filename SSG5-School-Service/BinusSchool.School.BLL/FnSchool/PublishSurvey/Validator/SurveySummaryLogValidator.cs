using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using FluentValidation;

namespace BinusSchool.School.FnSchool.PublishSurvey.Validator
{
    public class SurveySummaryLogValidator : AbstractValidator<AddAndUpdateSurveySummaryLogRequest>
    {
        public SurveySummaryLogValidator()
        {
            RuleFor(x => x.IdUser).NotEmpty().WithMessage("Id User year cant empty");
        }
    }
}
