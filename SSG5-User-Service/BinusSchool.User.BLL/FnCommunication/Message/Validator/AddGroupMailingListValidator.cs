using BinusSchool.Data.Model.User.FnCommunication.Message;
using FluentValidation;
using System.Linq;

namespace BinusSchool.User.FnCommunication.Message.Validator
{
    public class AddGroupMailingListValidator : AbstractValidator<AddGroupMailingListRequest>
    {
        public AddGroupMailingListValidator()
        {

            RuleFor(x => x.GroupName).NotEmpty();

            RuleFor(x => x.Description).NotEmpty();

            RuleFor(x => x.IdUser).NotEmpty();

            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
