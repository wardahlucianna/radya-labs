using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Common.Functions.Abstractions
{
    public interface INotificationManager
    {
        /// <summary>
        /// Get email and push notification template.
        /// See <see href="https://docs.google.com/spreadsheets/d/1AneDkrChrxIRQcdyXbHZLmwfutuujT91uWFlR6_upvk/edit?usp=sharing"/> for reference.
        /// </summary>
        /// <param name="idSchool">Id school</param>
        /// <param name="idScenario">Id of scenario describe in the docs</param>
        /// <returns>Template for email and push notification</returns>
        Task<NotificationTemplate> GetTemplate(string idSchool, string idScenario);

        /// <summary>
        /// Save notification history.
        /// </summary>
        /// <param name="notification">Notification to save</param>
        Task SaveNotification(NotificationHistory notification);
        
        /// <summary>
        /// Send email notifications with SendGrid.
        /// The email will be queued in Table Storage before being sent,
        /// and the Queue Trigger in other service will send the email to SendGrid service.
        /// </summary>
        /// <param name="message">Object of SendGrid message</param>
        /// <returns>Queue message id</returns>
        Task<string> SendEmail(SendGridMessage message);

        /// <summary>
        /// Send email notification with SMTP.
        /// The email will be queued in Table Storage before being sent,
        /// and the Queue Trigger in other service will send the email to SMTP service.
        /// </summary>
        /// <param name="message">Object of EmailData message</param>
        /// <returns>Queue message id</returns>
        Task<string> SendSmtp(EmailData message);

        /// <summary>
        /// Send push notification with Firebase Cloud Messaging (FCM).
        /// The push notification will be queued in Table Storage before being sent,
        /// and the Queue Trigger in other service will send the push notification to FCM service.
        /// </summary>
        /// <param name="message">Object of MulticastMessage notification</param>
        /// <returns>Queue message id</returns>
        Task<string> SendPushNotification(MulticastMessage message);
    }
}
