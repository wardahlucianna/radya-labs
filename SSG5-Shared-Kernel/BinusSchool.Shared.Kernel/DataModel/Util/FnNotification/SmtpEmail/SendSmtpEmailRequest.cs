using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace BinusSchool.Data.Model.Util.FnNotification.SmtpEmail
{
    public class SendSmtpEmailRequest
    {
        public string IdSchool { get; set; }
        //public SendSmtpEmailRequest_SmtpConfiguration SmtpConfiguration { get; set; }
        public SendSmtpEmailRequest_RecepientConfiguration RecepientConfiguration { get; set; }
        public SendSmtpEmailRequest_MessageContent MessageContent { get; set; }
    }

    //public class SendSmtpEmailRequest_SmtpConfiguration
    //{
    //    public int Port { get; set; }
    //    public string HostServer { get; set; }
    //    public string FromAddress { get; set; }
    //    public string FromDisplayName { get; set; }
    //    public string MailPassword { get; set; }
    //}

    public class SendSmtpEmailRequest_RecepientConfiguration
    {
        public List<SendSmtpEmailRequest_AddressBuilder> ToList { get; set; }
        public List<SendSmtpEmailRequest_AddressBuilder> CcList { get; set; }
        public List<SendSmtpEmailRequest_AddressBuilder> BccList { get; set; }
    }

    public class SendSmtpEmailRequest_AddressBuilder
    {
        public string Address { get; set; }
        public string DisplayName { get; set; }
    }

    public class SendSmtpEmailRequest_MessageContent
    {
        public string Subject { get; set; }
        public string BodyHtml { get; set; }
        //public List<Attachment> AttachmentList { get; set; }
    }
}
