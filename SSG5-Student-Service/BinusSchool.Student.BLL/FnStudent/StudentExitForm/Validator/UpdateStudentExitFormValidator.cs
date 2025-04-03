using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentExitForm.Validator
{
    public class UpdateStudentExitFormValidator : AbstractValidator<UpdateStudentExitFormRequest>
    {
        public UpdateStudentExitFormValidator()
        {
            Include(new AddStudentExitFormValidator());
            RuleFor(x => x.Id)
                .NotNull()
                .WithName("Id cannot null");
        }
    }
}
