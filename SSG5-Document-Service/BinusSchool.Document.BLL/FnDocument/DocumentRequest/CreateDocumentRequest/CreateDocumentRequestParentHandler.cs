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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest.Validator;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestNotification;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class CreateDocumentRequestParentHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly AddDocumentRequestWorkflowHandler _addDocumentRequestWorkflowHandler;
        private readonly SendEmailNewRequestToParentHandler _sendEmailNewRequestToParentHandler;

        public CreateDocumentRequestParentHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime,
            AddDocumentRequestWorkflowHandler addDocumentRequestWorkflowHandler,
            SendEmailNewRequestToParentHandler sendEmailNewRequestToParentHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _addDocumentRequestWorkflowHandler = addDocumentRequestWorkflowHandler;
            _sendEmailNewRequestToParentHandler = sendEmailNewRequestToParentHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CreateDocumentRequestParentRequest, CreateDocumentRequestParentValidator>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                // get Active AY
                var getActiveAYSemester = _dbContext.Entity<MsPeriod>()
                                        .Include(x => x.Grade)
                                        .Include(x => x.Grade.Level)
                                        .Include(x => x.Grade.Level.AcademicYear)
                                        .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                                        .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
                                        .OrderByDescending(x => x.StartDate)
                                        .Select(x => new
                                        {
                                            IdAcademicYear = x.Grade.Level.AcademicYear.Id,
                                            AcademicYearDescription = x.Grade.Level.AcademicYear.Description,
                                            Semester = x.Semester
                                        })
                                        .FirstOrDefault();

                // get request number
                var getLatestRequestNumber = await _dbContext.Entity<MsDocumentReqApplicant>()
                                                .Where(x => x.IdSchool == param.IdSchool &&
                                                            x.RequestDate.Date == _dateTime.ServerTime.Date)
                                                .Select(x => x.RequestNumber)
                                                .OrderByDescending(x => x)
                                                .FirstOrDefaultAsync(CancellationToken);

                var newRunningNumber = getLatestRequestNumber == null ?
                                        "001" :
                                        (int.Parse((getLatestRequestNumber.Substring(getLatestRequestNumber.Length - 3))) + 1).ToString("D3");

                var newRequestNumber = "R" + param.IdSchool + _dateTime.ServerTime.ToString("yyMMdd") + newRunningNumber;

                // get latest student status and homeroom
                var studentStatusAndHomeroom = _dbContext.Entity<MsStudent>()
                                                .Include(x => x.TrStudentStatuss)
                                                    .ThenInclude(x => x.StudentStatus)
                                                .Include(x => x.HomeroomStudents)
                                                    .ThenInclude(x => x.Homeroom)
                                                    .ThenInclude(x => x.Grade)
                                                    .ThenInclude(x => x.Level)
                                                .Where(x => x.Id == param.IdStudent)
                                                .Select(x => new
                                                {
                                                    IdStudentStatus = x.TrStudentStatuss
                                                                        .Where(y => y.CurrentStatus == "A")
                                                                        .OrderByDescending(y => y.StartDate)
                                                                        .Select(y => y.IdStudentStatus)
                                                                        .Any() ?
                                                                        x.TrStudentStatuss
                                                                        .Where(y => y.CurrentStatus == "A")
                                                                        .OrderByDescending(y => y.StartDate)
                                                                        .Select(y => y.IdStudentStatus)
                                                                        .FirstOrDefault() :
                                                                        x.IdStudentStatus,
                                                    IdHomeroom = x.HomeroomStudents
                                                                .Where(y =>
                                                                    y.Homeroom.Grade.Level.IdAcademicYear == getActiveAYSemester.IdAcademicYear &&
                                                                    y.Homeroom.Semester == getActiveAYSemester.Semester)
                                                                .Select(y => y.IdHomeroom)
                                                                .FirstOrDefault()
                                                })
                                                .FirstOrDefault();

                // Get all DocumentReqType
                var getDocumentReqTypeRaw = await _dbContext.Entity<MsDocumentReqType>()
                                            .Where(x => x.Status == true &&
                                                        param.DocumentRequestList.Select(y => y.IdDocumentReqType).Any(y => y == x.Id))
                                            .ToListAsync(CancellationToken);

                // Get all DefaultPIC
                var defaultPICAllRawList = new List<CreateDocumentRequestParentRequest_PIC>();

                var defaultPICIndividualList = _dbContext.Entity<MsDocumentReqDefaultPIC>()
                                                .Include(x => x.Staff)
                                                .Where(x => param.DocumentRequestList.Select(y => y.IdDocumentReqType).Any(y => y == x.IdDocumentReqType))
                                                .ToList()
                                                .GroupBy(x => x.IdDocumentReqType)
                                                .Select(x => new CreateDocumentRequestParentRequest_PIC
                                                {
                                                    IdDocumentReqType = x.Key,
                                                    IdBinusianList = x.Select(y => y.IdBinusian).ToList()
                                                })
                                                .ToList();

                if (defaultPICIndividualList != null || defaultPICIndividualList.Any())
                    defaultPICAllRawList.AddRange(defaultPICIndividualList);

                var defaultPICGroupIdBinusianList = await _dbContext.Entity<MsDocumentReqDefaultPICGroup>()
                                                    .Include(x => x.Role)
                                                        .ThenInclude(x => x.UserRoles)
                                                    .Where(x => param.DocumentRequestList.Select(y => y.IdDocumentReqType).Any(y => y == x.IdDocumentReqType))
                                                    .Select(x => new CreateDocumentRequestParentRequest_PIC
                                                    {
                                                        IdDocumentReqType = x.IdDocumentReqType,
                                                        IdBinusianList = x.Role.UserRoles.Select(x => x.IdUser).ToList()
                                                    })
                                                    .ToListAsync(CancellationToken);

                if (defaultPICGroupIdBinusianList != null || defaultPICGroupIdBinusianList.Any())
                    defaultPICAllRawList.AddRange(defaultPICGroupIdBinusianList);

                // Get longest estimation finish days
                int longestEstimationFinishDays = getDocumentReqTypeRaw
                                                    .Select(x => x.DefaultNoOfProcessDay)
                                                    .OrderByDescending(x => x)
                                                    .FirstOrDefault();

                // Get need approval status 
                bool needApprovalDocumentRequest = getDocumentReqTypeRaw
                                                    .Where(x => x.ParentNeedApproval)
                                                    .Any();

                // Add MsDocumentReqApplicant
                var newIdDocumentReqApplicant = Guid.NewGuid().ToString();

                var parentApplicantInfo = await _dbContext.Entity<MsParent>()
                                            .Include(x => x.ParentRole)
                                            .Where(x => x.Id == param.IdParentApplicant)
                                            .Select(x => new ItemValueVm
                                            {
                                                Id = x.Id,
                                                Description = $"{NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName)} ({x.ParentRole.ParentRoleNameEng})"
                                            })
                                            .FirstOrDefaultAsync(CancellationToken);

                var newMsDocumentReqApplicant = new MsDocumentReqApplicant
                {
                    Id = newIdDocumentReqApplicant,
                    IdSchool = param.IdSchool,
                    RequestNumber = newRequestNumber,
                    RequestDate = _dateTime.ServerTime,
                    EstimationFinishDays = longestEstimationFinishDays,
                    IdParentApplicant = param.IdParentApplicant,
                    IdStudent = param.IdStudent,
                    IdStudentStatus = studentStatusAndHomeroom.IdStudentStatus,
                    IdHomeroom = studentStatusAndHomeroom.IdHomeroom,
                    DocumentNeedApproval = needApprovalDocumentRequest,
                    CanProcessBeforePaid = false,
                    CreatedBy = parentApplicantInfo.Description,
                    ApprovalStatus = needApprovalDocumentRequest ? DocumentRequestApprovalStatus.WaitingForApproval : DocumentRequestApprovalStatus.Approved,
                    IdDocumentReqStatusWorkflow = DocumentRequestStatusWorkflow.InitialStatus
                };

                _dbContext.Entity<MsDocumentReqApplicant>().Add(newMsDocumentReqApplicant);

                var getAllIdGradeDocumentRequestParam = param.DocumentRequestList.Select(y => y.IdGradeDocument).ToList();

                var getAllIdDocumentReqFormFieldParam = param.DocumentRequestList.SelectMany(y => y.AdditionalFieldsList).Select(y => y.IdDocumentReqFormField).ToList();

                var getAllIdDocumentReqOptionParam = param.DocumentRequestList.SelectMany(y => y.AdditionalFieldsList).SelectMany(y => y.IdDocumentReqOptionList).Select(y => y).ToList();

                // Get all Grade AY Info
                var getDocumentReqAYByIdGradeRaw = await _dbContext.Entity<MsGrade>()
                                                    .Include(x => x.Level)
                                                    .ThenInclude(x => x.AcademicYear)
                                                    .Where(x => getAllIdGradeDocumentRequestParam.Any(y => y == x.Id))
                                                    .Select(x => new
                                                    {
                                                        IdGrade = x.Id,
                                                        AcademicYear = x.Level.AcademicYear
                                                    })
                                                    .ToListAsync(CancellationToken);

                // Get all MsDocumentReqFormField
                var getDocumentReqFormFieldRaw = await _dbContext.Entity<MsDocumentReqFormField>()
                                                    .Include(x => x.DocumentReqFieldType)
                                                    .Where(x => getAllIdDocumentReqFormFieldParam.Any(y => y == x.Id))
                                                    .ToListAsync(CancellationToken);

                // Get all MsDocumentReqOption
                var getMsDocumentReqOptionRaw = await _dbContext.Entity<MsDocumentReqOption>()
                                                            .Include(x => x.DocumentReqOptionCategory)
                                                            .Where(x => getAllIdDocumentReqOptionParam.Any(y => y == x.Id))
                                                            .ToListAsync(CancellationToken);

                decimal totalPriceInvoice = 0;
                int? maxInvoiceDueHoursPayment = null;

                foreach (var documentRequest in param.DocumentRequestList)
                {
                    var getDocumentReqType = getDocumentReqTypeRaw
                                            .Where(x => x.Id == documentRequest.IdDocumentReqType)
                                            .FirstOrDefault();

                    if (getDocumentReqType == null)
                        throw new BadRequestException("Failed to save because one of the document types is missing or inactive");

                    // count max invoice due hours payment
                    if (getDocumentReqType.InvoiceDueHoursPayment != null)
                    {
                        maxInvoiceDueHoursPayment = (maxInvoiceDueHoursPayment == null ? getDocumentReqType.InvoiceDueHoursPayment :
                            (getDocumentReqType.InvoiceDueHoursPayment > maxInvoiceDueHoursPayment ? (maxInvoiceDueHoursPayment = getDocumentReqType.InvoiceDueHoursPayment) : maxInvoiceDueHoursPayment));
                    }

                    // Add TrDocumentReqApplicantDetail
                    var idAcademicYearDocument = getDocumentReqAYByIdGradeRaw
                                                    .Where(x => x.IdGrade == documentRequest.IdGradeDocument)
                                                    .Select(x => x.AcademicYear.Id)
                                                    .FirstOrDefault();

                    var newIdDocumentReqApplicantDetail = Guid.NewGuid().ToString();

                    var newTrDocumentReqApplicantDetail = new TrDocumentReqApplicantDetail
                    {
                        Id = newIdDocumentReqApplicantDetail,
                        IdDocumentReqApplicant = newIdDocumentReqApplicant,
                        IdDocumentReqType = documentRequest.IdDocumentReqType,
                        NoOfPages = getDocumentReqType.DefaultNoOfPages,
                        NoOfCopy = documentRequest.NoOfCopy == null ? 1 : documentRequest.NoOfCopy.Value,
                        PriceReal = getDocumentReqType.Price,
                        PriceInvoice = getDocumentReqType.Price,
                        IdAcademicYearDocument = idAcademicYearDocument,
                        IdPeriodDocument = documentRequest.IdPeriodDocument,
                        NeedHardCopy = getDocumentReqType.HardCopyAvailable,
                        NeedSoftCopy = getDocumentReqType.SoftCopyAvailable
                    };

                    _dbContext.Entity<TrDocumentReqApplicantDetail>().Add(newTrDocumentReqApplicantDetail);

                    totalPriceInvoice += (newTrDocumentReqApplicantDetail.PriceInvoice * newTrDocumentReqApplicantDetail.NoOfCopy);

                    // Get Document Req PIC ID Binusian List
                    var defaultPICAllList = defaultPICAllRawList
                                                .Where(x => x.IdDocumentReqType == documentRequest.IdDocumentReqType)
                                                .SelectMany(x => x.IdBinusianList)
                                                .Distinct()
                                                .ToList();

                    // Add TrDocumentReqPIC
                    var newTrDocumentReqPICList = defaultPICAllList
                                                .Select(x => new TrDocumentReqPIC
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    IdDocumentReqApplicantDetail = newIdDocumentReqApplicantDetail,
                                                    IdBinusian = x
                                                })
                                                .ToList();

                    _dbContext.Entity<TrDocumentReqPIC>().AddRange(newTrDocumentReqPICList);

                    #region Additional Field
                    foreach (var additionalField in documentRequest.AdditionalFieldsList)
                    {
                        var getDocumentReqFormField = getDocumentReqFormFieldRaw
                                                        .Where(x => x.Id == additionalField.IdDocumentReqFormField)
                                                        .FirstOrDefault();

                        if (getDocumentReqFormField == null)
                            throw new BadRequestException("Failed to save. Additional field does not exist. Please refresh the page or contact administrator");

                        // Add MsDocumentReqFormFieldAnswered
                        var newIdDocumentReqFormFieldAnswered = Guid.NewGuid().ToString();

                        var newMsDocumentReqFormFieldAnswered = new MsDocumentReqFormFieldAnswered
                        {
                            Id = newIdDocumentReqFormFieldAnswered,
                            IdDocumentReqType = getDocumentReqFormField.IdDocumentReqType,
                            IdDocumentReqFieldType = getDocumentReqFormField.IdDocumentReqFieldType,
                            QuestionDescription = getDocumentReqFormField.QuestionDescription,
                            OrderNumber = getDocumentReqFormField.OrderNumber,
                            IsRequired = getDocumentReqFormField.IsRequired,
                            IdDocumentReqOptionCategory = getDocumentReqFormField.IdDocumentReqOptionCategory
                        };

                        _dbContext.Entity<MsDocumentReqFormFieldAnswered>().Add(newMsDocumentReqFormFieldAnswered);


                        // Add TrDocumentReqApplicantFormAnswer
                        // has option (ddl, checkbox)
                        if (getDocumentReqFormField.DocumentReqFieldType.HasOption)
                        {
                            if ((additionalField.IdDocumentReqOptionList == null || !additionalField.IdDocumentReqOptionList.Any()) && getDocumentReqFormField.IsRequired)
                                throw new BadRequestException("Please fill all of the required fields");

                            var getMsDocumentReqOptionList = getMsDocumentReqOptionRaw
                                                            .Where(x => additionalField.IdDocumentReqOptionList.Any(y => y == x.Id))
                                                            .ToList();

                            if (getMsDocumentReqOptionList == null || !getMsDocumentReqOptionList.Any())
                                throw new BadRequestException("Failed to save. Document request option category is not found. Please refresh the page or contact administrator");

                            var newTrDocumentReqApplicantFormAnswerList = (additionalField.IdDocumentReqOptionList == null || additionalField.IdDocumentReqOptionList.Count() <= 0) ?
                            // condition if no answer and not required to answer
                            new List<TrDocumentReqApplicantFormAnswer>
                            {
                                new TrDocumentReqApplicantFormAnswer
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdDocumentReqApplicantDetail = newIdDocumentReqApplicantDetail,
                                    IdDocumentReqFormFieldAnswered = newIdDocumentReqFormFieldAnswered,
                                    IdDocumentReqOptionCategory = getDocumentReqFormField.IdDocumentReqOptionCategory,
                                    IdDocumentReqOption = null,
                                    TextValue = null
                                }
                            }
                            :
                            additionalField.IdDocumentReqOptionList
                                .Select(x => new TrDocumentReqApplicantFormAnswer
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdDocumentReqApplicantDetail = newIdDocumentReqApplicantDetail,
                                    IdDocumentReqFormFieldAnswered = newIdDocumentReqFormFieldAnswered,
                                    IdDocumentReqOptionCategory = getDocumentReqFormField.IdDocumentReqOptionCategory,
                                    IdDocumentReqOption = x,
                                    TextValue = getMsDocumentReqOptionList.Where(y => y.Id == x).Select(y => y.OptionDescription).FirstOrDefault()
                                })
                                .ToList();

                            _dbContext.Entity<TrDocumentReqApplicantFormAnswer>().AddRange(newTrDocumentReqApplicantFormAnswerList);
                        }
                        // textbox
                        else
                        {
                            var newTrDocumentReqApplicantFormAnswer = new TrDocumentReqApplicantFormAnswer
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdDocumentReqApplicantDetail = newIdDocumentReqApplicantDetail,
                                IdDocumentReqFormFieldAnswered = newIdDocumentReqFormFieldAnswered,
                                TextValue = additionalField.TextValue
                            };

                            _dbContext.Entity<TrDocumentReqApplicantFormAnswer>().Add(newTrDocumentReqApplicantFormAnswer);
                        }
                    }
                    #endregion
                }

                // NOT FREE
                if (totalPriceInvoice > 0)
                {
                    #region Payment
                    // Get LtDocumentReqPaymentMethod
                    var getLtDocumentReqPaymentMethod = await _dbContext.Entity<LtDocumentReqPaymentMethod>()
                                                        .FindAsync(param.Payment.IdDocumentReqPaymentMethod);


                    var invoiceDateTime = _dateTime.ServerTime;

                    decimal totalAmountInvoiceWithUniqueNumber = 0;

                    if (!getLtDocumentReqPaymentMethod.UsingManualVerification)
                        totalAmountInvoiceWithUniqueNumber = totalPriceInvoice;
                    else
                    {
                        // Generate unique Amount untuk invoice
                        Random random = new Random();

                        bool successGetUniqueAmount = false;
                        do
                        {
                            var random3DigitsUniqueNumber = Convert.ToDecimal(random.Next(10, (999 - (Convert.ToInt32(totalPriceInvoice) % 1000))));

                            var tempTotalAmountInvoice = totalPriceInvoice + random3DigitsUniqueNumber;

                            var isExistsUniqueAmount = _dbContext.Entity<TrDocumentReqPaymentMapping>()
                                                        .Where(x => x.IdDocumentReqPaymentManual == null &&
                                                                    _dateTime.ServerTime >= x.StartDatePayment &&
                                                                    _dateTime.ServerTime <= x.EndDatePayment &&
                                                                    x.IsVirtualAccount == false &&
                                                                    x.UsingManualVerification == true &&
                                                                    x.TotalAmountInvoice == tempTotalAmountInvoice)
                                                        .Any();

                            if (isExistsUniqueAmount == false)
                            {
                                successGetUniqueAmount = true;
                                totalAmountInvoiceWithUniqueNumber = tempTotalAmountInvoice;
                            }

                        } while (successGetUniqueAmount == false);
                    }

                    var newTrDocumentReqPaymentMapping = new TrDocumentReqPaymentMapping
                    {
                        Id = Guid.NewGuid().ToString(),
                        TotalAmountReal = totalPriceInvoice,
                        TotalAmountInvoice = totalAmountInvoiceWithUniqueNumber,
                        IdDocumentReqApplicant = newIdDocumentReqApplicant,
                        IdDocumentReqPaymentMethod = param.Payment.IdDocumentReqPaymentMethod,
                        UsingManualVerification = getLtDocumentReqPaymentMethod.UsingManualVerification,
                        IsVirtualAccount = getLtDocumentReqPaymentMethod.IsVirtualAccount,
                    };

                    // for no need approval document, start date and end date payment are set
                    if (!newMsDocumentReqApplicant.DocumentNeedApproval)
                    {
                        newTrDocumentReqPaymentMapping.StartDatePayment = invoiceDateTime;
                        newTrDocumentReqPaymentMapping.EndDatePayment = invoiceDateTime.AddHours(maxInvoiceDueHoursPayment == null ? 0 : Convert.ToDouble(maxInvoiceDueHoursPayment.Value));
                    }

                    _dbContext.Entity<TrDocumentReqPaymentMapping>().Add(newTrDocumentReqPaymentMapping);
                    #endregion
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                // Add TrDocumentReqStatusTrackingHistory

                // validate DocumentReqStatusWorkflow
                var newIdDocumentReqStatusWorkflow = new DocumentRequestStatusWorkflow();

                if (newMsDocumentReqApplicant.DocumentNeedApproval)
                    newIdDocumentReqStatusWorkflow = DocumentRequestStatusWorkflow.WaitingForApproval;
                else if (!newMsDocumentReqApplicant.DocumentNeedApproval && totalPriceInvoice > 0)
                    newIdDocumentReqStatusWorkflow = DocumentRequestStatusWorkflow.WaitingForPayment;
                else
                    newIdDocumentReqStatusWorkflow = DocumentRequestStatusWorkflow.OnProcess;

                var addTrDocumentReqStatusTrackingHistory = await _addDocumentRequestWorkflowHandler.AddDocumentRequestWorkflow(new AddDocumentRequestWorkflowRequest
                {
                    IdDocumentReqApplicant = newIdDocumentReqApplicant,
                    IdDocumentReqStatusWorkflow = newIdDocumentReqStatusWorkflow
                });

                if (!addTrDocumentReqStatusTrackingHistory.IsSuccess)
                    throw new BadRequestException("Internal error. Please contact administrator");

                var result = new CreateDocumentRequestParentResult
                {
                    IdDocumentReqApplicant = newIdDocumentReqApplicant,
                    CanConfirmPayment = newIdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPayment
                };


                #region send email notification
                // notification to parent
                try
                {
                    var sendEmail = await _sendEmailNewRequestToParentHandler.SendEmailNewRequestToParent(new SendEmailNewRequestToParentRequest
                    {
                        IdSchool = param.IdSchool,
                        IdDocumentReqApplicant = newIdDocumentReqApplicant,
                        IdStudent = param.IdStudent
                    });
                }
                catch (Exception)
                {

                }
                #endregion

                return Request.CreateApiResult2(result as object);
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
