using BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.EmergencyAttendance.Validator
{
    public class UnsubmitValidator : AbstractValidator<UnsubmitRequest>
    {
        public UnsubmitValidator()
        {
            RuleFor(x => x.IdEmergencyAttendance)
                .NotEmpty();
        }
    }
}
