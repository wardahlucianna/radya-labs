using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom.Validator
{
    public class GetHomeroomByLevelGradeValidator : AbstractValidator<GetHomeroomByLevelGradeRequest>
    {
        public GetHomeroomByLevelGradeValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
        }
    }
}
