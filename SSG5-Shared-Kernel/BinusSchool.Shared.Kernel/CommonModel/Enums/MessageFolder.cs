using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
{
    public enum MessageFolder
    {
        Inbox,
        Delete,
        Trash,
        Sent,
        Unsend,
        [Description("Trash Permanent")]
        TrashPermanent
    }
}
