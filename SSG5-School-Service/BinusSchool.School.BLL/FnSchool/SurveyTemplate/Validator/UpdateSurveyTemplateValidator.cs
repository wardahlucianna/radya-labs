using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.SurveyTemplate;
using FluentValidation;

namespace BinusSchool.School.FnSchool.SurveyTemplate.Validator
{
    public class UpdateSurveyTemplateValidator : AbstractValidator<UpdateSurveyTemplateRequest>
    {
        public UpdateSurveyTemplateValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cant empty");
            RuleFor(x => x.IdTemplateChild).NotEmpty().WithMessage("Id template child cant empty");
        }
    }
}
