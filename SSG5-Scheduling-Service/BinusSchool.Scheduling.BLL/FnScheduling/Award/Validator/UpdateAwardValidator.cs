using BinusSchool.Data.Model.Scheduling.FnSchedule.Award;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Award.Validator
{
    public class UpdateAwardValidator : AbstractValidator<UpdateAwardRequest>
    {
        public UpdateAwardValidator()
        {
            Include(new AddAwardValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
