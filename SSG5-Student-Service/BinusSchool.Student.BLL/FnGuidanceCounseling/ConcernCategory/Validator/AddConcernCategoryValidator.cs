using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ConcernCategory;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.ConcernCategory.Validator
{
    public class AddConcernCategoryValidator : AbstractValidator<AddConcernCategoryRequest>
    {
        public AddConcernCategoryValidator()
        {
            RuleFor(x => x.ConcernCategoryName)
                .NotEmpty()
                .WithName("Concern Category Name");

            RuleFor(x => x.IdSchool)
                .NotEmpty()
                .WithName("Id School");
        }
    }
}
