using BinusSchool.Data.Model.User.FnUser.NotificationHistory;
using FluentValidation;

namespace BinusSchool.User.FnUser.NotificationHistory.Validator
{
    public class UpdateNotificationReadValidator : AbstractValidator<UpdateNotificationReadRequest>
    {
        public UpdateNotificationReadValidator()
        {
            RuleFor(x => x.IdNotification).NotEmpty();

            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
