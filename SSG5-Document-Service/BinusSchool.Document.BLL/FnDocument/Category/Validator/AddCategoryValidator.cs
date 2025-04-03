using BinusSchool.Data.Model.Document.FnDocument.Category;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.Category.Validator
{
    public class AddCategoryValidator : AbstractValidator<AddCategoryRequest>
    {
        public AddCategoryValidator()
        {
            RuleFor(x => x.IdDocumentCategory).NotEmpty();

            RuleFor(x => x.IdSchoolDocumentType).NotEmpty();
        }
    }
}