using BinusSchool.Data.Model.User.FnCommunication.Message;
using FluentValidation;
using System.Linq;

namespace BinusSchool.User.FnCommunication.Message.Validator
{
    public class AddMessageValidator : AbstractValidator<AddMessageRequest>
    {
        public AddMessageValidator()
        {

            RuleFor(x => x.IdSender).NotEmpty();

            RuleFor(x => x.Type).NotEmpty();

            RuleFor(x => x.IsSetSenderAsSchool).NotNull();

            RuleFor(x => x.IsAllowReply).NotNull();

            RuleFor(x => x.IsMarkAsPinned).NotNull();

            RuleFor(x => x.IsDraft).NotNull();
        }
    }
}
