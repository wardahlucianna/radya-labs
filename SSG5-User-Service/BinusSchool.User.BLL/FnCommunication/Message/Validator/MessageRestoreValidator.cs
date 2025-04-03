using BinusSchool.Data.Model.User.FnCommunication.Message;
using FluentValidation;

namespace BinusSchool.User.FnCommunication.Message.Validator
{
    public class MessageRestoreValidator : AbstractValidator<MessageRestoreRequest>
    {
        public MessageRestoreValidator()
        {
            RuleFor(x => x.MessageId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}