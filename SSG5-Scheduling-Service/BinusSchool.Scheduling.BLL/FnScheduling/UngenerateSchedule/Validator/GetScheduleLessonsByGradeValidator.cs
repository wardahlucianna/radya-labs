using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule.Validator
{
    public class GetScheduleLessonsByGradeValidator : AbstractValidator<GetScheduleLessonsByGradeRequest>
    {
        public GetScheduleLessonsByGradeValidator()
        {
            RuleFor(x => x.IdGrade).NotEmpty();
            RuleFor(x => x.IdAscTimetable).NotEmpty();
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
            RuleFor(x => x.IdDays.Any()).NotEmpty();
        }
    }
}
