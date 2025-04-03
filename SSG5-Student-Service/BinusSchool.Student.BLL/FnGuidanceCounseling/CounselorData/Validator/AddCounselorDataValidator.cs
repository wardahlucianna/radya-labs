using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselorData.Validator
{
    public class AddCounselorDataValidator : AbstractValidator<AddCounselorDataRequest>
    {
        public AddCounselorDataValidator()
        {
            RuleFor(x => x.IdAcademicYear)
                .NotEmpty()
                .WithName("Academic Years");

            RuleFor(x => x.IdRole)
                .NotEmpty()
                .WithName("Role");

            RuleFor(x => x.IdPosition)
                .NotEmpty()
                .WithName("Position");

            RuleFor(x => x.IdUser)
                .NotEmpty()
                .WithName("Counselor Name");

            RuleFor(x => x.ListGradeCounselorData)
                .NotNull()
                .NotEmpty()
                .WithName("Grade Counseling Data");
        }
    }
}
