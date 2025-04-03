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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Document.BLL.FnDocument.DocumentRequest.DocumentRequestNotification.Helper;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestNotification.Validator;
using BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestNotification
{
    public class SendEmailDataUpdateRequestToParentHandler : FunctionsHttpSingleHandler
    {
        // MsNotificationTemplate - DB SCHOOL
        private readonly string _notificationScenario = "DRE07";
        private const string _separatorStartRepeaterData = "[[TableRepeaterDataStart]]";
        private const string _separatorEndRepeaterData = "[[TableRepeaterDataEnd]]";
        private const string _separatorRowSpanStartRepeaterData = "[[RowSpanStart]]";
        private const string _separatorRowSpanEndRepeaterData = "[[RowSpanEnd]]";

        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly INotificationTemplate _notificationTemplate;
        private readonly ISendGrid _sendGridEmailApi;
        private readonly IConfiguration _configuration;
        private readonly GetDocumentRequestDetailHandler _getDocumentRequestDetailHandler;

        public SendEmailDataUpdateRequestToParentHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime,
            INotificationTemplate notificationTemplate,
            ISendGrid sendGridEmailApi,
            IConfiguration configuration,
            GetDocumentRequestDetailHandler getDocumentRequestDetailHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _notificationTemplate = notificationTemplate;
            _sendGridEmailApi = sendGridEmailApi;
            _configuration = configuration;
            _getDocumentRequestDetailHandler = getDocumentRequestDetailHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SendEmailDataUpdateRequestToParentRequest, SendEmailDataUpdateRequestToParentValidator>();

            var result = await SendEmailDataUpdateRequestToParent(param);

            return Request.CreateApiResult2(result as object);
        }

        public async Task<SendEmailDataUpdateRequestToParentResult> SendEmailDataUpdateRequestToParent(SendEmailDataUpdateRequestToParentRequest param)
        {
            var hostUrl = _configuration.GetSection("ClientApp:Web:Host").Get<string>();

            if (string.IsNullOrEmpty(hostUrl))
                throw new BadRequestException("Host url is not set");

            // get email template
            var getEmailTemplateApi = await _notificationTemplate.GetNotificationTemplateScenario(new GetNotificationTemplateScenarioRequest
            {
                IdSchool = param.IdSchool,
                IdScenario = _notificationScenario
            });

            var getEmailTemplate = getEmailTemplateApi.Payload;

            if (getEmailTemplate == null)
                throw new BadRequestException("Email template is not found");

            var emailSubject = getEmailTemplate.Title;
            var emailHtmlTemplate = getEmailTemplate.Email;
            var emailRepeaterTemplate = GetRepeaterTemplate(emailHtmlTemplate, _separatorStartRepeaterData, _separatorEndRepeaterData);

            // get school data
            var schoolData = await _dbContext.Entity<MsSchool>()
                                .Where(x => x.Id == param.IdSchool)
                                .FirstOrDefaultAsync(CancellationToken);

            // get data
            var documentRequestDetail = await _getDocumentRequestDetailHandler.GetDocumentRequestDetail(new Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest.GetDocumentRequestDetailRequest
            {
                IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                IdStudent = param.IdStudent,
                IncludePaymentInfo = true
            });

            if (documentRequestDetail == null)
                throw new BadRequestException("Document request detail is not found");

            // replace subject
            emailSubject = emailSubject.Replace("{{RequestNumber}}", documentRequestDetail.RequestNumber);

            // replace data

            // get updated data
            string requestedBy = OldNewComparisonStringBuilder(param.ChangeData.ChangeDetail.OldParentApplicant.Name, param.ChangeData.ChangeDetail.NewParentApplicant.Name);

            string estimationFinishDate = documentRequestDetail.EstimationFinishDate == null ? "-" : OldNewComparisonStringBuilder(param.ChangeData.ChangeDetail.OldEstimationFinishDate.Value.ToString("dd MMMM yyyy, HH:mm:ss"), param.ChangeData.ChangeDetail.NewEstimationFinishDate.Value.ToString("dd MMMM yyyy, HH:mm:ss"));

            emailHtmlTemplate = emailHtmlTemplate
                                .Replace("{{StudentID}}", documentRequestDetail.Student.Id)
                                .Replace("{{StudentName}}", documentRequestDetail.Student.Name)
                                .Replace("{{RequestNumber}}", documentRequestDetail.RequestNumber)
                                .Replace("{{Homeroom}}", string.IsNullOrEmpty(documentRequestDetail.HomeroomWhenRequestWasMade?.Description) ? "-" : documentRequestDetail.HomeroomWhenRequestWasMade.Description)
                                .Replace("{{CurrentStudentStatus}}", documentRequestDetail.StudentStatusWhenRequestWasCreated == null ? "-" : documentRequestDetail.StudentStatusWhenRequestWasCreated.Description + " (" + (documentRequestDetail.StudentStatusWhenRequestWasCreated.StartDate == null ? "-" : documentRequestDetail.StudentStatusWhenRequestWasCreated.StartDate.Value.ToString("dd MMMM yyyy")) + ")")

                                .Replace("{{RequestedBy}}", requestedBy)

                                .Replace("{{CreatedBy}}", documentRequestDetail.CreatedBy)
                                .Replace("{{RequestDate}}", documentRequestDetail.RequestDate.ToString("dd MMMM yyyy, HH:mm:ss"))

                                .Replace("{{EstimationFinishDate}}", estimationFinishDate)

                                .Replace("{{UpdatedBy}}", param.ChangeData.UpdatedBy.Id + " - " + param.ChangeData.UpdatedBy.Name)
                                .Replace("{{UpdateDate}}", param.ChangeData.UpdateDate.ToString("dd MMMM yyyy, HH:mm:ss"))
                                .Replace("{{RequestStatus}}", documentRequestDetail.LatestDocumentReqStatusWorkflow.Description)
                                .Replace("{{StatusRemarks}}", string.IsNullOrEmpty(documentRequestDetail.LatestDocumentReqStatusWorkflow.Remarks) ? "-" : documentRequestDetail.LatestDocumentReqStatusWorkflow.Remarks)

                                .Replace("{{RedirectLink}}", hostUrl.Trim() + "/DocumentRequestParent/DocumentRequestHistory")
                                .Replace("{{SchoolName}}", schoolData.Name);

            // replate table repeater data
            var rowNo = 1;
            var emailRepeaterResult = "";
            foreach (var document in documentRequestDetail.DocumentList)
            {
                var emailRepeaterTemplateTemp = "";

                // get updated data
                var updatedDataDocumentRequest = param.ChangeData.ChangeDetail.DocumentRequestList
                                        .Where(x => x.IdDocumentReqApplicantDetail == document.IdDocumentReqApplicantDetail)
                                        .FirstOrDefault();

                string periodDocument = document.PeriodDocument == null ? "-" :
                                        OldNewComparisonStringBuilder(updatedDataDocumentRequest.OldPeriodDocument.Description, updatedDataDocumentRequest.NewPeriodDocument.Description);

                var newPICList = document.PICList.OrderBy(x => x.Id).ToList();

                var oldPICList = updatedDataDocumentRequest.OldBinusianPICList
                                    .Select(x => x)
                                    .OrderBy(x => x.Id)
                                    .ToList();

                string binusianPICList = OldNewComparisonStringBuilder(oldPICList.Count == 1 ? oldPICList.Select(x => x.Name + " (" + x.Id + ")").FirstOrDefault().ToString() : string.Join("<br>", oldPICList.Select(x => "- " + x.Name + " (" + x.Id + ")")),
                    (newPICList.Count == 1 ? newPICList.Select(x => x.Name + " (" + x.Id + ")").FirstOrDefault().ToString() : string.Join("<br>", newPICList.Select(x => "- " + x.Name + " (" + x.Id + ")")))
                    );

                if (document.AdditionalFieldList == null || !document.AdditionalFieldList.Any())
                {
                    emailRepeaterTemplateTemp = emailRepeaterTemplate;

                    emailRepeaterTemplateTemp = emailRepeaterTemplateTemp
                                            .Replace("{{RowNo}}", rowNo.ToString())
                                            .Replace("{{DocumentName}}", document.DocumentName)
                                            .Replace("{{IsAcademicDocument}}", document.IsAcademicDocument ? "Yes" : "No")
                                            .Replace("{{AcademicYearGrade}}", document.AcademicYearDocument == null ? "-" : document.AcademicYearDocument.Description + " (" + document.GradeDocument.Description + ")")
                                            .Replace("{{Term}}", periodDocument)
                                            .Replace("{{Price}}", string.Format("{0:N}", document.PriceInvoice))
                                            .Replace("{{NeedHardCopy}}", document.NeedHardCopy ? "Yes" : "No")
                                            .Replace("{{NeedSoftCopy}}", document.NeedSoftCopy ? "Yes" : "No")
                                            .Replace("{{NoOfPages}}", document.NoOfPages == null ? "-" : document.NoOfPages.ToString())
                                            .Replace("{{NoOfCopies}}", document.NoOfCopy.ToString())
                                            .Replace("{{PIC}}", binusianPICList)
                                            .Replace("{{RowSpan}}", 1.ToString())
                                            .Replace("{{AdditionalFieldName}}", "-")
                                            .Replace("{{AdditionalFieldAnswer}}", "-");

                    // remove rowspan tag
                    emailRepeaterTemplateTemp = RemoveTagFromHtmlTemplate(emailRepeaterTemplateTemp, _separatorRowSpanStartRepeaterData);
                    emailRepeaterTemplateTemp = RemoveTagFromHtmlTemplate(emailRepeaterTemplateTemp, _separatorRowSpanEndRepeaterData);

                    emailRepeaterResult += emailRepeaterTemplateTemp;
                }
                else
                {
                    var totalAdditionalFieldsRow = document.AdditionalFieldList.Count();
                    var countAdditionalField = 0;

                    foreach (var additionalField in document.AdditionalFieldList)
                    {
                        // get updated data
                        var oldAnswerTextList = updatedDataDocumentRequest
                                                    .AdditionalFieldsList
                                                    .Where(x => x.IdDocumentReqFormFieldAnswered == additionalField.IdDocumentReqFormFieldAnswered)
                                                    .SelectMany(x => x.OldTextValueList)
                                                    .OrderBy(x => x)
                                                    .ToList();


                        var newAnswerTextList = updatedDataDocumentRequest
                                                    .AdditionalFieldsList
                                                    .Where(x => x.IdDocumentReqFormFieldAnswered == additionalField.IdDocumentReqFormFieldAnswered)
                                                    .SelectMany(x => x.NewTextValueList)
                                                    .OrderBy(x => x)
                                                    .ToList();

                        var answerTextList = OldNewComparisonStringBuilder((oldAnswerTextList.Count == 1 ? string.Join("<br>", oldAnswerTextList.Select(x => x)) : string.Join("<br>", oldAnswerTextList.Select(x => "- " + x))),
                            (additionalField.AnswerTextList.Count == 1 ? string.Join("<br>", additionalField.AnswerTextList.Select(x => x)) : string.Join("<br>", additionalField.AnswerTextList.Select(x => "- " + x))));

                        if (countAdditionalField == 0)
                        {
                            emailRepeaterTemplateTemp = emailRepeaterTemplate;

                            emailRepeaterTemplateTemp = emailRepeaterTemplateTemp
                                                    .Replace("{{RowNo}}", rowNo.ToString())
                                                    .Replace("{{DocumentName}}", document.DocumentName)
                                                    .Replace("{{IsAcademicDocument}}", document.IsAcademicDocument ? "Yes" : "No")
                                                    .Replace("{{AcademicYearGrade}}", document.AcademicYearDocument == null ? "-" : document.AcademicYearDocument.Description + " (" + document.GradeDocument.Description + ")")
                                                    .Replace("{{Term}}", periodDocument)
                                                    .Replace("{{Price}}", string.Format("{0:N}", document.PriceInvoice))
                                                    .Replace("{{NeedHardCopy}}", document.NeedHardCopy ? "Yes" : "No")
                                                    .Replace("{{NeedSoftCopy}}", document.NeedSoftCopy ? "Yes" : "No")
                                                    .Replace("{{NoOfPages}}", document.NoOfPages == null ? "-" : document.NoOfPages.ToString())
                                                    .Replace("{{NoOfCopies}}", document.NoOfCopy.ToString())
                                                    .Replace("{{PIC}}", binusianPICList)
                                                    .Replace("{{RowSpan}}", totalAdditionalFieldsRow.ToString())
                                                    .Replace("{{AdditionalFieldName}}", additionalField.QuestionDescription)
                                                    .Replace("{{AdditionalFieldAnswer}}", answerTextList);
                        }
                        else
                        {
                            // remove  rowspan template
                            emailRepeaterTemplateTemp = InjectRepeaterDataToTemplate(emailRepeaterTemplate, "", _separatorRowSpanStartRepeaterData, _separatorRowSpanEndRepeaterData);

                            emailRepeaterTemplateTemp = emailRepeaterTemplateTemp
                                                    .Replace("{{AdditionalFieldName}}", additionalField.QuestionDescription)
                                                    .Replace("{{AdditionalFieldAnswer}}", answerTextList);
                        }

                        // remove rowspan tag
                        emailRepeaterTemplateTemp = RemoveTagFromHtmlTemplate(emailRepeaterTemplateTemp, _separatorRowSpanStartRepeaterData);
                        emailRepeaterTemplateTemp = RemoveTagFromHtmlTemplate(emailRepeaterTemplateTemp, _separatorRowSpanEndRepeaterData);

                        emailRepeaterResult += emailRepeaterTemplateTemp;
                        countAdditionalField++;
                    }
                }
                rowNo++;
            }

            // inject repeater data to main email template
            emailHtmlTemplate = InjectRepeaterDataToTemplate(emailHtmlTemplate, emailRepeaterResult, _separatorStartRepeaterData, _separatorEndRepeaterData);

            // Fill Additional Notes
            var additionalNotesHtml = "";

            emailHtmlTemplate = emailHtmlTemplate
                                    .Replace("{{AdditionalNotes}}", string.IsNullOrWhiteSpace(additionalNotesHtml) ? "-" : additionalNotesHtml);

            // get Approver email
            var approverDataList = await _dbContext.Entity<MsDocumentReqApprover>()
                                        .Include(x => x.Staff)
                                        .Where(x => x.IdSchool == param.IdSchool)
                                        .ToListAsync(CancellationToken);

            if (approverDataList == null || !approverDataList.Any())
                throw new BadRequestException("Document request approver is not found");

            var studentParentDataList = await _dbContext.Entity<MsStudentParent>()
                                            .Include(x => x.Parent)
                                            .Include(x => x.Student)
                                            .Where(x => x.IdStudent == documentRequestDetail.Student.Id)
                                            .ToListAsync(CancellationToken);

            if (studentParentDataList == null || !studentParentDataList.Any())
                throw new BadRequestException("Student or parent data is not found");


            var studentData = studentParentDataList
                                .Select(x => x.Student)
                                .FirstOrDefault();

            var sendSendGridEmailApi = await _sendGridEmailApi.SendSendGridEmail(new SendSendGridEmailRequest
            {
                IdSchool = param.IdSchool,
                MessageContent = new SendSendGridEmailRequest_MessageContent
                {
                    Subject = emailSubject,
                    BodyHtml = emailHtmlTemplate
                },
                RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration
                {
                    ToList = studentParentDataList
                                .Select(x => new SendSendGridEmailRequest_AddressBuilder
                                {
                                    DisplayName = NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName),
                                    Address = x.Parent.PersonalEmailAddress
                                })
                                .ToList(),
                    CcList = !LevelCodeChecker.IsHighSchool(documentRequestDetail?.LevelWhenRequestWasMade?.Code) ? null : new List<SendSendGridEmailRequest_AddressBuilder>() { new SendSendGridEmailRequest_AddressBuilder
                    {
                        DisplayName = NameUtil.GenerateFullName(studentData.FirstName, studentData.MiddleName, studentData.LastName),
                        Address = studentData.BinusianEmailAddress
                    } },
                    BccList = approverDataList
                                .Select(x => new SendSendGridEmailRequest_AddressBuilder
                                {
                                    DisplayName = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName),
                                    Address = x.Staff.BinusianEmailAddress
                                })
                                .ToList()
                }
            });

            var result = new SendEmailDataUpdateRequestToParentResult();
            if (sendSendGridEmailApi.Payload.IsSuccess)
                result.IsSuccess = true;
            else
            {
                result.IsSuccess = false;
                result.ErrorMessage = sendSendGridEmailApi.Payload.ErrorMessage;
            }

            return result;
        }

        private string GetRepeaterTemplate(string rawHtmlTemplate, string separatorStartRepeaterData, string separatorEndRepeaterData)
        {
            int indexFrom = rawHtmlTemplate.IndexOf(separatorStartRepeaterData) + separatorStartRepeaterData.Length;
            int indexTo = rawHtmlTemplate.IndexOf(separatorEndRepeaterData);

            var rowDataTemplate = rawHtmlTemplate.Substring(indexFrom, indexTo - indexFrom);

            return rowDataTemplate;
        }

        private string InjectRepeaterDataToTemplate(string rawHtmlTemplate, string repeaterHtmlData, string separatorStartRepeaterData, string separatorEndRepeaterData)
        {
            string resultHtml = rawHtmlTemplate;
            int indexFrom = resultHtml.IndexOf(separatorStartRepeaterData) + separatorStartRepeaterData.Length;
            int indexTo = rawHtmlTemplate.IndexOf(separatorEndRepeaterData);

            // remove repeater template
            resultHtml = resultHtml.Remove(indexFrom, indexTo - indexFrom);

            resultHtml = resultHtml.Insert(indexFrom, repeaterHtmlData);

            // remove repeater tag
            resultHtml = resultHtml
                                .Replace(separatorStartRepeaterData, "")
                                .Replace(separatorEndRepeaterData, "");

            return resultHtml;
        }

        private string RemoveTagFromHtmlTemplate(string rawHtmlTemplate, string tag)
        {
            string resultHtml = rawHtmlTemplate;

            // remove repeater tag
            resultHtml = resultHtml
                                .Replace(tag, "");

            return resultHtml;
        }

        private string OldNewComparisonStringBuilder(string oldValue, string newValue)
        {
            if (oldValue == newValue)
                return newValue;

            return string.Format("<span style='color: red;'><s>{0}</s></span><br><span style='color: #00ffff;'>{1}</span>", oldValue, newValue);
        }
    }
}
