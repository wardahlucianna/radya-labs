using BinusSchool.Data.Model.Document.FnDocument.Sourcedata;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.Sourcedata
{
    public class UpdateSourcedataValidator : AbstractValidator<UpdateSourcedataRequest>
    {
        public UpdateSourcedataValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            
            RuleFor(x => x.IdSchool).NotEmpty();
            
            RuleFor(x => x.IdSourcedata).NotEmpty();
        }
    }
}