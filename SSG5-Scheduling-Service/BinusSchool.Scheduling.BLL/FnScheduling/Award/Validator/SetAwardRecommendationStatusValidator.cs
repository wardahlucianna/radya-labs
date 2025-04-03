using BinusSchool.Data.Model.Scheduling.FnSchedule.Award;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Award.Validator
{
    public class SetAwardRecommendationStatusValidator : AbstractValidator<SetAwardRecommendationStatusRequest>
    {
        public SetAwardRecommendationStatusValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IsSetRecommendation).NotNull();
        }
    }
}