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
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestNotification.Validator;
using BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestNotification
{
    public class SendEmailCancelRequestToStaffHandler : FunctionsHttpSingleHandler
    {
        // MsNotificationTemplate - DB SCHOOL
        private readonly string _notificationScenario = "DRE02S";
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

        public SendEmailCancelRequestToStaffHandler(
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
            var param = await Request.ValidateBody<SendEmailCancelRequestToStaffRequest, SendEmailCancelRequestToStaffValidator>();

            var result = await SendEmailCancelRequestToStaff(param);

            return Request.CreateApiResult2(result as object);
        }

        public async Task<SendEmailCancelRequestToStaffResult> SendEmailCancelRequestToStaff(SendEmailCancelRequestToStaffRequest param)
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

            // get request document workflow
            var getLatestRequestDocumentStatus = await _dbContext.Entity<TrDocumentReqStatusTrackingHistory>()
                                                    .Where(x => x.IdDocumentReqApplicant == param.IdDocumentReqApplicant)
                                                    .OrderByDescending(x => x.StatusDate)
                                                    .FirstOrDefaultAsync(CancellationToken);

            if (getLatestRequestDocumentStatus == null)
                throw new BadRequestException("Document request status workflow is not found");

            var getStaffCancelationActor = await _dbContext.Entity<MsStaff>()
                                            .Where(x => x.IdBinusian == getLatestRequestDocumentStatus.IdBinusianStaff)
                                            .Select(x => new
                                            {
                                                IdBinusian = x.IdBinusian,
                                                Name = NameUtil.GenerateFullName(x.FirstName, x.LastName)
                                            })
                                            .FirstOrDefaultAsync(CancellationToken);


            // replace subject
            emailSubject = emailSubject.Replace("{{RequestNumber}}", documentRequestDetail.RequestNumber);

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

                                .Replace("{{CanceledBy}}", getStaffCancelationActor == null ? "Parent" : string.Format("{0} - {1} (Staff)", getStaffCancelationActor.IdBinusian, getStaffCancelationActor.Name))
                                .Replace("{{CancelationDate}}", documentRequestDetail.LatestDocumentReqStatusWorkflow.StatusDate.ToString("dd MMMM yyyy, HH:mm:ss"))
                                .Replace("{{CancelationRemarks}}", string.IsNullOrWhiteSpace(documentRequestDetail.LatestDocumentReqStatusWorkflow.Remarks) ? "-" : documentRequestDetail.LatestDocumentReqStatusWorkflow.Remarks)

                                .Replace("{{RedirectLink}}", hostUrl.Trim() + "/DocumentRequest/MasterDocumentRequest")
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

            // get Approver email
            var approverDataList = await _dbContext.Entity<MsDocumentReqApprover>()
                                        .Include(x => x.Staff)
                                        .Where(x => x.IdSchool == param.IdSchool)
                                        .Select(x => new SendEmailCancelRequestToStaffResult_Recepient
                                        {
                                            Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName),
                                            Email = x.Staff.BinusianEmailAddress
                                        })
                                        .ToListAsync(CancellationToken);

            if (approverDataList == null || !approverDataList.Any())
                throw new BadRequestException("Document request approver is not found");

            // get PIC email
            var PICDataList = getLatestRequestDocumentStatus.IsOnProcess ? await _dbContext.Entity<TrDocumentReqPIC>()
                                .Include(x => x.Staff)
                                .Where(x => documentRequestDetail.DocumentList.Select(y => y.IdDocumentReqApplicantDetail).Any(y => y == x.IdDocumentReqApplicantDetail))
                                .Select(x => new SendEmailCancelRequestToStaffResult_Recepient
                                {
                                    Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName),
                                    Email = x.Staff.BinusianEmailAddress
                                })
                                .ToListAsync(CancellationToken) : null;

            var recepientToList = new List<SendEmailCancelRequestToStaffResult_Recepient>();

            if (approverDataList != null && approverDataList.Any())
                recepientToList.AddRange(approverDataList);
            if (PICDataList != null && PICDataList.Any())
                recepientToList.AddRange(PICDataList);

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
                    ToList = recepientToList
                                .Select(x => new SendSendGridEmailRequest_AddressBuilder
                                {
                                    DisplayName = x.Name,
                                    Address = x.Email
                                })
                                .ToList(),
                    CcList = null,
                    BccList = null
                }
            });

            var result = new SendEmailCancelRequestToStaffResult();
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
