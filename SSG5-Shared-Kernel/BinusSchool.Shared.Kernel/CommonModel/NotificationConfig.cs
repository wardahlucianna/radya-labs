using System;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Common.Model
{
    public class NotificationConfig
    {
        public NotificationConfig() => EnPush = new EnablePushConfig();

        public EnablePushConfig EnPush { get; set; }
        public bool EnEmail { get; set; }

        public bool NotificationEnabled => EnEmail || EnPush.Web || EnPush.Mobile;
    }
        
    public class EnablePushConfig
    {
        public bool Web { get; set; }
        public bool Mobile { get; set; }

        public bool PushEnabled => Web || Mobile;
        public AppPlatform[] AllowedPlatforms => (Web, Mobile) switch
        {
            (true, false) => new[] { AppPlatform.Web },
            (false, true) => new[] { AppPlatform.Android, AppPlatform.iOS },
            (true, true) => new[] { AppPlatform.Web, AppPlatform.Android, AppPlatform.iOS },
            _ => Array.Empty<AppPlatform>()
        };
    }
}
