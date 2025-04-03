using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;
using FluentValidation;

namespace BinusSchool.School.FnSchool.PublishSurvey.Validator
{
    public class UpdatePublishSurveyValidator : AbstractValidator<UpdatePublishSurveyRequest>
    {
        public UpdatePublishSurveyValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id cant empty");
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Id academic year cant empty");
            RuleFor(x => x.SurveyName).NotEmpty().WithMessage("Survey Name cant empty");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Description cant empty");
            RuleFor(x => x.IdSurveyTemplate).NotEmpty().WithMessage("Id Survey Template cant empty");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("StartDate cant empty");
            RuleFor(x => x.EndDate).NotEmpty().WithMessage("EndDate cant empty");
        }
    }
}
