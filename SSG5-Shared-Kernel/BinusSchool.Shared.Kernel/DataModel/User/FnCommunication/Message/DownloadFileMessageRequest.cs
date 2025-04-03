using System;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class DownloadFileMessageRequest
    {
        public string FileName { get; set; }
        public string OriginFileName { get; set; }
    }
}