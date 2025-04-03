using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.SurveyTemplate;
using FluentValidation;

namespace BinusSchool.School.FnSchool.SurveyTemplate.Validator
{
    public class AddSurveyTemplateCopyValidator : AbstractValidator<AddSurveyTemplateCopyRequest>
    {
        public AddSurveyTemplateCopyValidator()
        {
            RuleFor(x => x.IdAcademicYearTo).NotEmpty().WithMessage("Id academic year to cant empty");
            RuleFor(x => x.ListIdSurveyTemplate).NotEmpty().WithMessage("Title cant empty");
        }
    }
}
