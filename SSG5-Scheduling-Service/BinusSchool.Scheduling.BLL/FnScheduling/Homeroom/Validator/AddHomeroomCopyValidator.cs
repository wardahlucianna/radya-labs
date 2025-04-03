using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom.Validator
{
    public class AddHomeroomCopyValidator : AbstractValidator<AddHomeroomCopyRequest>
    {
        public AddHomeroomCopyValidator()
        {
            RuleFor(x => x.IdAcadyearCopyTo).NotEmpty().WithMessage("Academic year cannot empty");

            RuleFor(x => x.SemesterCopyTo)
                .NotEmpty()
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(2);

            RuleFor(x => x.IdHomeroom).NotEmpty().WithMessage("Id Lesson cannot empty");
        }
    }
}
