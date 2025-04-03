using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ConcernCategory;
using FluentValidation;

namespace BinusSchool.Student.FnGuidanceCounseling.ConcernCategory.Validator
{
    public class UpdateConcernCategoryValidator : AbstractValidator<UpdateConcernCategoryRequest>
    {
        public UpdateConcernCategoryValidator()
        {
            Include(new AddConcernCategoryValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
