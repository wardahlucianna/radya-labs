using BinusSchool.Data.Model.Document.FnDocument.Sourcedata;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.Sourcedata
{
    public class AddSourcedataValidator : AbstractValidator<AddSourcedataRequest>
    {
        public AddSourcedataValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            
            RuleFor(x => x.IdSourcedata).NotEmpty();
        }
    }
}