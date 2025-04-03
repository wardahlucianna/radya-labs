using BinusSchool.Data.Model.User.FnCommunication.Message;
using FluentValidation;

namespace BinusSchool.User.FnCommunication.Message.Validator
{
    public class SetMessageApprovalStatusValidator : AbstractValidator<SetMessageApprovalStatusRequest>
    {
        public SetMessageApprovalStatusValidator()
        {
            RuleFor(x => x.MessageId).NotEmpty();
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.IsApproved).NotNull();
            RuleFor(x => x.IdSchool).NotNull();
        }
    }
}
