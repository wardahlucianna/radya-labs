using BinusSchool.Data.Model.School.FnSchool.MySurvey;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MySurvey.Validator
{
    public class AddMySurveyValidator : AbstractValidator<AddMySurveyRequest>
    {
        public AddMySurveyValidator()
        {
            RuleFor(x => x.IdSurvey).NotEmpty();
            RuleFor(x => x.IdSurveyChild).NotEmpty();
            RuleFor(x => x.IdPublishSurvey).NotEmpty();
            RuleFor(x => x.IdSurveyTemplateChild).NotEmpty();
            RuleFor(x => x.Status).NotEmpty();
        }
    }
}
