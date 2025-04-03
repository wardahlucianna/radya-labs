using BinusSchool.Data.Model.User.FnCommunication.Message;
using FluentValidation;

namespace BinusSchool.User.FnCommunication.Message.Validator
{
    public class SetMessageIsReadValidator : AbstractValidator<SetMessageIsReadRequest>
    {
        public SetMessageIsReadValidator()
        {
            RuleFor(x => x.MessageId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}