using BinusSchool.Data.Model.Scheduling.FnSchedule.Award;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Award.Validator
{
    public class AddAwardValidator : AbstractValidator<CreateAwardRequest>
    {
        public AddAwardValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithName("Award Name");
        }
    }
}
