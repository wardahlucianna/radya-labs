using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance.Validator
{
    public class DeleteSessionExtracurricularAttendanceValidator : AbstractValidator<DeleteSessionExtracurricularAttendanceRequest>
    {
        public DeleteSessionExtracurricularAttendanceValidator()
        {
            RuleFor(x => x.IdExtracurricularGeneratedAtt).NotEmpty();
        }
    }
}
