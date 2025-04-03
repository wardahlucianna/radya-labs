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
using BinusSchool.Data.Api.Document.FnDocument;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Model.Document.FnDocument.BlendedLearningProgram;
using BinusSchool.Data.Model.Document.FnDocument.ClearanceForm;
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
    public class SendEmailClearanceFormForParentHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly ISendGrid _sendGridApi;
        private readonly IBlendedLearningProgram _blendedLearningProgramApi;
        private readonly IMachineDateTime _dateTime;
        private IDbContextTransaction _transaction;

        public SendEmailClearanceFormForParentHandler(
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

        #region unused code
        //        private string _emailTemplate = @"
        //<head>
        //    <title></title>
        //</head>
        //<body>
        //    <table role='presentation' cellpadding='0' cellspacing='0' width='100%'>
        //        <tr>
        //            <td>
        //                <table role='presentation' cellpadding='0' cellspacing='0' width='600px' style='margin: 0 auto;'>
        //                    <tr>
        //                        <td style='padding: 0 30px 30px 30px;'>
        //                            <table role='presentation' cellpadding='0' cellspacing='0' width='100%' style='margin: 0 auto;'>
        //                                <tr>
        //                                    <td>
        //                                        <p>Dear Parent of <b>{{StudentName}}</b>,</p>
        //                                    </td>
        //                                </tr>
        //                                <tr>
        //                                    <td>
        //                                        <p>
        //                                            Student Name: <b>{{StudentName}}</b><br>
        //                                            Student ID: <b>{{StudentId}}</b><br>
        //                                            Class: <b>{{HomeroomClass}}</b><br>
        //                                            Academic Year: <b>{{AcademicYear}}</b><br>
        //                                        </p>
        //                                        <hr style='color: black;'>
        //                                        <p>
        //                                            Submitted by: <b>{{ParentName}}</b><br>
        //                                            Submission date: <b>{{SubmissionDate}}</b><br>
        //                                        </p>
        //                                        <hr style='color: black;'>
        //                                    </td>
        //                                </tr>
        //                                <tr>
        //                                   <td>
        //                                        [[StatusDescription]]
        //                                        <hr style='color: black;'>
        //                                   </td>
        //                                </tr>
        //                            </table>
        //                        </td>
        //                    </tr>

        //                    <tr>
        //                        <td style='padding: 0 30px 30px 30px;'>
        //                            {{SurveyQuestionAnswer}}
        //                        </td>
        //                    </tr>

        //                    <tr>
        //                        <td style='padding: 0 30px 30px 30px;'>
        //                            [[BarcodeCheckIn]]
        //                        </td>
        //                    </tr>

        //                    <tr>
        //                        <td>
        //                            <!-- FOOTER -->
        //                            [[Footer]]
        //                        </td>
        //                    </tr>

        //                </table>
        //            </td>
        //        </tr>
        //    </table>
        //</body>
        //        ";
        #endregion

        // private readonly string _emailErrorTo = "danis.prasetyanto@binus.edu";//"austin.tanujaya@binus.edu";
        // private readonly string _emailErrorCC = "danis.prasetyanto@binus.edu";//"austin.tanujaya@binus.edu";
        private readonly string _emailErrorTo = "itdevschool@binus.edu";
        private readonly string _emailErrorCC = "itdevschool@binus.edu";

        private string _emailTemplate = "";
        private readonly string[] _selfFillQuestionType = new string[] { "text", "textarea", "date" };
        private readonly string[] _uploadFileQuestionType = new string[] { "file" };
        private readonly string[] _ignoreQuestionType = new string[] { "list" };

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SendEmailClearanceFormForParentRequest, SendEmailClearanceFormForParentValidator>();

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                #region unused code
                //var GetBLPFinalStatusAllStudents = _clearanceForm.GetAllStudentStatusClearanceForm(new GetAllStudentStatusClearanceFormRequest { IdParent = param.StudentSurveyData.Parent.Id });

                //var GetBLPFinalStatusAllStudentsPayload = GetBLPFinalStatusAllStudents.Result.Payload;

                //var GetBLPFinalStatusAllStudentsList = GetBLPFinalStatusAllStudentsPayload.ListChild;

                //if (GetBLPFinalStatusAllStudentsPayload.IsAnySubmitted == false || GetBLPFinalStatusAllStudentsList.Count == 0)
                //{
                //    throw new BadRequestException("Failed to send email (ERR-1)");
                //}

                //var studentBLPFinalStatus = GetBLPFinalStatusAllStudentsList.Where(x => x.Student.Id == param.StudentSurveyData.Student.Id).FirstOrDefault();

                //if(studentBLPFinalStatus == null || studentBLPFinalStatus.IsSubmitted == false)
                //{
                //    throw new BadRequestException("Failed to send email (ERR-2)");
                //}
                #endregion

                var BLPEmail = _dbContext.Entity<MsBLPEmail>()
                                .Include(bem => bem.BLPSetting)
                                .Include(bem => bem.RoleGroup)
                                .Where(x => x.BLPSetting.IdSchool == param.IdSchool &&
                                            x.IdSurveyCategory == "2" &&    // clearance form
                                            x.BLPFinalStatus == param.BLPFinalStatus &&
                                            x.RoleGroup.Code.ToUpper().Trim() == "PARENT")
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

                var BLPQuestionAnswerList = new List<GetBLPQuestionResult>();

                if (BLPEmail.ShowSurveyAnswer)
                {
                    // Get BLP Question and Answer
                    var BLPQuestionAnswer = _blendedLearningProgramApi.GetBLPQuestion(new GetBLPQuestionRequest
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
                var buildTemplate = new SendEmailClearanceFormForParentRequest_BuildTemplate()
                {
                    StatusDescription = BLPEmail?.StatusDescription,
                    IsUsingBarcodeCheckIn = (bool)BLPEmail?.BLPSetting.NeedBarcode ? true : false,
                    Footer = BLPEmail?.BLPSetting.FooterEmail
                };
                FillEmailTemplate(buildTemplate);

                // Fill Email Data
                var buildData = new SendEmailClearanceFormForParentRequest_BuildData()
                {
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
                FillEmailData(buildData);

                var toList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var ccList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var bccList = new List<SendSendGridEmailRequest_AddressBuilder>();

                // main receiver
                var to = new SendSendGridEmailRequest_AddressBuilder()
                {
                    Address = param.StudentSurveyData.ParentEmail,
                    DisplayName = param.StudentSurveyData.Parent.Name
                    //Address = "austin.tanujaya@binus.edu"
                };

                toList.Add(to);

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

                if (sendEmail.Payload.IsSuccess == false)
                {
                    throw new BadRequestException("Failed to send email (ERR-4)");
                }

                var saveLog = new TrBLPEmailSentLog()
                {
                    Id = Guid.NewGuid().ToString(),
                    IdStudent = param.StudentSurveyData.Student.Id,
                    IdSurveyPeriod = param.IdSurveyPeriod,
                    IdClearanceWeekPeriod = param.IdClearanceWeekPeriod,
                    EmailSubject = BLPEmail.EmailSubject?.Trim(),
                    HTMLDescription = _emailTemplate,
                    PrimaryToAddress = param.StudentSurveyData.ParentEmail,
                    AdditionalToAddress = additionalTo,
                    AdditionalCCAddress = additionalCC,
                    AdditionalBCCAddress = additionalBCC,
                    ResendDate = null
                };

                _dbContext.Entity<TrBLPEmailSentLog>().Add(saveLog);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();

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
                        Subject = "Error Send Email Clearance For Parent",
                        BodyHtml = "SendEmailClearanceFormForParentHandler\n\n" + ex.ToString()
                    }
                });

                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction.Dispose();
            }
        }

        private void FillEmailTemplate(SendEmailClearanceFormForParentRequest_BuildTemplate buildTemplate)
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

        private void FillEmailData(SendEmailClearanceFormForParentRequest_BuildData buildData)
        {
            var questionAnswerBuilder = buildData.BLPQuestionResultList == null || buildData.BLPQuestionResultList?.Count == 0 ? "" : QuestionAnswerFormatBuilder(buildData.BLPQuestionResultList);

            _emailTemplate = _emailTemplate
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

        private string QuestionAnswerFormatBuilder(List<GetBLPQuestionResult> BLPQuestionResultList)
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
                                    <th style='width: 60%; border: 1px solid; padding: 0 10px; vertical-align: top;'>Questions</th>
                                    <th style='width: 40%; border: 1px solid; padding: 0 10px; vertical-align: top;'>Answers</th>
                                </tr>";

                    int questionNumber = 0;
                    foreach (var questionAnswerList in BLPQuestionSectionResult.QuestionAnswerList)
                    {

                        if(questionAnswerList.AnswerGroup == null)
                        {
                            sectionQuestionAnswer += @"
                                <tr>
                                    <td colspan='2' style='width: 100%; border: 1px solid; padding: 0 10px; vertical-align: top;'>
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
                                    sectionQuestionAnswer = GetChildQuestionAnswerRecursion(sectionQuestionAnswer, answer.Childs, answer.Childs?.ChildsQuestion, 30);
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

        private string GetChildQuestionAnswerRecursion(string htmlFormat, BLPAnswerVm BLPChildAnswerGroup, string childQuestion, int paddingLeft)
        {
            if (BLPChildAnswerGroup != null)
            {
                string answerText = "";

                if (BLPChildAnswerGroup == null)
                {
                    htmlFormat += @$"<tr>
                                    <td colspan='2' style='width: 60%; border: 1px solid; padding: 0 10px; vertical-align: top; padding-left: {paddingLeft}px;'>" +
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
                                    <td style='width: 60%; border: 1px solid; padding: 0 10px; vertical-align: top; padding-left: {paddingLeft}px;'>" +
                                    @"
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
                            htmlFormat = GetChildQuestionAnswerRecursion(htmlFormat, BLPChildAnswer.Childs, BLPChildAnswer.Childs?.ChildsQuestion, paddingLeft + 30);

                        }
                    }
                }
            }

            return htmlFormat;
        }
    }
}
