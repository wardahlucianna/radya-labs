using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator
{
    public class CreateDataActivityForMeritValidator : AbstractValidator<CreateActivityDataToMeritRequest>
    {
        public CreateDataActivityForMeritValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty().WithMessage("Academic year cannot empty");
            RuleFor(x => x.MeritStudents).NotEmpty().WithMessage("Merit Students cannot empty");
        }
    }
}
