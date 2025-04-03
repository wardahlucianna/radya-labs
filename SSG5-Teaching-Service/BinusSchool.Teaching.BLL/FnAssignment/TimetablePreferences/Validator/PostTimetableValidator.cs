using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences.Validator
{
    public class PostTimetableValidator : AbstractValidator<PostTimetableRequest>
    {
        public PostTimetableValidator()
        {
            //RuleForEach(x => x.TimeTable).NotNull();

            RuleFor(x => x.TimeTable)
                .NotEmpty()
                .ForEach(timetables => timetables.ChildRules(data =>
                {
                    data.RuleFor(x => x.TimeTableDetail)
                        .NotEmpty()
                        .ForEach(details => details.ChildRules(detail =>
                        {
                            detail.RuleFor(x => x.IdVenue).NotEmpty();

                            detail.RuleFor(x => x.IdSchoolDivision).NotEmpty();

                            detail.RuleFor(x => x.IdSchoolTerm).NotEmpty();

                            detail.RuleFor(x => x.IdUserTeaching).NotEmpty();

                            detail.RuleFor(x => x.Week).NotEmpty();

                            detail.RuleFor(x => x.Count).NotEmpty().GreaterThan(0);

                            detail.RuleFor(x => x.Length).NotEmpty().GreaterThan(0);

                        }));
                }));
        }
    }
}
