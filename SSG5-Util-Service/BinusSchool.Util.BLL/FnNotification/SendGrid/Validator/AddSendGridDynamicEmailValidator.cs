using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using FluentValidation;

namespace BinusSchool.Util.FnNotification.SendGrid.Validator
{
    public class AddSendGridDynamicEmailValidator : AbstractValidator<AddSendGridDynamicEmailRequest>
    {
        public AddSendGridDynamicEmailValidator()
        {
            RuleFor(x => x.IdTemplate).NotEmpty();

            RuleFor(x => x.To)
                .NotEmpty()
                .ForEach(tos => tos.ChildRules(to => to.RuleFor(x => !string.IsNullOrEmpty(x))));

            RuleFor(x => x.TemplateData).NotEmpty();
        }
    }
}
