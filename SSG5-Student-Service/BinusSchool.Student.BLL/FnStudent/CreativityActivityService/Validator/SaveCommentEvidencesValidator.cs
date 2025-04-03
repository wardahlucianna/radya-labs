using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class SaveCommentEvidencesValidator : AbstractValidator<SaveCommentEvidencesRequest>
    {
        public SaveCommentEvidencesValidator()
        {
            RuleFor(x => x.IdUserComment)
                .NotEmpty()
                .WithName("Id User");

            RuleFor(x => x.IdEvidences)
                .NotNull()
                .WithName("Id Evidences");

            RuleFor(x => x.Comment)
                .NotEmpty()
                .WithName("Comment");

            RuleFor(x => x.IdAcademicYear)
               .NotEmpty()
               .WithName("Academic Year");
        }
    }
}
