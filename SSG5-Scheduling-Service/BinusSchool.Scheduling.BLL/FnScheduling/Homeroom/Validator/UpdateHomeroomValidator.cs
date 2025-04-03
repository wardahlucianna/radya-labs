using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom.Validator
{
    public class UpdateHomeroomValidator : AbstractValidator<UpdateHomeroomRequest>
    {
        public UpdateHomeroomValidator()
        {
            RuleFor(x => x.Id).NotEmpty();

            // RuleFor(x => x.IdVenue).NotEmpty();

            RuleFor(x => x.Teachers).SetValidator(_ => new HomeroomTeacherValidator());
        }
    }
}
