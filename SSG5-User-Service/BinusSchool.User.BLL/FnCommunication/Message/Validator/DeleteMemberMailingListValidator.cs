using BinusSchool.Data.Model.User.FnCommunication.Message;
using FluentValidation;
using System.Linq;

namespace BinusSchool.User.FnCommunication.Message.Validator
{
    public class DeleteMemberMailingListValidator : AbstractValidator<DeleteMemberMailingListRequest>
    {
        public DeleteMemberMailingListValidator()
        {
            RuleFor(x => x.IdUser).NotEmpty();

            RuleFor(x => x.IdGroupMailingList).NotEmpty();
        }
    }
}
