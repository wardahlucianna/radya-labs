using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class AddEvidencesValidator : AbstractValidator<AddEvidencesRequest>
    {
        public AddEvidencesValidator()
        {
            RuleFor(x => x.IdExperience)
                .NotEmpty()
                .WithName("Id Experience");

            RuleFor(x => x.EvidencesType)
                .NotNull()
                .WithName("Evidences Type");
        }
    }
}
