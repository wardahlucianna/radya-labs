using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BinusSchool.Common.Utils
{
    public static class RecipientUtil
    {
        public static string AddMrMsRecipients(string NameRecipient, bool IsStudent = false)
        {
            string recipient = default;

            if (!IsStudent)
                recipient = "Mr/Ms ";

            recipient += NameRecipient;

            return recipient;
        }
    }

}
