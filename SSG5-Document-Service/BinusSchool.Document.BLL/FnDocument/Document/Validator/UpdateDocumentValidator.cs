using BinusSchool.Data.Model.Document.FnDocument.Document;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.Document.Validator
{
    public class UpdateDocumentValidator : AbstractValidator<UpdateDocumentRequest>
    {
        public UpdateDocumentValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.JsonDocumentValue)
               .NotEmpty();

        }
    }
}
