using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceBlockingSetting;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.AttendanceBlockingSetting.Validator
{
    public class UpdateAttendanceBlockingSettingValidator : AbstractValidator<UpdateAttendanceBlockingSettingRequest>
    {
        public UpdateAttendanceBlockingSettingValidator()
        {
            RuleFor(x => x.IdBlockingType).NotNull();
            RuleFor(x => x.IdAcademicYear).NotNull();
        }
    }
}
