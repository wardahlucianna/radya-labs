using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Util.FnNotification.SmtpEmail;
using BinusSchool.Util.FnNotification.SmtpEmail.Validator;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Util.FnNotification.SmtpEmail
{
    public class SendSmtpEmailHandler : FunctionsHttpSingleHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IFunctionsConfigurationBuilder _builder;

        // private readonly List<string> _emailBccDefault = new List<string> { "group-itdevelopmentschools@binus.edu", "Simprug-ISDevelopment@binus.edu", "group-itdevelopmentschoolserpong@binus.edu" };
        // private readonly List<string> _emailErrorTo = new List<string> { "group-itdevelopmentschools@binus.edu" };
        // private readonly List<string> _emailErrorCC = new List<string> { "Simprug-ISDevelopment@binus.edu", "group-itdevelopmentschoolserpong@binus.edu" };

        private readonly List<string> _emailBccDefault = new List<string> { "itdevschool@binus.edu" };
        private readonly List<string> _emailErrorTo = new List<string> { "group-itdevelopmentschools@binus.edu" };
        private readonly List<string> _emailErrorCC = new List<string> { "itdevschool@binus.edu"};

        public SendSmtpEmailHandler(
            IConfiguration configuration,
            IFunctionsConfigurationBuilder builder)
        {
            _configuration = configuration;
            _builder = builder;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SendSmtpEmailRequest, SendSmtpEmailValidator>();

            // get environment
            var hostContext = _builder.GetContext();

            // Workaround for unstable EnvironmentName in Azure 
            // see https://github.com/Azure/azure-functions-host/issues/6239
            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                hostContext.EnvironmentName;

            string additionalEnvSubject = string.IsNullOrWhiteSpace(envName) ? "" : envName.ToUpper().Contains("PRODUCTION") ? "" : (string.Format("(TESTING {0}) ", envName.ToUpper()));
            string additionalEnvErrorSubject = string.IsNullOrWhiteSpace(envName) ? "" : (string.Format("({0}) ", envName.ToUpper()));

            var result = new SendSmtpEmailResult();

            var senderEmail = new EmailAddress()
            {
                Email = string.IsNullOrEmpty(_configuration.GetValue<string>($"EmailSender:{param.IdSchool?.Trim()}:Email")) ? _configuration.GetValue<string>($"EmailSender:Default:Email") : _configuration.GetValue<string>($"EmailSender:{param.IdSchool?.Trim()}:Email"),
                Name = string.IsNullOrEmpty(_configuration.GetValue<string>($"EmailSender:{param.IdSchool?.Trim()}:Name")) ? _configuration.GetValue<string>($"EmailSender:Default:Name") : _configuration.GetValue<string>($"EmailSender:{param.IdSchool?.Trim()}:Name")
            };

            try
            {
                using (var smtp = new SmtpClient())
                {
                    smtp.Port = _configuration.GetValue<int>("SmtpClient:Port");
                    smtp.Host = _configuration.GetValue<string>("SmtpClient:Host");
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(senderEmail.Email, _configuration.GetValue<string>("SmtpClient:Password"));
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    MailMessage message = new MailMessage();

                    message.From = new MailAddress(senderEmail.Email, senderEmail.Name);

                    //if (string.IsNullOrEmpty(param.SmtpConfiguration.FromDisplayName))
                    //    message.From = new MailAddress(param.SmtpConfiguration.FromAddress);
                    //else
                    //    message.From = new MailAddress(param.SmtpConfiguration.FromAddress, param.SmtpConfiguration.FromDisplayName);

                    if (param.RecepientConfiguration.ToList.Count == 0)
                        throw new BadRequestException("Email doesn't have recepient (To) address");

                    // To
                    foreach (var to in param.RecepientConfiguration.ToList)
                    {
                        if (!StringUtil.IsValidEmailAddress(to.Address))
                            continue;

                        if (string.IsNullOrEmpty(to.DisplayName))
                            message.To.Add(new MailAddress(to.Address));
                        else
                            message.To.Add(new MailAddress(to.Address, to.DisplayName));
                    }

                    // Cc
                    if(param.RecepientConfiguration.CcList != null && param.RecepientConfiguration.CcList?.Count != 0)
                    {
                        foreach (var cc in param.RecepientConfiguration.CcList)
                        {
                            if (!StringUtil.IsValidEmailAddress(cc.Address))
                                continue;

                            if (string.IsNullOrEmpty(cc.DisplayName))
                                message.CC.Add(new MailAddress(cc.Address));
                            else
                                message.CC.Add(new MailAddress(cc.Address, cc.DisplayName));
                        }
                    }

                    // Bcc
                    if (param.RecepientConfiguration.BccList != null && param.RecepientConfiguration.BccList?.Count != 0)
                    {
                        foreach (var bcc in param.RecepientConfiguration.BccList)
                        {
                            if (!StringUtil.IsValidEmailAddress(bcc.Address))
                                continue;

                            if (string.IsNullOrEmpty(bcc.DisplayName))
                                message.Bcc.Add(new MailAddress(bcc.Address));
                            else
                                message.Bcc.Add(new MailAddress(bcc.Address, bcc.DisplayName));
                        }
                    }

                    foreach (var bcc in _emailBccDefault)
                    {
                        if (!StringUtil.IsValidEmailAddress(bcc))
                            continue;

                        message.Bcc.Add(bcc);
                    }

                    message.Subject = additionalEnvSubject + param.MessageContent.Subject;
                    message.IsBodyHtml = true;
                    message.Body = param.MessageContent.BodyHtml;

                    //if (param.MessageContent.AttachmentList.Count != 0)
                    //{
                    //    foreach (var attachment in param.MessageContent.AttachmentList)
                    //    {
                    //        message.Attachments.Add(attachment);
                    //    }
                    //}

                    smtp.Send(message);
                }

                result = new SendSmtpEmailResult()
                {
                    IsSuccess = true,
                    ErrorMessage = ""
                };
            }
            catch (Exception ex)
            {
                result = new SendSmtpEmailResult()
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message.ToString()
                };

                #region Send error email notification
                using (var smtp = new SmtpClient())
                {
                    smtp.Port = _configuration.GetValue<int>("SmtpClient:Port");
                    smtp.Host = _configuration.GetValue<string>("SmtpClient:Host");
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(senderEmail.Email, _configuration.GetValue<string>("SmtpClient:Password"));
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    MailMessage message = new MailMessage();

                    message.From = new MailAddress(senderEmail.Email, senderEmail.Name);

                    foreach (var errorTo in _emailErrorTo)
                        message.To.Add(errorTo);

                    foreach (var errorCc in _emailErrorCC)
                        message.CC.Add(errorCc);

                    message.Subject = additionalEnvErrorSubject + "Send Email Error (New BSIS Application) - SMTP";
                    message.IsBodyHtml = false;
                    message.Body = "ID SCHOOL:\n" + (string.IsNullOrWhiteSpace(param.IdSchool) ? "-" : param.IdSchool) +
                                    
                                    "\n\n\nUSER LOGIN INFORMATION" +
                                    "\nID User: " + (AuthInfo == null ? "-" : AuthInfo.UserId) +
                                    "\nUsername: " + (AuthInfo == null ? "-" : AuthInfo.UserName) +
                                    "\nUser Email: " + (AuthInfo == null ? "-" : AuthInfo.Email) +

                                    "\n\n\nEXCEPTION/ERROR INFORMATION" +
                                    "\nException: " + (ex == null ? "-" : ex.ToString()) +

                                    "\n\n\nEMAIL CONTENT INFORMATION" +
                                    "\nSubject:\n" + (string.IsNullOrWhiteSpace(param.MessageContent?.Subject) ? "-" : param.MessageContent?.Subject) +
                                    "\n\nTo List:\n" + ((param.RecepientConfiguration?.ToList == null || param.RecepientConfiguration?.ToList.Count == 0) ? "-" : string.Join(", ", param.RecepientConfiguration.ToList.Select(x => x.Address))) +
                                    "\n\nCC List:\n" + ((param.RecepientConfiguration?.CcList == null || param.RecepientConfiguration?.CcList.Count == 0) ? "-" : string.Join(", ", param.RecepientConfiguration.CcList.Select(x => x.Address))) +
                                    "\n\nBCC List:\n" + ((param.RecepientConfiguration?.BccList == null || param.RecepientConfiguration?.BccList.Count == 0) ? "-" : string.Join(", ", param.RecepientConfiguration.BccList.Select(x => x.Address))) +
                                    "\n\nBody HTML:\n" + (string.IsNullOrWhiteSpace(param.MessageContent?.BodyHtml) ? "-" : param.MessageContent?.BodyHtml);

                    smtp.Send(message);
                }
                #endregion
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
