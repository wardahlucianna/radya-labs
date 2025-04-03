using System.Collections.Generic;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance.Validator
{
    public class UpdateExtracurricularAttendanceValidator : AbstractValidator<UpdateExtracurricularAttendanceRequest>
    {
        public UpdateExtracurricularAttendanceValidator()
        {
            RuleFor(x => x.IdGeneratedAttendance).NotEmpty();
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x.StatusAttendance)
               .NotEmpty()
               .ForEach(data => data.ChildRules(data =>
               {
                   data.RuleFor(x => x.IdStudent).NotEmpty();
                   data.RuleFor(x => x.IdStatusAttendance).NotEmpty();
               }));
        }
    }
}
