using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class UpdateEvidencesValidator : AbstractValidator<UpdateEvidencesRequest>
    {
        public UpdateEvidencesValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithName("Id");

            RuleFor(x => x.IdExperience)
                .NotEmpty()
                .WithName("Id Experience");

            RuleFor(x => x.EvidencesType)
                .NotNull()
                .WithName("Evidences Type");
        }
    }
}
