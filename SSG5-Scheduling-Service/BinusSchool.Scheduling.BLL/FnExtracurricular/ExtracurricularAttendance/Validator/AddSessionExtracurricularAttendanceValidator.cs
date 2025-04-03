using System.Collections.Generic;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance.Validator
{
    public class AddSessionExtracurricularAttendanceValidator : AbstractValidator<AddSessionExtracurricularAttendanceRequest>
    {
        public AddSessionExtracurricularAttendanceValidator()
        {
            RuleFor(x => x.IdExtracurricular).NotEmpty();
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x.Venue).NotEmpty();
            RuleFor(x => x.StartTime).NotEmpty();
            RuleFor(x => x.EndTime).NotEmpty();
        }
    }
}
