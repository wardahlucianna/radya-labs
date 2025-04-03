using BinusSchool.Data.Model.User.FnCommunication.Message;
using FluentValidation;
using System.Linq;

namespace BinusSchool.User.FnCommunication.Message.Validator
{
    public class UpdateGroupMailingListValidator : AbstractValidator<UpdateGroupMailingListRequest>
    {
        public UpdateGroupMailingListValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.GroupName).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
        }
    }
}
