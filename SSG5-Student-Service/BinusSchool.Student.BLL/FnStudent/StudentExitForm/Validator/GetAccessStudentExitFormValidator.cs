using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentExitForm.Validator
{
    public class GetAccessStudentExitFormValidator : AbstractValidator<GetAccessStudentExitFormRequest>
    {
        public GetAccessStudentExitFormValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.IdParent).NotEmpty();
        }
    }
}
