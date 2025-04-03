using BinusSchool.Data.Model.User.FnUser.NotificationHistory;
using FluentValidation;

namespace BinusSchool.User.FnUser.NotificationHistory.Validator
{
    public class UpdateAllNotificationReadValidator : AbstractValidator<UpdateAllNotificationReadRequest>
    {
        public UpdateAllNotificationReadValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
