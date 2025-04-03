using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingCategory;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingCategory.Validator
{
    public class UpdateCounselingCategoryValidator : AbstractValidator<UpdateCounselingCategoryRequest>
    {
        public UpdateCounselingCategoryValidator()
        {
            Include(new AddCounselingCategoryValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
