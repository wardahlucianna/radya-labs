using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Util.FnNotification.SendGrid
{
    public class SendSendGridEmailRequest
    {
        public string IdSchool { get; set; }
        public SendSendGridEmailRequest_RecepientConfiguration RecepientConfiguration { get; set; }
        public SendSendGridEmailRequest_MessageContent MessageContent { get; set; }
    }
    public class SendSendGridEmailRequest_RecepientConfiguration
    {
        public List<SendSendGridEmailRequest_AddressBuilder> ToList { get; set; }
        public List<SendSendGridEmailRequest_AddressBuilder> CcList { get; set; }
        public List<SendSendGridEmailRequest_AddressBuilder> BccList { get; set; }
    }

    public class SendSendGridEmailRequest_AddressBuilder
    {
        public string Address { get; set; }
        public string DisplayName { get; set; }
    }

    public class SendSendGridEmailRequest_MessageContent
    {
        public string Subject { get; set; }
        public string BodyHtml { get; set; }
        //public List<Attachment> AttachmentList { get; set; }
    }
}
