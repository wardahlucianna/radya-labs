using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using FluentValidation;

namespace BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2.Validator
{
    public class UpdateEmergencyAttendanceReportValidator : AbstractValidator<UpdateEmergencyAttendanceReportRequest>
    {
        public UpdateEmergencyAttendanceReportValidator()
        {
            RuleFor(x => x.IdEmergencyReport).NotEmpty();
        }
    }

}
