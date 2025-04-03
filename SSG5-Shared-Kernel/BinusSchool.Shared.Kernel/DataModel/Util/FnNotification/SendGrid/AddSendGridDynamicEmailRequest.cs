using System.Collections.Generic;

namespace BinusSchool.Data.Model.Util.FnNotification.SendGrid
{
    public class AddSendGridDynamicEmailRequest
    {
        /// <summary>
        /// SendGrid dynamic template id
        /// </summary>
        public string IdTemplate { get; set; }
        
        /// <summary>
        /// Email sender, use registered email in portal, otherwise set empty
        /// </summary>
        public string From { get; set; }
        
        /// <summary>
        /// Email recipient
        /// </summary>
        public IEnumerable<string> To { get; set; }
        
        /// <summary>
        /// Email CC
        /// </summary>
        public IEnumerable<string> Cc { get; set; }
        
        /// <summary>
        /// Email BCC
        /// </summary>
        public IEnumerable<string> Bcc { get; set; }
        
        /// <summary>
        /// Template data used by template, see <a href="https://docs.sendgrid.com/for-developers/sending-email/using-handlebars#handlebarjs-reference">the doc</a>
        /// </summary>
        public IDictionary<string, object> TemplateData { get; set; }
        
        /// <summary>
        /// Email categories
        /// </summary>
        public IEnumerable<string> Categories { get; set; }
    }
}
