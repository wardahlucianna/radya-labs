using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnUser.NotificationHistory
{
    public class GetNotificationHistoryRequest : CollectionSchoolRequest
    {
        public string UserId { get; set; }
        public NotificationType? Type { get; set; }
        public bool? IsRead { get; set; }
    }
}
