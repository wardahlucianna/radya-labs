using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule.Validator
{
    public class DeleteUngenerateScheduleGradeValidator : AbstractValidator<DeleteUngenerateScheduleGradeRequest>
    {
        public DeleteUngenerateScheduleGradeValidator()
        {
            RuleFor(x => x.IdAscTimetable).NotEmpty();

            RuleFor(x => x.Periods)
                .NotEmpty()
                .ForEach(periods => periods.ChildRules(period =>
                {
                    period.RuleFor(x => x.Start)
                        .NotEmpty()
                        .LessThanOrEqualTo(x => x.End);

                    period.RuleFor(x => x.End)
                        .NotEmpty();
                        // .GreaterThanOrEqualTo(x => x.Start);

                    period.RuleFor(x => x.IdGrade).NotEmpty();

                    period.RuleFor(x => x.ClassIds)
                        .NotEmpty()
                        .WithMessage("Can't be ungenerate because class id is empty, please map it")
                        .ForEach(classIds => classIds.ChildRules(classId =>
                            classId.RuleFor(x => x)
                                   .NotEmpty()
                                   .WithMessage("Can't be ungenerate because class id is empty, please map it")
                        ));
                }));
        }
    }
}
