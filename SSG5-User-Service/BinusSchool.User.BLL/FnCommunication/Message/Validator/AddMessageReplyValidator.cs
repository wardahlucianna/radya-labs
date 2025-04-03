using BinusSchool.Data.Model.User.FnCommunication.Message;
using FluentValidation;

namespace BinusSchool.User.FnCommunication.Message.Validator
{
    public class AddMessageReplyValidator : AbstractValidator<AddMessageReplyRequest>
    {
        public AddMessageReplyValidator()
        {
            RuleFor(x => x.IdSender).NotEmpty();
            RuleFor(x => x.Content).NotEmpty();
            RuleFor(x => x.MessageId).NotEmpty();
            RuleFor(x => x.ParentMessageId).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
