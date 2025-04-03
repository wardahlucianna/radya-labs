using BinusSchool.Data.Model.User.FnCommunication.Message;
using FluentValidation;
using System.Linq;

namespace BinusSchool.User.FnCommunication.Message.Validator
{
    public class DeleteMessageValidator : AbstractValidator<DeleteMessageRequest>
    {
        public DeleteMessageValidator()
        {
            RuleFor(x => x.IdUser).NotEmpty();

            RuleFor(x => x.IsDeletePermanent)
                .NotNull()
                .WithMessage("Type Delete is required");

            RuleFor(x => x.IdMessage)
                .NotNull()
                .Must(x => x.Count > 0)
                .WithMessage("Message is required");

            RuleFor(x => x.MessageFolder)
                .NotNull()
                .WithMessage("Message Folder is required");
        }
    }
}
