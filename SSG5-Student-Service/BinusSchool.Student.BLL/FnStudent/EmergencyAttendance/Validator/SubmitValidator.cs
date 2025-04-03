using BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.EmergencyAttendance.Validator
{
    public class SubmitValidator : AbstractValidator<SubmitRequest>
    {
        public SubmitValidator()
        {
            RuleFor(x => x.IdStudents)
                .NotNull();
        }
    }
}
