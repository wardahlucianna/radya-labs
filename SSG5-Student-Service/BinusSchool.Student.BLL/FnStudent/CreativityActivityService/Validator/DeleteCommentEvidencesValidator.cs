using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class DeleteCommentEvidencesValidator : AbstractValidator<DeleteCommentEvidencesRequest>
    {
        public DeleteCommentEvidencesValidator()
        {
            RuleFor(x => x.IdCommentEvidences)
                .NotEmpty()
                .WithName("Id Comment Evidences");
        }
    }
}
