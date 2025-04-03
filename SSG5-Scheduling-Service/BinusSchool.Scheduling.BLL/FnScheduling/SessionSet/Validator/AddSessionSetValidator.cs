using BinusSchool.Data.Model.Scheduling.FnSchedule.SessionSet;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.SessionSet.Validator
{
    public class AddSessionSetValidator : AbstractValidator<AddSessionSetRequest>
    {
        public AddSessionSetValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}
