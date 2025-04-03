using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule.Validator
{
    public class DeleteUngenerateScheduleGradeV2Validator : AbstractValidator<DeleteUngenerateScheduleGradeV2Request>
    {
        public DeleteUngenerateScheduleGradeV2Validator()
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

                    period.RuleFor(x => x.UngenerateScheduleClass)
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
