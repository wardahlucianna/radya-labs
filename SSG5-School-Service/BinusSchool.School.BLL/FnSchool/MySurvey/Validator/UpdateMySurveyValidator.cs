using BinusSchool.Data.Model.School.FnSchool.MySurvey;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MySurvey.Validator
{
    public class UpdateMySurveyValidator : AbstractValidator<UpdateMySurveyRequest>
    {
        public UpdateMySurveyValidator()
        {
            Include(new AddMySurveyValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
