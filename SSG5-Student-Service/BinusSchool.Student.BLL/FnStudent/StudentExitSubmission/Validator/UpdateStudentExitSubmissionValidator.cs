using BinusSchool.Data.Model.Student.FnStudent.StudentExitSubmission;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentExitSubmission.Validator
{
    public class UpdateStudentExitSubmissionValidator : AbstractValidator<UpdateStudentExitSubmissionRequest>
    {
        public UpdateStudentExitSubmissionValidator()
        {
            RuleFor(x => x.Id)
                .NotNull()
                .WithName("Id cannot null");

            RuleFor(x => x.Status)
                .NotNull()
                .WithName("Status cannot null");
        }
    }
}
