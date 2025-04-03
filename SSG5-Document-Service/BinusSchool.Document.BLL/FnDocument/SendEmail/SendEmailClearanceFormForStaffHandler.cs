using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Document.FnDocument;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram;
using BinusSchool.Data.Model.Document.FnDocument.SendEmail;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Data.Model.Util.FnNotification.SmtpEmail;
using BinusSchool.Document.FnDocument.SendEmail.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Document.FnDocument.SendEmail
{
    public class SendEmailClearanceFormForStaffHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly ISendGrid _sendGridApi;
        private readonly IBlendedLearningProgram _blendedLearningProgramApi;
        private readonly IMachineDateTime _dateTime;
        private IDbContextTransaction _transaction;

        public SendEmailClearanceFormForStaffHandler(
            IDocumentDbContext dbContext,
            ISendGrid sendGridApi,
            IBlendedLearningProgram blendedLearningProgramApi,
            IMachineDateTime dateTime
            )
        {
            _dbContext = dbContext;
            _sendGridApi = sendGridApi;
            _blendedLearningProgramApi = blendedLearningProgramApi;
            _dateTime = dateTime;
        }

        //private readonly string _emailErrorTo = "danis.prasetyanto@binus.edu";//"austin.tanujaya@binus.edu";
        //private readonly string _emailErrorCC = "danis.prasetyanto@binus.edu";//"austin.tanujaya@binus.edu";
        private readonly string _emailErrorTo = "itdevschool@binus.edu";
        private readonly string _emailErrorCC = "itdevschool@binus.edu";

        private string _emailTemplate = "";
        private readonly string[] _selfFillQuestionType = new string[] { "text", "textarea", "date" };
        private readonly string[] _uploadFileQuestionType = new string[] { "file" };
        private readonly string[] _ignoreQuestionType = new string[] { "list" };

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SendEmailClearanceFormForStaffRequest, SendEmailClearanceFormForStaffValidator>();

            try
            {
                var BLPEmail = _dbContext.Entity<MsBLPEmail>()
                                .Include(bem => bem.BLPSetting)
                                .Include(bem => bem.RoleGroup)
                                .Where(x => x.BLPSetting.IdSchool == param.IdSchool &&
                                            x.IdSurveyCategory == "2" &&    // clearance form
                                            x.BLPFinalStatus == param.BLPFinalStatus &&
                                            x.AuditAction == param.AuditAction &&
                                            x.RoleGroup.Code.ToUpper().Trim() == "STAFF")
                                .FirstOrDefault();

                if (BLPEmail == null)
                {
                    return Request.CreateApiResult2();
                    //throw new BadRequestException("Failed to send email (ERR-3)");
                }

                var BLPAdditionalReceivers = _dbContext.Entity<MsBLPEmailAdditionalReceiver>()
                                                    .Include(bear => bear.User)
                                                    .Where(x => x.IdBLPEmail == BLPEmail.Id)
                                                    .ToList();

                // fill email template
                _emailTemplate = BLPEmail.Description;

                var BLPQuestionAnswerList = new List<GetBLPQuestionWithHistoryResult>();

                if (BLPEmail.ShowSurveyAnswer)
                {
                    // Get BLP Question and Answer
                    var BLPQuestionAnswer = _blendedLearningProgramApi.GetBLPQuestionWithHistory(new GetBLPQuestionWithHistoryRequest
                    {
                        IdSchool = param.IdSchool,
                        IdSurveyCategory = "2",    // clearance form
                        IdSurveyPeriod = param.IdSurveyPeriod,
                        IdClearanceWeekPeriod = string.IsNullOrEmpty(param.IdClearanceWeekPeriod) ? null : param.IdClearanceWeekPeriod,
                        IdStudent = param.StudentSurveyData.Student.Id
                    });

                    var BLPQuestionAnswerPayload = BLPQuestionAnswer.Result.Payload;

                    if (BLPQuestionAnswerPayload == null)
                        throw new BadRequestException("Failed to generate BLP question and answer");

                    BLPQuestionAnswerList = BLPQuestionAnswerPayload.ToList();
                }

                // Fill Email Template
                var buildTemplate = new SendEmailClearanceFormForStaffRequest_BuildTemplate()
                {
                    StatusDescription = BLPEmail?.StatusDescription,
                    IsUsingBarcodeCheckIn = (bool)BLPEmail?.BLPSetting.NeedBarcode ? true : false,
                    Footer = BLPEmail?.BLPSetting.FooterEmail
                };
                FillEmailTemplate(buildTemplate);

                // Fill Email Data
                var buildData = new SendEmailClearanceFormForStaffRequest_BuildData()
                {
                    StaffName = BLPAdditionalReceivers.Where(x => x.AddressType.ToUpper().Trim() == "TO").Select(x => x.User.DisplayName?.Trim()).FirstOrDefault(),
                    StudentId = param.StudentSurveyData.Student?.Id,
                    StudentName = param.StudentSurveyData.Student?.Name?.ToUpper(),
                    AcademicYear = param.StudentSurveyData.AcademicYear?.Description,
                    HomeroomClass = param.StudentSurveyData.Homeroom?.Name,
                    ParentName = param.StudentSurveyData.Parent?.Name?.ToUpper(),
                    BLPGroupName = param.StudentSurveyData.BLPGroup?.Name,
                    SubmissionDate = param.StudentSurveyData.SubmissionDate == null ? _dateTime.ServerTime.ToString("dd-MM-yyyy") : param.StudentSurveyData.SubmissionDate.Value.ToString("dd-MM-yyyy"),
                    BarcodeGeneratedDate = _dateTime.ServerTime.ToString("dd-MM-yyyy"),
                    BLPQuestionResultList = BLPQuestionAnswerList == null || BLPQuestionAnswerList.Count == 0 ? null : BLPQuestionAnswerList,
                };
                FillEmailData(buildData, param.AuditAction);


                var toList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var ccList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var bccList = new List<SendSendGridEmailRequest_AddressBuilder>();

                // main receiver
                //var to = new SendSendGridEmailRequest_AddressBuilder()
                //{
                //    //Address = "austin.tanujaya@binus.edu"
                //};

                //toList.Add(to);

                string additionalTo = "", additionalCC = "", additionalBCC = "";

                // additional receivers
                foreach (var BLPAdditionalReceiver in BLPAdditionalReceivers)
                {
                    var additionalReceiver = new SendSendGridEmailRequest_AddressBuilder()
                    {
                        Address = BLPAdditionalReceiver.User.Email,
                        DisplayName = BLPAdditionalReceiver.User.DisplayName
                    };

                    if (BLPAdditionalReceiver.AddressType.ToUpper().Trim() == "TO")
                    {
                        additionalTo += additionalReceiver.Address + ";";
                        toList.Add(additionalReceiver);
                    }

                    if (BLPAdditionalReceiver.AddressType.ToUpper().Trim() == "CC")
                    {
                        additionalCC += additionalReceiver.Address + ";";
                        ccList.Add(additionalReceiver);
                    }

                    if (BLPAdditionalReceiver.AddressType.ToUpper().Trim() == "BCC")
                    {
                        additionalBCC += additionalReceiver.Address + ";";
                        bccList.Add(additionalReceiver);
                    }
                }

                var sendEmail = await _sendGridApi.SendSendGridEmail(new SendSendGridEmailRequest
                {
                    IdSchool = param.IdSchool,
                    RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration()
                    {
                        ToList = toList,
                        CcList = ccList,
                        BccList = bccList
                    },
                    MessageContent = new SendSendGridEmailRequest_MessageContent
                    {
                        Subject = BLPEmail.EmailSubject?.Trim(),
                        BodyHtml = _emailTemplate
                    }
                });

                if (sendEmail.Payload?.IsSuccess == false)
                {
                    throw new BadRequestException("Failed to send email (ERR-4)");
                }

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                // send error email
                var toList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var ccList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var bccList = new List<SendSendGridEmailRequest_AddressBuilder>();

                toList.Add(new SendSendGridEmailRequest_AddressBuilder
                {
                    Address = _emailErrorTo
                });

                ccList.Add(new SendSendGridEmailRequest_AddressBuilder
                {
                    Address = _emailErrorCC
                });

                var sendEmail = await _sendGridApi.SendSendGridEmail(new SendSendGridEmailRequest
                {
                    IdSchool = param.IdSchool,
                    RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration()
                    {
                        ToList = toList,
                        CcList = ccList,
                        BccList = bccList
                    },
                    MessageContent = new SendSendGridEmailRequest_MessageContent
                    {
                        Subject = "Error Send Email Clearance For Staff",
                        BodyHtml = "SendEmailClearanceFormForStaffHandler\n\n" + ex.ToString()
                    }
                });

                throw new Exception(ex.Message);
            }
        }

        private void FillEmailTemplate(SendEmailClearanceFormForStaffRequest_BuildTemplate buildTemplate)
        {
            string barcodeCheckInTemplate = @"
                            <table role='presentation' cellpadding='0' cellspacing='0' width='95%' style='margin: 0 auto; vertical-align: top;'>
                                <!-- QR CODE STARTS HERE -->
                                <tr>
                                    <td style='width: 100%; padding: 0 10px; vertical-align: top; text-align: center; border-style: dotted;'>
                                        <h4 style='margin-top: 16px;'>
                                            <b>Student QR Code For Check In</b>
                                        </h4>
                    
                                        <img src='https://chart.googleapis.com/chart?cht=qr&chs=250x250&chl={{StudentId}}&choe=UTF-8' />

                                        <p style='margin-bottom: 16px;'>
                                            Student ID : {{StudentId}}<br>
                                            Student Name : {{StudentName}}<br>
                                            Generated date : {{BarcodeGeneratedDate}}
                                        </p>
                                    </td>
                                </tr>
                            </table>";


            _emailTemplate = _emailTemplate
                                .Replace("[[StatusDescription]]", (string.IsNullOrEmpty(buildTemplate.StatusDescription) ? "" : buildTemplate.StatusDescription))
                                .Replace("[[BarcodeCheckIn]]", (buildTemplate.IsUsingBarcodeCheckIn ? barcodeCheckInTemplate : ""))
                                .Replace("[[Footer]]", (string.IsNullOrEmpty(buildTemplate.Footer) ? "" : buildTemplate.Footer));
        }

        private void FillEmailData(SendEmailClearanceFormForStaffRequest_BuildData buildData, AuditAction auditAction)
        {
            string questionAnswerBuilder = "";

            if (buildData.BLPQuestionResultList != null || buildData.BLPQuestionResultList?.Count > 0)
            {
                // format for inserted data
                if (auditAction == AuditAction.Insert)
                    questionAnswerBuilder = QuestionAnswerFormatBuilderInsert(buildData.BLPQuestionResultList);

                // format for updated data
                else if (auditAction == AuditAction.Update)
                    questionAnswerBuilder = QuestionAnswerFormatBuilderUpdate(buildData.BLPQuestionResultList);
            }

            _emailTemplate = _emailTemplate
                                .Replace("{{StaffName}}", (buildData.StaffName ?? "{{StaffName}}"))
                                .Replace("{{StudentName}}", (buildData.StudentName ?? "{{StudentName}}"))
                                .Replace("{{StudentId}}", (buildData.StudentId ?? "{{StudentId}}"))
                                .Replace("{{HomeroomClass}}", (buildData.HomeroomClass ?? "{{HomeroomClass}}"))
                                .Replace("{{AcademicYear}}", (buildData.AcademicYear ?? "{{AcademicYear}}"))
                                .Replace("{{ParentName}}", (buildData.ParentName ?? "{{ParentName}}"))
                                .Replace("{{SubmissionDate}}", (buildData.SubmissionDate ?? "{{SubmissionDate}}"))
                                .Replace("{{GroupName}}", (buildData.BLPGroupName ?? "{{GroupName}}"))
                                .Replace("{{BarcodeGeneratedDate}}", (buildData.BarcodeGeneratedDate ?? "{{BarcodeGeneratedDate}}"))
                                .Replace("{{SurveyQuestionAnswer}}", questionAnswerBuilder);
        }

        private string QuestionAnswerFormatBuilderInsert(List<GetBLPQuestionWithHistoryResult> BLPQuestionResultList)
        {
            string finalQuestionAnswerFormat = "";

            foreach (var BLPQuestionSectionResult in BLPQuestionResultList)
            {
                finalQuestionAnswerFormat += @"<table role='presentation' cellpadding='0' cellspacing='0' width='95%' style='margin: 0 auto; vertical-align: top;'>{{section}}</table>";

                var sectionQuestionAnswer = @"
                                <tr>
                                    <td style='padding: 10px 0px; vertical-align: top;' colspan='3'>
                                        <h4 style='margin-top: 16px;'>
                                            <b>{{sectionName}}</b>
                                        </h4>
                                
                                        <p>
                                            {{sectionDescription}}
                                        </p>
                                
                                    </td>
                                </tr>";

                sectionQuestionAnswer = sectionQuestionAnswer
                                            .Replace("{{sectionName}}", BLPQuestionSectionResult.SectionName)
                                            .Replace("{{sectionDescription}}", BLPQuestionSectionResult.Description);

                if (BLPQuestionSectionResult.QuestionAnswerList.Count != 0)
                {
                    sectionQuestionAnswer += @"
                                <tr>
                                    <th style='width: 60%; border: 1px solid; padding: 0 10px; vertical-align: top;'>Questions</th>
                                    <th style='width: 40%; border: 1px solid; padding: 0 10px; vertical-align: top;'>Answers</th>
                                </tr>";

                    int questionNumber = 0;
                    foreach (var questionAnswerList in BLPQuestionSectionResult.QuestionAnswerList)
                    {
                        if (questionAnswerList.AnswerGroup == null)
                        {
                            sectionQuestionAnswer += @"
                                <tr>
                                    <td colspan='3' style='width: 100%; border: 1px solid; padding: 0 10px; vertical-align: top;'>
                                        {{questionName}}
                                    </td>
                                </tr>";

                            sectionQuestionAnswer = sectionQuestionAnswer
                                                    .Replace("{{questionName}}", questionAnswerList.QuestionName);
                        }
                        else
                        {
                            questionNumber++;

                            sectionQuestionAnswer += @"
                                <tr>
                                    <td style='width: 60%; border: 1px solid; padding: 0 10px; vertical-align: top;'>
                                        {{questionName}}
                                    </td>
                                    <td style='width: 40%; border: 1px solid; padding: 0 10px; vertical-align: top;'>
                                        <b>{{studentAnswerDescription}}</b>
                                    </td>
                                </tr>";

                            sectionQuestionAnswer = sectionQuestionAnswer
                                                    .Replace("{{questionName}}", questionAnswerList.QuestionName);

                            string answerText = "";
                            string answerType = questionAnswerList.AnswerGroup.AnswerType;

                            foreach (var answer in questionAnswerList.AnswerGroup.Answers)
                            {
                                if (answer.StudentAnswer != null)
                                {
                                    // check answer type
                                    string answerTemp = "";
                                    if (_selfFillQuestionType.Any(type => type == answerType))
                                        answerTemp = answer.StudentAnswer.Description;
                                    else if (_uploadFileQuestionType.Any(type => type == answerType))
                                        answerTemp = answer.StudentAnswer.FilePath;
                                    else
                                        answerTemp = answer.SurveyAnswerDesc;
                                    //

                                    if (string.IsNullOrEmpty(answerText))
                                        answerText += answerTemp;
                                    else
                                        answerText += "; " + answerTemp;
                                }
                            }

                            sectionQuestionAnswer = sectionQuestionAnswer
                                                    .Replace("{{studentAnswerDescription}}", answerText);

                            foreach (var answer in questionAnswerList.AnswerGroup.Answers)
                            {
                                if (answer.StudentAnswer != null || _ignoreQuestionType.Any(x => x == answerType))
                                {
                                    sectionQuestionAnswer = GetChildQuestionAnswerRecursionInsert(sectionQuestionAnswer, answer.Childs, answer.Childs?.ChildsQuestion, 30);
                                }
                            }
                        }
                    }
                }

                finalQuestionAnswerFormat += "<br><br>";

                finalQuestionAnswerFormat = finalQuestionAnswerFormat.Replace("{{section}}", sectionQuestionAnswer);
            }

            finalQuestionAnswerFormat += @"<hr style='color: black;'>";

            return finalQuestionAnswerFormat;
        }

        private string GetChildQuestionAnswerRecursionInsert(string htmlFormat, BLPQuestionWithHistory_BLPAnswerVm BLPChildAnswerGroup, string childQuestion, int paddingLeft)
        {
            if (BLPChildAnswerGroup != null)
            {
                string answerText = "";

                if (BLPChildAnswerGroup == null)
                {
                    htmlFormat += @$"<tr>
                                    <td colspan='2' style='width: 100%; border: 1px solid; padding: 0 10px; vertical-align: top; padding-left: {paddingLeft}px;'>" +
                                    @"
                                        {{questionName}}
                                    </td>
                                </tr>";

                    htmlFormat = htmlFormat
                                            .Replace("{{questionName}}", string.IsNullOrEmpty(childQuestion) ? "" : childQuestion);
                }
                else
                {
                    htmlFormat += @$"<tr>
                                    <td style='width: 60%; border: 1px solid; padding: 0 10px; vertical-align: top; padding-left: {paddingLeft}px;'>" + @"
                                        {{questionName}}
                                    </td>
                                    <td style='width: 40%; border: 1px solid; padding: 0 10px; vertical-align: top;'>
                                        <b>{{studentAnswerDescription}}</b>
                                    </td>
                                </tr>";

                    htmlFormat = htmlFormat
                                        .Replace("{{questionName}}", string.IsNullOrEmpty(childQuestion) ? "" : childQuestion);

                    string answerType = BLPChildAnswerGroup.AnswerType;

                    foreach (var BLPChildAnswer in BLPChildAnswerGroup.Answers)
                    {

                        if (BLPChildAnswer.StudentAnswer != null)
                        {
                            // check answer type
                            string answerTemp = "";
                            if (_selfFillQuestionType.Any(type => type == answerType))
                                answerTemp = BLPChildAnswer.StudentAnswer.Description;
                            else if (_uploadFileQuestionType.Any(type => type == answerType))
                                answerTemp = BLPChildAnswer.StudentAnswer.FilePath;
                            else
                                answerTemp = BLPChildAnswer.SurveyAnswerDesc;
                            //

                            if (string.IsNullOrEmpty(answerText))
                                answerText += answerTemp;
                            else
                                answerText += "; " + answerTemp;

                        }
                    }

                    htmlFormat = htmlFormat
                                    .Replace("{{studentAnswerDescription}}", answerText);

                    foreach (var BLPChildAnswer in BLPChildAnswerGroup.Answers)
                    {

                        if (BLPChildAnswer.StudentAnswer != null || _ignoreQuestionType.Any(x => x == answerType))
                        {
                            htmlFormat = GetChildQuestionAnswerRecursionInsert(htmlFormat, BLPChildAnswer.Childs, BLPChildAnswer.Childs?.ChildsQuestion, paddingLeft + 30);

                        }
                    }
                }
            }

            return htmlFormat;
        }

        private string QuestionAnswerFormatBuilderUpdate(List<GetBLPQuestionWithHistoryResult> BLPQuestionResultList)
        {
            string finalQuestionAnswerFormat = "";

            foreach (var BLPQuestionSectionResult in BLPQuestionResultList)
            {
                finalQuestionAnswerFormat += @"<table role='presentation' cellpadding='0' cellspacing='0' width='95%' style='margin: 0 auto; vertical-align: top;'>{{section}}</table>";

                var sectionQuestionAnswer = @"
                                <tr>
                                    <td style='padding: 10px 0px; vertical-align: top;' colspan='2'>
                                        <h4 style='margin-top: 16px;'>
                                            <b>{{sectionName}}</b>
                                        </h4>
                                
                                        <p>
                                            {{sectionDescription}}
                                        </p>
                                
                                    </td>
                                </tr>";

                sectionQuestionAnswer = sectionQuestionAnswer
                                            .Replace("{{sectionName}}", BLPQuestionSectionResult.SectionName)
                                            .Replace("{{sectionDescription}}", BLPQuestionSectionResult.Description);
                
                if (BLPQuestionSectionResult.QuestionAnswerList.Count != 0)
                {
                    sectionQuestionAnswer += @"
                                <tr>
                                    <th style='width: 40%; border: 1px solid; padding: 0 10px; vertical-align: top;'>Questions</th>
                                    <th style='width: 30%; border: 1px solid; padding: 0 10px; vertical-align: top;'>Old Answers</th>
                                    <th style='width: 30%; border: 1px solid; padding: 0 10px; vertical-align: top;'>New Answers</th>
                                </tr>";

                    int questionNumber = 0;
                    foreach (var questionAnswerList in BLPQuestionSectionResult.QuestionAnswerList)
                    {

                        if (questionAnswerList.AnswerGroup == null)
                        {
                            sectionQuestionAnswer += @"
                                <tr>
                                    <td colspan='3' style='width: 100%; border: 1px solid; padding: 0 10px; vertical-align: top;'>
                                        {{questionName}}
                                    </td>
                                </tr>";

                            sectionQuestionAnswer = sectionQuestionAnswer
                                                    .Replace("{{questionName}}", questionAnswerList.QuestionName);
                        }
                        else
                        {
                            questionNumber++;

                            sectionQuestionAnswer += @"
                                <tr>
                                    <td style='width: 40%; border: 1px solid; padding: 0 10px; vertical-align: top;'>
                                        {{questionName}}
                                    </td>
                                    <td style='width: 30%; border: 1px solid; padding: 0 10px; vertical-align: top;'>
                                        <b>{{studentOldAnswerDescription}}</b>
                                    </td>
                                    <td style='width: 30%; border: 1px solid; padding: 0 10px; vertical-align: top;'>
                                        <b>{{studentAnswerDescription}}</b>
                                    </td>
                                </tr>";

                            sectionQuestionAnswer = sectionQuestionAnswer
                                                    .Replace("{{questionName}}", questionAnswerList.QuestionName);

                            string answerText = "";
                            string oldAnswerText = "";
                            string answerType = questionAnswerList.AnswerGroup.AnswerType;

                            foreach (var answer in questionAnswerList.AnswerGroup.Answers)
                            {
                                if (answer.StudentAnswer != null)
                                {
                                    // check answer type
                                    string answerTemp = "";
                                    if (_selfFillQuestionType.Any(type => type == answerType))
                                        answerTemp = answer.StudentAnswer.Description;
                                    else if (_uploadFileQuestionType.Any(type => type == answerType))
                                        answerTemp = answer.StudentAnswer.FilePath;
                                    else
                                        answerTemp = answer.StudentAnswer.SurveyAnswerDesc;
                                    //

                                    if (string.IsNullOrEmpty(answerText))
                                        answerText += answerTemp;
                                    else
                                        answerText += "; " + answerTemp;
                                }

                                if (answer.StudentAnswerHistory != null)
                                {
                                    // check answer type
                                    string answerTemp = "";
                                    if (_selfFillQuestionType.Any(type => type == answerType))
                                        answerTemp = answer.StudentAnswerHistory.Description;
                                    else if (_uploadFileQuestionType.Any(type => type == answerType))
                                        answerTemp = answer.StudentAnswerHistory.FilePath;
                                    else
                                        answerTemp = answer.StudentAnswerHistory.SurveyAnswerDesc;
                                    //

                                    if (string.IsNullOrEmpty(oldAnswerText))
                                        oldAnswerText += answerTemp;
                                    else
                                        oldAnswerText += "; " + answerTemp;
                                }
                            }

                            sectionQuestionAnswer = sectionQuestionAnswer
                                                    .Replace("{{studentOldAnswerDescription}}", oldAnswerText)
                                                    .Replace("{{studentAnswerDescription}}", answerText);

                            foreach (var answer in questionAnswerList.AnswerGroup.Answers)
                            {
                                if (answer.StudentAnswer != null || answer.StudentAnswerHistory != null || _ignoreQuestionType.Any(x => x == answerType))
                                {
                                    sectionQuestionAnswer = GetChildQuestionAnswerRecursionUpdate(sectionQuestionAnswer, answer.Childs, answer.Childs?.ChildsQuestion, 30);
                                }
                            }
                        }
                    }

                }

                finalQuestionAnswerFormat += "<br><br>";

                finalQuestionAnswerFormat = finalQuestionAnswerFormat.Replace("{{section}}", sectionQuestionAnswer);
            }

            finalQuestionAnswerFormat += @"<hr style='color: black;'>";

            return finalQuestionAnswerFormat;
        }

        private string GetChildQuestionAnswerRecursionUpdate(string htmlFormat, BLPQuestionWithHistory_BLPAnswerVm BLPChildAnswerGroup, string childQuestion, int paddingLeft)
        {
            if (BLPChildAnswerGroup != null)
            {
                string answerText = "";
                string oldAnswerText = "";

                if (BLPChildAnswerGroup == null)
                {
                    htmlFormat += @$"<tr>
                                    <td colspan='3' style='width: 100%; border: 1px solid; padding: 0 10px; vertical-align: top; padding-left: {paddingLeft}px;'>" +
                                    @"
                                        {{questionName}}
                                    </td>
                                </tr>";

                    htmlFormat = htmlFormat
                                            .Replace("{{questionName}}", string.IsNullOrEmpty(childQuestion) ? "" : childQuestion);
                }
                else
                {
                    htmlFormat += @$"<tr>
                                    <td style='width: 40%; border: 1px solid; padding: 0 10px; vertical-align: top; padding-left: {paddingLeft}px;'>" +
                                    @"
                                        {{questionName}}
                                    </td>
                                    <td style='width: 30%; border: 1px solid; padding: 0 10px; vertical-align: top;'>
                                        <b>{{studentOldAnswerDescription}}</b>
                                    </td>
                                    <td style='width: 30%; border: 1px solid; padding: 0 10px; vertical-align: top;'>
                                        <b>{{studentAnswerDescription}}</b>
                                    </td>
                                </tr>";

                    htmlFormat = htmlFormat
                                        .Replace("{{questionName}}", string.IsNullOrEmpty(childQuestion) ? "" : childQuestion);

                    string answerType = BLPChildAnswerGroup.AnswerType;

                    foreach (var BLPChildAnswer in BLPChildAnswerGroup.Answers)
                    {

                        if (BLPChildAnswer.StudentAnswer != null)
                        {
                            // check answer type
                            string answerTemp = "";
                            if (_selfFillQuestionType.Any(type => type == answerType))
                                answerTemp = BLPChildAnswer.StudentAnswer.Description;
                            else if (_uploadFileQuestionType.Any(type => type == answerType))
                                answerTemp = BLPChildAnswer.StudentAnswer.FilePath;
                            else
                                answerTemp = BLPChildAnswer.StudentAnswer.SurveyAnswerDesc;
                            //

                            if (string.IsNullOrEmpty(answerText))
                                answerText += answerTemp;
                            else
                                answerText += "; " + answerTemp;

                        }

                        if (BLPChildAnswer.StudentAnswerHistory != null)
                        {
                            // check answer type
                            string answerTemp = "";
                            if (_selfFillQuestionType.Any(type => type == answerType))
                                answerTemp = BLPChildAnswer.StudentAnswerHistory.Description;
                            else if (_uploadFileQuestionType.Any(type => type == answerType))
                                answerTemp = BLPChildAnswer.StudentAnswerHistory.FilePath;
                            else
                                answerTemp = BLPChildAnswer.StudentAnswerHistory.SurveyAnswerDesc;
                            //

                            if (string.IsNullOrEmpty(oldAnswerText))
                                oldAnswerText += answerTemp;
                            else
                                oldAnswerText += "; " + answerTemp;

                        }
                    }

                    htmlFormat = htmlFormat
                                    .Replace("{{studentOldAnswerDescription}}", oldAnswerText)
                                    .Replace("{{studentAnswerDescription}}", answerText);

                    foreach (var BLPChildAnswer in BLPChildAnswerGroup.Answers)
                    {

                        if (BLPChildAnswer.StudentAnswer != null || BLPChildAnswer.StudentAnswerHistory != null || _ignoreQuestionType.Any(x => x == answerType))
                        {
                            htmlFormat = GetChildQuestionAnswerRecursionUpdate(htmlFormat, BLPChildAnswer.Childs, BLPChildAnswer.Childs?.ChildsQuestion, paddingLeft + 30);

                        }
                    }
                }
            }

            return htmlFormat;
        }

    }
}
