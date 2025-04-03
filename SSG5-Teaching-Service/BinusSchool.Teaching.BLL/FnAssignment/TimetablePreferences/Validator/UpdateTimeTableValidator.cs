using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences.Validator
{
    public class UpdateTimeTableValidator : AbstractValidator<UpdateTimetableRequest>
    {
        public UpdateTimeTableValidator()
        {
            RuleFor(x => x.Id).NotNull();
        }
    }
}
