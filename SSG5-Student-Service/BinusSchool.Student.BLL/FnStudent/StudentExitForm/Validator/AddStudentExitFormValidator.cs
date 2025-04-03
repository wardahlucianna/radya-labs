using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.StudentExitForm.Validator
{
    public class AddStudentExitFormValidator : AbstractValidator<AddStudentExitFormRequest>
    {
        public AddStudentExitFormValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();

            RuleFor(x => x.IdHomeroomStudent).NotEmpty();

            //RuleFor(x => x.IdUserFather).NotEmpty();

            //RuleFor(x => x.FatherEmail).NotEmpty();

            //RuleFor(x => x.FatherPhone).NotEmpty();

            //RuleFor(x => x.IdUserMother).NotEmpty();

            //RuleFor(x => x.MotherEmail).NotEmpty();

            //RuleFor(x => x.MotherPhone).NotEmpty();

            RuleFor(x => x.StartExit).NotNull();

            //RuleFor(x => x.Explain).NotEmpty();

            RuleFor(x => x.NewSchoolName).NotEmpty();

            RuleFor(x => x.NewSchoolCity).NotEmpty();

            RuleFor(x => x.NewSchoolCountry).NotEmpty();

            RuleFor(x => x.Status).IsInEnum();

            RuleFor(x => x.IdHomeroom).NotEmpty();
        }
    }
}
