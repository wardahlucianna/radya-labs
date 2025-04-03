using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Scheduling.FnSchedule.CertificateTemplate.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Scheduling.FnSchedule.CertificateTemplate
{
    public class SendEmailApprovalCertificateTemplateNotificationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        private string _smtpMail;
        private readonly string _emailErrorTo = "riki@radyalabs.id";
        private readonly string _emailErrorCC = "riki@radyalabs.id";
        public SendEmailApprovalCertificateTemplateNotificationHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }
        
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();
            _smtpMail = Configuration.GetSection($"Smtp:connection").Get<string>();

            // var param = await Request.GetBody<SendNotificationToParentRequest>();

            SendMailHelperSender("riki@radyalabs.id", "riki@radyalabs.id", "riki@radyalabs.id", "riki@radyalabs.id", "StudentProfileUpdateNotification", "Student Update Profile", "hehe", "Samuel Edsel Fernandez", "BN001180614");

            return Request.CreateApiResult2();
        }

        public bool SendMailHelperSender(string strTo, string strCC, string strBCC, string strFrom,
           string strFromDisplay, string strSubject, string strMessage, string studentName, string userSenderID)
        {

            try
            {
                //change the value to true for send email to real
                bool sendReal = false;
                MailMessage objMM = new MailMessage();
                objMM.From = new MailAddress(strFrom, strFromDisplay);
                objMM.To.Clear();

                //Sending email to REAL (parent and student)
                if (sendReal)
                {
                    String[] emails = strTo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (String email in emails)
                    {
                        objMM.To.Add(email.Trim());
                    }

                    objMM.CC.Clear();
                    emails = strCC.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String email in emails)
                    {
                        objMM.CC.Add(email.Trim());
                    }

                    objMM.Bcc.Clear();
                    if (strBCC.Trim() != "")
                    {
                        String[] EmailBCC = strBCC.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (String email in EmailBCC)
                        {
                            objMM.Bcc.Add(email.Trim());
                        }
                    }

                }
                else //Sending email to testing email
                {
                    String[] emails = _emailErrorTo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (String email in emails)
                    {
                        objMM.To.Add(email);
                    }

                    emails = _emailErrorCC.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String email in emails)
                    {
                        objMM.CC.Add(email);
                    }

                    strMessage += "<br /><br /><hr />";
                    strMessage += "<br /><b><h1>This is a testing email ! </h1></b>";
                    strMessage += "<br />Sent To : ";
                    strMessage += strTo;
                    strMessage += "<br />CC To : ";
                    strMessage += strCC;
                    strMessage += "<br />Bcc To : ";
                    strMessage += strBCC;
                    strMessage += "<br /><hr />";
                }



                objMM.Subject = strSubject;
                objMM.Priority = MailPriority.Normal;
                objMM.IsBodyHtml = true;
                objMM.Body = strMessage;


                SmtpClient client = new SmtpClient(_smtpMail);
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
                client.Port = 25;
                client.Send(objMM);
                objMM.Dispose();
                GC.Collect();

                return true;
            }
            catch (Exception ex)
            {
                string strSubjectEmail = "Error Sending Student Update Notification";
                string strBody = "";
                strBody += "<b><h2>Message Error</h2></b>";
                strBody += "<hr /><br /><table border='0'>";

                //strBody += "<tr><td>Sender</td>";
                //strBody += "<td>: " + userSenderID + "</td></tr>";

                //strBody += "<tr><td>Student Name</td>";
                //strBody += "<td>: " + studentName + "</td></tr>";

                //strBody += "<tr><td>Email</td>";
                //strBody += "<td>: " + strTo + "</td></tr>";

                strBody += "</table><br />";
                MailHelperSenderEmailError(strFrom, strFromDisplay, strSubjectEmail, strBody, ex.Message);
                return false;
            }
        }

        public void MailHelperSenderEmailError(string strFrom, string strfromDisplay, string strSubject, string strBody, string errorMessage)
        {
            String[] emailTo = _emailErrorTo.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            String[] emailCC = _emailErrorCC.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            MailMessage objMM = new MailMessage();
            //objMM.From = new MailAddress(EmailFrom, EmailFromName);
            objMM.From = new MailAddress(strFrom, strfromDisplay);

            objMM.To.Clear();

            foreach (String email in emailTo)
            {
                objMM.To.Add(email);
            }

            objMM.CC.Clear();

            foreach (String email in emailCC)
            {
                objMM.CC.Add(email);
            }

            strBody += "<hr /><br />" + errorMessage;

            objMM.Bcc.Clear();
            objMM.Subject = strSubject;
            objMM.Priority = MailPriority.Normal;
            objMM.IsBodyHtml = true;
            objMM.Body = strBody;


            SmtpClient client = new SmtpClient(_smtpMail);
            client.Credentials = CredentialCache.DefaultNetworkCredentials;
            client.Port = 25;
            client.Send(objMM);
            objMM.Dispose();
            GC.Collect();
        }

    }
}