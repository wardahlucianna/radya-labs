using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom.Validator
{
    public class AddHomeroomValidator : AbstractValidator<AddHomeroomRequest>
    {
        public AddHomeroomValidator()
        {
            RuleFor(x => x.IdAcadyear).NotEmpty();

            RuleFor(x => x.Semester)
                .NotEmpty()
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(2);

            RuleFor(x => x.IdGrade).NotEmpty();

            RuleFor(x => x.IdPathway).NotEmpty();

            // RuleFor(x => x.IdVenue).NotEmpty();

            RuleFor(x => x.Teachers).SetValidator(_ => new HomeroomTeacherValidator());
        }
    }
}
