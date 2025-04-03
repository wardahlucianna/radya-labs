using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class EvidencesDeleteValidator : AbstractValidator<DeleteEvidencesRequest>
    {
        public EvidencesDeleteValidator()
        {
            RuleFor(x => x.IdEvidences)
                .NotEmpty()
                .WithName("Id Evidences");
        }
    }
}
