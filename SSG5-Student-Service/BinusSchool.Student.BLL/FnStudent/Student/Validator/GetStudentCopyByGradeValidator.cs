using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Student.Validator
{
    public class GetStudentCopyByGradeValidator : AbstractValidator<GetStudentCopyByGradeRequest>
    {
        public GetStudentCopyByGradeValidator()
        {
            RuleFor(x => x.IdAcademicYearTarget).NotNull();
            RuleFor(x => x.IdAcademicYearSource).NotNull();
            RuleFor(x => x.IdLevel).NotNull();
            RuleFor(x => x.IdGrade).NotNull();
        }
    }
}
