using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularUnattendance;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularUnattendance.Validator
{
    public class DeleteExtracurricularUnattendanceValidator : AbstractValidator<DeleteExtracurricularUnattendanceRequest>
    {
        public DeleteExtracurricularUnattendanceValidator()
        {
            RuleFor(x => x.IdExtracurricularNoAttDate).NotEmpty();
        }
    }
}
