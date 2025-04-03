using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class MessageAttachment 
    {
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public int Filesize { get; set; }
    }
    public class AddMessageRequest
    {
        public string IdMessage { get; set; }
        public string IdSender { get; set; }
        public UserMessageType Type { get; set; }
        public bool IsSetSenderAsSchool { get; set; }
        public string Subject { get; set; }
        public string IdMessageCategory { get; set; }
        public string Content { get; set; }
        public DateTime? PublishStartDate { get; set; }
        public DateTime? PublishEndDate { get; set; }
        public bool IsAllowReply { get; set; }
        public DateTime? ReplyStartDate { get; set; }
        public DateTime? ReplyEndDate { get; set; }
        public bool IsMarkAsPinned { get; set; }
        public bool IsDraft { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdSchool { get; set; }
        public List<string> Recepients { get; set; }
        public List<MessageAttachment> Attachments { get; set; }
        public List<MessageFor> MessageFor { get; set; }
        public bool IsSendEmail { get; set; }
        public bool IsEdit { get; set; }
        public List<string> GroupMembers { get; set; }
    }

    public class MessageFor
    {
        public string IdMessageFor { get; set; }
        public UserRolePersonalOptionRole Role { get; set; }
        public MessageForOption Option { get; set; }
        public List<string> Depertements { get; set; }
        public List<string> TeacherPositions { get; set; }
        public List<string> Personal { get; set; }
        public List<MessageForGrade> Grade { get; set; }
    }

    public class MessageForGrade
    {
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
        public List<string> IdHomeroom { get; set; }
    }
}
