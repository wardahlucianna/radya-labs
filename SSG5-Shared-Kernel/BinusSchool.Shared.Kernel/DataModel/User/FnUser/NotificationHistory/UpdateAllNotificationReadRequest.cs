
namespace BinusSchool.Data.Model.User.FnUser.NotificationHistory
{
    public class UpdateAllNotificationReadRequest
    {
        public string IdSchool { get; set; }
        public string UserId { get; set; }
        public bool MarkAsRead { get; set; }
    }
}
