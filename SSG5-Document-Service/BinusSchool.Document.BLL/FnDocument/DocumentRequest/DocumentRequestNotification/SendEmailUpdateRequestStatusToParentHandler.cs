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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Document.FnDocument;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
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
    public class SendEmailUpdateRequestStatusToParentHandler : FunctionsHttpSingleHandler
    {
        // MsNotificationTemplate - DB SCHOOL
        private readonly string _notificationScenario = "DRE05";
        private const string _separatorStartRepeaterData = "[[TableRepeaterDataStart]]";
        private const string _separatorEndRepeaterData = "[[TableRepeaterDataEnd]]";
        private const string _separatorRowSpanStartRepeaterData = "[[RowSpanStart]]";
        private const string _separatorRowSpanEndRepeaterData = "[[RowSpanEnd]]";

        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly INotificationTemplate _notificationTemplate;
        private readonly ISendGrid _sendGridEmailApi;
        private readonly IConfiguration _configuration;
        private readonly IDocumentRequestPayment _documentRequestPaymentApi;
        private readonly GetDocumentRequestDetailHandler _getDocumentRequestDetailHandler;

        public SendEmailUpdateRequestStatusToParentHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime,
            INotificationTemplate notificationTemplate,
            ISendGrid sendGridEmailApi,
            IConfiguration configuration,
            IDocumentRequestPayment documentRequestPaymentApi,
            GetDocumentRequestDetailHandler getDocumentRequestDetailHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _notificationTemplate = notificationTemplate;
            _sendGridEmailApi = sendGridEmailApi;
            _configuration = configuration; 
            _documentRequestPaymentApi = documentRequestPaymentApi;
            _getDocumentRequestDetailHandler = getDocumentRequestDetailHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SendEmailUpdateRequestStatusToParentRequest, SendEmailUpdateRequestStatusToParentValidator>();

            var result = await SendEmailUpdateRequestStatusToParent(param);

            return Request.CreateApiResult2(result as object);
        }

        public async Task<SendEmailUpdateRequestStatusToParentResult> SendEmailUpdateRequestStatusToParent(SendEmailUpdateRequestStatusToParentRequest param)
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
            emailSubject = emailSubject
                            .Replace("{{RequestNumber}}", documentRequestDetail.RequestNumber)
                            .Replace("{{DocumentRequestStatusWorkflow}}", documentRequestDetail.LatestDocumentReqStatusWorkflow.Description);

            // replace data
            emailHtmlTemplate = emailHtmlTemplate
                                .Replace("{{StudentID}}", documentRequestDetail.Student.Id)
                                .Replace("{{StudentName}}", documentRequestDetail.Student.Name)
                                .Replace("{{RequestNumber}}", documentRequestDetail.RequestNumber)
                                .Replace("{{Homeroom}}", string.IsNullOrEmpty(documentRequestDetail.HomeroomWhenRequestWasMade?.Description) ? "-" : documentRequestDetail.HomeroomWhenRequestWasMade.Description)
                                .Replace("{{RequestedBy}}", documentRequestDetail.RequestedBy.Description)
                                .Replace("{{CreatedBy}}", documentRequestDetail.CreatedBy)
                                .Replace("{{RequestDate}}", documentRequestDetail.RequestDate.ToString("dd MMMM yyyy, HH:mm:ss"))
                                .Replace("{{EstimationFinishDate}}", documentRequestDetail.EstimationFinishDate == null ? "-" : documentRequestDetail.EstimationFinishDate.Value.ToString("dd MMMM yyyy, HH:mm:ss"))
                                .Replace("{{CurrentStudentStatus}}", documentRequestDetail.StudentStatusWhenRequestWasCreated == null ? "-" : documentRequestDetail.StudentStatusWhenRequestWasCreated.Description + " (" + (documentRequestDetail.StudentStatusWhenRequestWasCreated.StartDate == null ? "-" : documentRequestDetail.StudentStatusWhenRequestWasCreated.StartDate.Value.ToString("dd MMMM yyyy")) + ")")
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

                if (document.AdditionalFieldList == null || !document.AdditionalFieldList.Any())
                {
                    emailRepeaterTemplateTemp = emailRepeaterTemplate;

                    emailRepeaterTemplateTemp = emailRepeaterTemplateTemp
                                            .Replace("{{RowNo}}", rowNo.ToString())
                                            .Replace("{{DocumentName}}", document.DocumentName)
                                            .Replace("{{IsAcademicDocument}}", document.IsAcademicDocument ? "Yes" : "No")
                                            .Replace("{{AcademicYearGrade}}", document.AcademicYearDocument == null ? "-" : document.AcademicYearDocument.Description + " (" + document.GradeDocument.Description + ")")
                                            .Replace("{{Term}}", document.PeriodDocument == null ? "-" : document.PeriodDocument.Description)
                                            .Replace("{{Price}}", string.Format("{0:N}", document.PriceInvoice))
                                            .Replace("{{NeedHardCopy}}", document.NeedHardCopy ? "Yes" : "No")
                                            .Replace("{{NeedSoftCopy}}", document.NeedSoftCopy ? "Yes" : "No")
                                            .Replace("{{NoOfPages}}", document.NoOfPages == null ? "-" : document.NoOfPages.ToString())
                                            .Replace("{{NoOfCopies}}", document.NoOfCopy.ToString())
                                            .Replace("{{PIC}}", document.PICList.Count == 1 ? document.PICList.Select(x => x.Name + " (" + x.Id + ")").FirstOrDefault().ToString() : string.Join("<br>", document.PICList.Select(x => "- " + x.Name + " (" + x.Id + ")")))
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
                        if (countAdditionalField == 0)
                        {
                            emailRepeaterTemplateTemp = emailRepeaterTemplate;

                            emailRepeaterTemplateTemp = emailRepeaterTemplateTemp
                                                    .Replace("{{RowNo}}", rowNo.ToString())
                                                    .Replace("{{DocumentName}}", document.DocumentName)
                                                    .Replace("{{IsAcademicDocument}}", document.IsAcademicDocument ? "Yes" : "No")
                                                    .Replace("{{AcademicYearGrade}}", document.AcademicYearDocument == null ? "-" : document.AcademicYearDocument.Description + " (" + document.GradeDocument.Description + ")")
                                                    .Replace("{{Term}}", document.PeriodDocument == null ? "-" : document.PeriodDocument.Description)
                                                    .Replace("{{Price}}", string.Format("{0:N}", document.PriceInvoice))
                                                    .Replace("{{NeedHardCopy}}", document.NeedHardCopy ? "Yes" : "No")
                                                    .Replace("{{NeedSoftCopy}}", document.NeedSoftCopy ? "Yes" : "No")
                                                    .Replace("{{NoOfPages}}", document.NoOfPages == null ? "-" : document.NoOfPages.ToString())
                                                    .Replace("{{NoOfCopies}}", document.NoOfCopy.ToString())
                                                    .Replace("{{PIC}}", document.PICList.Count == 1 ? document.PICList.Select(x => x.Name + " (" + x.Id + ")").FirstOrDefault().ToString() : string.Join("<br>", document.PICList.Select(x => "- " + x.Name + " (" + x.Id + ")")))
                                                    .Replace("{{RowSpan}}", totalAdditionalFieldsRow.ToString())
                                                    .Replace("{{AdditionalFieldName}}", additionalField.QuestionDescription)
                                                    .Replace("{{AdditionalFieldAnswer}}", additionalField.AnswerTextList.Count == 1 ? string.Join("<br>", additionalField.AnswerTextList.Select(x => x)) : string.Join("<br>", additionalField.AnswerTextList.Select(x => "- " + x)));
                        }
                        else
                        {
                            // remove  rowspan template
                            emailRepeaterTemplateTemp = InjectRepeaterDataToTemplate(emailRepeaterTemplate, "", _separatorRowSpanStartRepeaterData, _separatorRowSpanEndRepeaterData);

                            emailRepeaterTemplateTemp = emailRepeaterTemplateTemp
                                                    .Replace("{{AdditionalFieldName}}", additionalField.QuestionDescription)
                                                    .Replace("{{AdditionalFieldAnswer}}", additionalField.AnswerTextList.Count == 1 ? string.Join("<br>", additionalField.AnswerTextList.Select(x => x)) : string.Join("<br>", additionalField.AnswerTextList.Select(x => "- " + x)));
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

            if (documentRequestDetail.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPayment)
            {
                additionalNotesHtml += "The status of your document request is <b>Waiting for Payment</b>, please complete your payment with detail below:<br>";

                additionalNotesHtml += @"<table style='width: 100%; margin-bottom: 30px; table-layout: fixed; border: 3px solid red;'>
    <tr>
        <td style='vertical-align:top;'><b>Payment Due Date</b></td>
        <td style='width: 1%; vertical-align:top;'><b>:</b></td>
        <td><b>{{PaymentDueDate}}</b></td>
    </tr>
    <tr>
        <td style='vertical-align:top;'><b>Invoice Total Amount (Rp)</b></td>
        <td style='vertical-align:top;'><b>:</b></td>
        <td><b>{{InvoiceTotalAmount}}</b></td>
    </tr>
    <tr>
        <td style='vertical-align:top;'><b>Payment Instruction</b></td>
        <td style='vertical-align:top;'><b>:</b></td>
        <td>{{PaymentInstruction}}</td>
    </tr>
</table>";

                string encryptedPaymentConfirmationUrl = hostUrl.Trim() + "/DocumentRequestParent/DocumentRequestPaymentConfirmation?id=" + EncryptStringUtil.Encrypt(param.IdDocumentReqApplicant);

                additionalNotesHtml = additionalNotesHtml
                                        .Replace("{{PaymentDueDate}}", documentRequestDetail.Payment.EndDatePayment.Value.ToString("dd MMMM yyyy, HH:mm:ss"))
                                        .Replace("{{InvoiceTotalAmount}}", string.Format("{0:N}", documentRequestDetail.Payment.TotalAmountInvoice));

                if (documentRequestDetail.Payment.DocumentReqPaymentMethod.UsingManualVerification.HasValue ? documentRequestDetail.Payment.DocumentReqPaymentMethod.UsingManualVerification.Value : false)
                {
                    var getPaymentMethodDetailApi = await _documentRequestPaymentApi.GetPaymentMethodBySchool(new GetPaymentMethodBySchoolRequest
                    {
                        IdSchool = param.IdSchool,
                        IdDocumentReqPaymentMethod = documentRequestDetail.Payment.DocumentReqPaymentMethod.Id,
                        Return = CollectionType.Pagination,
                        GetAll = true
                    });

                    var getPaymentMethodDetail = getPaymentMethodDetailApi.Payload?.FirstOrDefault();

                    if (getPaymentMethodDetail == null)
                    {
                        additionalNotesHtml = additionalNotesHtml
                                            .Replace("{{PaymentInstruction}}", $"Please visit this <a href='{encryptedPaymentConfirmationUrl}'><b>payment confirmation link</b></a> for payment detail.");
                    }
                    else
                    {
                        additionalNotesHtml = additionalNotesHtml
                                            .Replace("{{PaymentInstruction}}", $"<p>{getPaymentMethodDetail.DescriptionHTML}</p><img src='{getPaymentMethodDetail.ImageUrl}' alt='' style='max-width: 300px;'><br><br>Please visit this <a href='{encryptedPaymentConfirmationUrl}'><b>payment confirmation link</b></a> for payment detail.");
                    }
                }
            }

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

            var result = new SendEmailUpdateRequestStatusToParentResult();
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
    }
}
