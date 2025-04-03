using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Session.Validator
{
    public class UpdateSessionValidator : AbstractValidator<UpdateSessionRequest>
    {
        public UpdateSessionValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Alias).NotEmpty();
            RuleFor(x => x.DurationInMinutes).NotEmpty();
            RuleFor(x => x.StartTime).NotEmpty();
            RuleFor(x => x.EndTime).NotEmpty();
        }
    }
}
