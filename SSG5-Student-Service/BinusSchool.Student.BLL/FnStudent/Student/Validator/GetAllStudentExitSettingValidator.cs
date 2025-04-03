using BinusSchool.Data.Model.Student.FnStudent.StudentExitSetting;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Student.Validator
{
    public class GetAllStudentExitSettingValidator : AbstractValidator<GetAllStudentExitSettingRequest>
    {
        public GetAllStudentExitSettingValidator()
        {
            RuleFor(x => x.AcademicYear).NotEmpty();
        }
    }
}
