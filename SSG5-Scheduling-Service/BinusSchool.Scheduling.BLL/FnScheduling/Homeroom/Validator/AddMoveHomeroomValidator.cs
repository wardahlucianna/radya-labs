using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom.Validator
{
    public class AddMoveHomeroomValidator : AbstractValidator<AddMoveHomeroomRequest>
    {
        public AddMoveHomeroomValidator()
        {
            RuleFor(x => x.IdHomeroomOld).NotEmpty();

            RuleFor(x => x.IdHomeroomNew).NotEmpty();

            RuleFor(x => x.IdStudents)
                .NotEmpty()
                .ForEach(ids => ids.ChildRules(id => id.RuleFor(x => x).NotEmpty()));
        }
    }
}