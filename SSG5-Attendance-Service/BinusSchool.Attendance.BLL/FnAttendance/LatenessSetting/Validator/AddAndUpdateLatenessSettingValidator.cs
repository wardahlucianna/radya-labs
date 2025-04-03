using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.LatenessSetting;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.LatenessSetting.Validator
{
    public class AddAndUpdateLatenessSettingValidator : AbstractValidator<AddAndUpdateLatenessSettingRequest>
    {
        public AddAndUpdateLatenessSettingValidator()
        {

            RuleFor(x => x.IdLevel).NotEmpty();

            RuleFor(x => x.TotalUnexcusedAbsend).NotEmpty();

            RuleFor(x => x.TotalLate).NotEmpty();
        }
    }
}
