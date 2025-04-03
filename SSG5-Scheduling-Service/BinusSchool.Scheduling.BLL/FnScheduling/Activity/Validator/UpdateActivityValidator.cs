using BinusSchool.Data.Model.Scheduling.FnSchedule.Activity;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Activity.Validator
{
    public class UpdateActivityValidator : AbstractValidator<UpdateActivityRequest>
    {
        public UpdateActivityValidator()
        {
            Include(new AddActivityValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
