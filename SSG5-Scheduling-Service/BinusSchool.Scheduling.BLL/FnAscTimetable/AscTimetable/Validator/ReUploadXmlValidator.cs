using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using FluentValidation;

namespace BinusSchool.Scheduling.FnAscTimetable.AscTimetable.Validator
{
    public class ReUploadXmlValidator : AbstractValidator<AscTimeTableReUploadXmlRequest>
    {
        public ReUploadXmlValidator()
        {
            RuleFor(x => x.IdAscTimeTable).NotEmpty();
            RuleFor(x => x.IdGradePathway).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
