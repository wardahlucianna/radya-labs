namespace BinusSchool.Data.Model.User.FnUser.NotificationHistory
{
    public class UpdateNotificationReadRequest
    {
        public string IdNotification { get; set; }
        public string UserId { get; set; }
        public bool MarkAsRead { get; set; }
    }
}
