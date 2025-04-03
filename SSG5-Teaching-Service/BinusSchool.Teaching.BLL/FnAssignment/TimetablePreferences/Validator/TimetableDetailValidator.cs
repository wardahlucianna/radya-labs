using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences.Validator
{
    public class TimetableDetailValidator : AbstractValidator<TimetableDetailRequest>
    {
        public TimetableDetailValidator()
        {
            RuleFor(x => x.IdTimetablePrefHeader).NotEmpty();
            RuleFor(x => x.DetailRequests).NotEmpty()
                .ForEach(x => x.ChildRules(data =>
                {
                    data.RuleFor(x => x.Action).NotEmpty();
                    data.RuleFor(x => x.NewValue.Count).GreaterThan(0);
                    data.RuleFor(x => x.NewValue.Length).GreaterThan(0);
                }));
        }
    }
}
