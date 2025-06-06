﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram;

namespace BinusSchool.Data.Model.Document.FnDocument.SendEmail
{
    public class SendEmailClearanceFormForParentRequest
    {
        public string IdSchool { get; set; }
        public string IdSurveyPeriod { get; set; }
        public string IdClearanceWeekPeriod { get; set; }
        public SendEmailClearanceFormForParentRequest_StudentSurveyData StudentSurveyData { get; set; }
        public BLPFinalStatus BLPFinalStatus { get; set; }
    }

    public class SendEmailClearanceFormForParentRequest_StudentSurveyData
    {
        public NameValueVm Student { get; set; }
        public NameValueVm Homeroom { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public NameValueVm Parent { get; set; }
        public string ParentEmail { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public NameValueVm BLPGroup { get; set; }
    }

    public class SendEmailClearanceFormForParentRequest_BuildTemplate
    {
        public string StatusDescription { get; set; }
        public bool IsUsingBarcodeCheckIn { get; set; }
        public string Footer { get; set; }
    }

    public class SendEmailClearanceFormForParentRequest_BuildData
    {
        public string StudentName { get; set; }
        public string StudentId { get; set; }
        public string HomeroomClass { get; set; }
        public string AcademicYear { get; set; }
        public string ParentName { get; set; }
        public string SubmissionDate { get; set; }
        public string BLPGroupName { get; set; }
        public string BarcodeGeneratedDate { get; set; }
        public List<GetBLPQuestionResult> BLPQuestionResultList { get; set; }
    }
}
