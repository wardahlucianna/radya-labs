using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance.Validator
{
    public class GetActiveUnsubmittedAttendanceValidator : AbstractValidator<GetActiveUnsubmittedAttendanceRequest>
    {
        public GetActiveUnsubmittedAttendanceValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            //RuleFor(x => x.IdAcademicYear).NotEmpty();
            //RuleFor(x => x.Semeter).NotEmpty();
            RuleFor(x => x.IdUser).NotEmpty();
        }
    }
}
