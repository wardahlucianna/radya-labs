using BinusSchool.Data.Model.Document.FnDocument.Category;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.Category.Validator
{
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCategoryValidator()
        {
            Include(new AddCategoryValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}