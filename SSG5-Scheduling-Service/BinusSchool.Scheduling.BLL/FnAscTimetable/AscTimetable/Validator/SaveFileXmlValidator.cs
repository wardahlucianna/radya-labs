using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using FluentValidation;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Validator
{
    public class SaveFileXmlValidator : AbstractValidator<SaveFileAscTimetableRequest>
    {
        public SaveFileXmlValidator()
        {
            RuleFor(x => x.IdAscTimetable).NotEmpty();
        }
    }

}
