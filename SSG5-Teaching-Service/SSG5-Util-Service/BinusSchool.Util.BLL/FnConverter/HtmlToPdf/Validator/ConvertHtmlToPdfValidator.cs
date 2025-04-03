using BinusSchool.Data.Model.Util.FnConverter.HtmlToPdf;
using FluentValidation;

namespace BinusSchool.Util.FnConverter.HtmlToPdf.Validator
{
    public class ConvertHtmlToPdfValidator : AbstractValidator<ConvertHtmlToPdfRequest>
    {
        public ConvertHtmlToPdfValidator()
        {
            RuleFor(x => x.FileName).NotEmpty();
            
            RuleFor(x => x.HtmlString).NotEmpty();
        }
    }
}