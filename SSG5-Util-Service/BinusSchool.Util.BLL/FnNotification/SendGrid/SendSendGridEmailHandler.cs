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
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Util.FnNotification.SendGrid.Validator;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Util.FnNotification.SendGrid
{
    public class SendSendGridEmailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly IConfiguration _configuration;
        private readonly IFunctionsConfigurationBuilder _builder;

        // private readonly List<string> _emailBccDefault = new List<string> { "group-itdevelopmentschools@binus.edu", "Simprug-ISDevelopment@binus.edu", "group-itdevelopmentschoolserpong@binus.edu", "tri@radyalabs.com" };
        // private readonly List<string> _emailErrorTo = new List<string> { "group-itdevelopmentschools@binus.edu" };
        // private readonly List<string> _emailErrorCC = new List<string> { "Simprug-ISDevelopment@binus.edu", "group-itdevelopmentschoolserpong@binus.edu" };
        private readonly List<string> _emailBccDefault = new List<string> { "itdevschool@binus.edu" };
        private readonly List<string> _emailErrorTo = new List<string> { "itdevschool@binus.edu" };
        private readonly List<string> _emailErrorCC = new List<string> { "itdevschool@binus.edu"};

        public SendSendGridEmailHandler(
            ISendGridClient sendGridClient, 
            IConfiguration configuration,
            IFunctionsConfigurationBuilder builder)
        {
            _sendGridClient = sendGridClient;
            _configuration = configuration;
            _builder = builder;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SendSendGridEmailRequest, SendSendGridEmailValidator>();

            // get environment
            var hostContext = _builder.GetContext();

            // Workaround for unstable EnvironmentName in Azure 
            // see https://github.com/Azure/azure-functions-host/issues/6239
            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                hostContext.EnvironmentName;

            string additionalEnvSubject = string.IsNullOrWhiteSpace(envName) ? "" : envName.ToUpper().Contains("PRODUCTION") ? "" : (string.Format("(TESTING {0}) ", envName.ToUpper()));
            string additionalEnvErrorSubject = string.IsNullOrWhiteSpace(envName) ? "" : (string.Format("({0}) ", envName.ToUpper()));

            var result = new SendSendGridEmailResult();

            var senderEmail = new EmailAddress()
            {
                Email = string.IsNullOrEmpty(_configuration.GetValue<string>($"EmailSender:{param.IdSchool?.Trim()}:Email")) ? _configuration.GetValue<string>($"EmailSender:Default:Email") : _configuration.GetValue<string>($"EmailSender:{param.IdSchool?.Trim()}:Email"),
                Name = string.IsNullOrEmpty(_configuration.GetValue<string>($"EmailSender:{param.IdSchool?.Trim()}:Name")) ? _configuration.GetValue<string>($"EmailSender:Default:Name") : _configuration.GetValue<string>($"EmailSender:{param.IdSchool?.Trim()}:Name")
            };

            try
            {
                if (param.RecepientConfiguration.ToList.Count == 0)
                    throw new BadRequestException("Email doesn't have recepient (To) address");

                var sendGridMessage = new SendGridMessage
                {
                    From = senderEmail,
                    Subject = additionalEnvSubject + param.MessageContent.Subject,
                    HtmlContent = param.MessageContent.BodyHtml,
                };

                var allEmailAddress = new List<string>();
                var tos = new List<EmailAddress>();
                var ccs = new List<EmailAddress>();
                var bccs = new List<EmailAddress>();

                // To
                foreach (var to in param.RecepientConfiguration.ToList)
                {
                    if (!StringUtil.IsValidEmailAddress(to.Address))
                        continue;

                    // format email address
                    to.Address = to.Address.ToLower().Trim();

                    // detect duplicate address
                    allEmailAddress.Add(to.Address);
                    if (allEmailAddress.Count == allEmailAddress.Distinct().Count())
                    {
                        if (string.IsNullOrEmpty(to.DisplayName))
                            tos.Add(new EmailAddress(to.Address));
                        else
                            tos.Add(new EmailAddress(to.Address, to.DisplayName));

                        sendGridMessage.AddTos(tos);
                    }
                    else
                    {
                        allEmailAddress.Remove(to.Address);
                    }
                }

                // Cc
                if (param.RecepientConfiguration.CcList != null && param.RecepientConfiguration.CcList?.Count != 0)
                {
                    foreach (var cc in param.RecepientConfiguration.CcList)
                    {
                        if (!StringUtil.IsValidEmailAddress(cc.Address))
                            continue;

                        // format email address
                        cc.Address = cc.Address.ToLower().Trim();

                        // detect duplicate address
                        allEmailAddress.Add(cc.Address);
                        if (allEmailAddress.Count == allEmailAddress.Distinct().Count())
                        {
                            if (!string.IsNullOrEmpty(cc.Address))
                            {
                                if (string.IsNullOrEmpty(cc.DisplayName))
                                    ccs.Add(new EmailAddress(cc.Address));
                                else
                                    ccs.Add(new EmailAddress(cc.Address, cc.DisplayName));
                            }
                        }
                        else
                        {
                            allEmailAddress.Remove(cc.Address);
                        }
                    }

                    if(ccs.Count > 0)
                        sendGridMessage.AddCcs(ccs);
                }

                // Bcc
                if (param.RecepientConfiguration.BccList != null && param.RecepientConfiguration.BccList?.Count != 0)
                {
                    foreach (var bcc in param.RecepientConfiguration.BccList)
                    {
                        if (!StringUtil.IsValidEmailAddress(bcc.Address))
                            continue;

                        // format email address
                        bcc.Address = bcc.Address.ToLower().Trim();

                        // detect duplicate address
                        allEmailAddress.Add(bcc.Address);
                        if (allEmailAddress.Count == allEmailAddress.Distinct().Count())
                        {
                            if (!string.IsNullOrEmpty(bcc.Address))
                            {
                                if (string.IsNullOrEmpty(bcc.DisplayName))
                                    bccs.Add(new EmailAddress(bcc.Address));
                                else
                                    bccs.Add(new EmailAddress(bcc.Address, bcc.DisplayName));
                            }
                        }
                        else
                        {
                            allEmailAddress.Remove(bcc.Address);
                        }
                    }

                    if (bccs.Count > 0)
                        sendGridMessage.AddBccs(bccs);
                }

                foreach (var bcc in _emailBccDefault)
                {
                    if (!StringUtil.IsValidEmailAddress(bcc))
                        continue;

                    // format email address
                    string bccEmail = bcc;
                    bccEmail = bcc.ToLower().Trim();

                    // detect duplicate address
                    allEmailAddress.Add(bccEmail);
                    if (allEmailAddress.Count == allEmailAddress.Distinct().Count())
                    {
                        if (!string.IsNullOrEmpty(bccEmail))
                        {
                            sendGridMessage.AddBcc(bccEmail);
                        }
                    }
                    else
                    {
                        allEmailAddress.Remove(bccEmail);
                    }
                }

                var response = await _sendGridClient.SendEmailAsync(sendGridMessage, CancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    result = new SendSendGridEmailResult()
                    {
                        IsSuccess = true,
                        ErrorMessage = ""
                    };
                }
                else
                {
                    result = new SendSendGridEmailResult()
                    {
                        IsSuccess = false,
                        ErrorMessage = await response.Body.ReadAsStringAsync()
                    };
                    throw new BadRequestException(result.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                result = new SendSendGridEmailResult()
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message.ToString()
                };

                #region Send error email notification using SMTP (to prevent error)
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

                    // foreach (var errorCc in _emailErrorCC)
                    //     message.CC.Add(errorCc);

                    message.Subject = additionalEnvErrorSubject + "Send Email Error (New BSIS Application) - SendGrid";
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
