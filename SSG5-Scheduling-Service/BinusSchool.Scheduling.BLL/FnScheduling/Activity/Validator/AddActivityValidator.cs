using BinusSchool.Data.Model.Scheduling.FnSchedule.Activity;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Activity.Validator
{
    public class AddActivityValidator : AbstractValidator<AddActivityRequest>
    {
        public AddActivityValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithName("Activity Name");
        }
    }
}
