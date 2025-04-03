using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AvailabilitySetting;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.AvailabilitySetting.Validator
{
    public class AddAvailabilitySettingValidator : AbstractValidator<AddAvailabilitySettingRequest>
    {
        public AddAvailabilitySettingValidator()
        {
            RuleFor(x => x.Day).NotEmpty();
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdUserTeacher).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            //RuleFor(x => x.AvailabilitySettings.Count > 0).NotEmpty();
        }


    }
}
