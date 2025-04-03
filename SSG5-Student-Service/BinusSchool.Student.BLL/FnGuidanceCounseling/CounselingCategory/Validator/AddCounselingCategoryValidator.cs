using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingCategory;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingCategory.Validator
{
    public class AddCounselingCategoryValidator : AbstractValidator<AddCounselingCategoryRequest>
    {
        public AddCounselingCategoryValidator()
        {
            RuleFor(x => x.CounselingCategoryName)
                .NotEmpty()
                .WithName("Counseling Category Name");

            RuleFor(x => x.IdSchool)
                .NotEmpty()
                .WithName("Id School");
        }
    }
}
