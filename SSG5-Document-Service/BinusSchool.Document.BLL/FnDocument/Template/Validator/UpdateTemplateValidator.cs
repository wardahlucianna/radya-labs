using BinusSchool.Data.Model.Document.FnDocument.Template;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.Template
{
    public class UpdateTemplateValidator : AbstractValidator<UpdateTemplateRequest>
    {
        public UpdateTemplateValidator()
        {
            Include(new AddTemplateValidator());
            
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}