using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class GetDocumentRequestDetailForEditHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly GetDocumentRequestPaymentInfoHandler _getDocumentRequestPaymentInfoHandler;

        public GetDocumentRequestDetailForEditHandler(
            IDocumentDbContext dbContext, 
            GetDocumentRequestPaymentInfoHandler getDocumentRequestPaymentInfoHandler)
        {
            _dbContext = dbContext;
            _getDocumentRequestPaymentInfoHandler = getDocumentRequestPaymentInfoHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDocumentRequestDetailForEditRequest>(
                            nameof(GetDocumentRequestDetailForEditRequest.IdDocumentReqApplicant),
                            nameof(GetDocumentRequestDetailForEditRequest.IdStudent));

            var result = await GetDocumentRequestDetailForEdit(new GetDocumentRequestDetailForEditRequest
            {
                IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                IdStudent = param.IdStudent
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<GetDocumentRequestDetailForEditResult> GetDocumentRequestDetailForEdit(GetDocumentRequestDetailForEditRequest param)
        {
            GetDocumentRequestPaymentInfoResult getPaymentInfo = null;

            var getPaymentInfoRaw = await _getDocumentRequestPaymentInfoHandler.GetDocumentRequestPaymentInfo(new GetDocumentRequestPaymentInfoRequest
            {
                IdDocumentReqApplicantList = new List<string>() { param.IdDocumentReqApplicant }
            });

            getPaymentInfo = getPaymentInfoRaw.FirstOrDefault();

            var getAcademicYearDocumentRawList = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                            .Include(x => x.AcademicYear)
                                                            .Where(x => x.IdDocumentReqApplicant == param.IdDocumentReqApplicant &&
                                                                        !string.IsNullOrEmpty(x.IdAcademicYearDocument))
                                                            .Select(x => new ItemValueVm
                                                            {
                                                                Id = x.IdAcademicYearDocument,
                                                                Description = x.AcademicYear.Description
                                                            })
                                                            .ToListAsync(CancellationToken);

            var getGradeDocumentRawList = await _dbContext.Entity<MsHomeroomStudent>()
                                                .Include(x => x.Homeroom)
                                                    .ThenInclude(x => x.Grade)
                                                    .ThenInclude(x => x.Level)
                                                .Include(x => x.Homeroom)
                                                    .ThenInclude(x => x.GradePathwayClassroom)
                                                    .ThenInclude(x => x.Classroom)
                                                .Where(x => x.IdStudent == param.IdStudent &&
                                                            getAcademicYearDocumentRawList.Select(y => y.Id).Any(y => y == x.Homeroom.Grade.Level.IdAcademicYear))
                                                .Select(x => new
                                                {
                                                    IdAcademicYear = x.Homeroom.Grade.Level.IdAcademicYear,
                                                    Grade = new ItemValueVm
                                                    {
                                                        Id = x.Homeroom.IdGrade,
                                                        Description = x.Homeroom.Grade.Description
                                                    },
                                                    HomeroomName = x.Homeroom.Grade.Description + x.Homeroom.GradePathwayClassroom.Classroom.Description
                                                })
                                                .ToListAsync(CancellationToken);

            var getPeriodDocumentRawList = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                            .Include(x => x.Period)
                                                            .Where(x => x.IdDocumentReqApplicant == param.IdDocumentReqApplicant &&
                                                                        !string.IsNullOrEmpty(x.IdPeriodDocument))
                                                            .Select(x => new ItemValueVm
                                                            {
                                                                Id = x.IdPeriodDocument,
                                                                Description = x.Period.Description
                                                            })
                                                            .ToListAsync(CancellationToken);

            var documentListRaw = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                .Include(x => x.DocumentReqType)
                                .Include(x => x.DocumentReqPICs)
                                    .ThenInclude(x => x.Staff)
                                .Include(x => x.DocumentReqApplicantFormAnswers)
                                    .ThenInclude(x => x.DocumentReqFormFieldAnswered)
                                .Where(x => x.IdDocumentReqApplicant == param.IdDocumentReqApplicant)
                                .Select(x => new
                                {
                                    IdDocumentReqApplicantDetail = x.Id,
                                    IdDocumentReqType = x.IdDocumentReqType,
                                    DocumentName = x.DocumentReqType.Name,
                                    DocumentTypeDescription = x.DocumentReqType.Description,
                                    IsAcademicDocument = x.DocumentReqType.IsAcademicDocument,
                                    NoOfPages = x.NoOfPages,
                                    NoOfCopy = x.NoOfCopy,
                                    PriceReal = x.PriceReal,
                                    PriceInvoice = x.PriceInvoice,
                                    IdAcademicYearDocument = x.IdAcademicYearDocument,
                                    IdPeriodDocument = x.IdPeriodDocument,
                                    EstimatedProcessDays = x.DocumentReqType.DefaultNoOfProcessDay,
                                    NeedHardCopy = x.NeedHardCopy,
                                    NeedSoftCopy = x.NeedSoftCopy,
                                    DocumentReceiver = new
                                    {
                                        IdBinusianReceiver = x.IdBinusianReceiver,
                                        ReceivedDateByStaff = x.ReceivedDateByStaff
                                    },
                                    PICList = x.DocumentReqPICs
                                                .Select(y => new NameValueVm
                                                {
                                                    Id = y.IdBinusian,
                                                    Name = y.IdBinusian + " - " + y.Staff.FirstName + (string.IsNullOrEmpty(y.Staff.LastName) ? "" : (" " + y.Staff.LastName))
                                                })
                                                .OrderBy(x => x.Name)
                                                .ToList(),
                                    //AdditionalFieldsRaw = x.DocumentReqApplicantFormAnswers == null || x.DocumentReqApplicantFormAnswers.Count == 0 ? null :
                                    //                    x.DocumentReqApplicantFormAnswers
                                    //                    .Select(y => new
                                    //                    {
                                    //                        IdDocumentReqFormFieldAnswered = y.IdDocumentReqFormFieldAnswered,
                                    //                        QuestionDescription = y.DocumentReqFormFieldAnswered.QuestionDescription,
                                    //                        OrderNo = y.DocumentReqFormFieldAnswered.OrderNumber,
                                    //                        AnswerText = y.TextValue
                                    //                    })
                                    //                    .ToList()
                                })
                                .OrderBy(x => x.DocumentName)
                                .ToListAsync(CancellationToken);

            var getAdditionalFieldsRaw = await _dbContext.Entity<TrDocumentReqApplicantFormAnswer>()
                                            .Include(x => x.DocumentReqFormFieldAnswered)
                                            .ThenInclude(x => x.DocumentReqFieldType)
                                            .Where(x => documentListRaw.Select(y => y.IdDocumentReqApplicantDetail).Any(y => y == x.IdDocumentReqApplicantDetail))
                                            .Select(x => new
                                            {
                                                IdDocumentReqApplicantDetail = x.IdDocumentReqApplicantDetail,
                                                IdDocumentReqFormFieldAnswered = x.IdDocumentReqFormFieldAnswered,
                                                IdDocumentReqOptionCategory = x.IdDocumentReqOptionCategory,
                                                QuestionDescription = x.DocumentReqFormFieldAnswered.QuestionDescription,
                                                OrderNumber = x.DocumentReqFormFieldAnswered.OrderNumber,
                                                IsRequired = x.DocumentReqFormFieldAnswered.IsRequired,
                                                FieldType = new ItemValueVm
                                                {
                                                    Id = x.DocumentReqFormFieldAnswered.IdDocumentReqFieldType,
                                                    Description = x.DocumentReqFormFieldAnswered.DocumentReqFieldType.Type,
                                                },
                                                HasOption = x.DocumentReqFormFieldAnswered.DocumentReqFieldType.HasOption,
                                                TextValue = x.TextValue,
                                                IdDocumentReqOption = x.IdDocumentReqOption
                                            })
                                            .Distinct()
                                            .ToListAsync(CancellationToken);

            var getAdditionalFieldsFinalRaw = getAdditionalFieldsRaw
                                                .Select(x => new
                                                {
                                                    IdDocumentReqApplicantDetail = x.IdDocumentReqApplicantDetail,
                                                    IdDocumentReqFormFieldAnswered = x.IdDocumentReqFormFieldAnswered,
                                                    IdDocumentReqOptionCategory = x.IdDocumentReqOptionCategory,
                                                    QuestionDescription = x.QuestionDescription,
                                                    OrderNumber = x.OrderNumber,
                                                    IsRequired = x.IsRequired,
                                                    FieldType = x.FieldType,
                                                    HasOption = x.HasOption,
                                                    TextValue = x.HasOption ? null : x.TextValue,
                                                    IdDocumentReqOptionAnsweredList = getAdditionalFieldsRaw
                                                                            .Where(y => y.IdDocumentReqApplicantDetail == x.IdDocumentReqApplicantDetail && y.IdDocumentReqFormFieldAnswered == x.IdDocumentReqFormFieldAnswered)
                                                                            .Any() && getAdditionalFieldsRaw
                                                                            .Where(y => y.IdDocumentReqApplicantDetail == x.IdDocumentReqApplicantDetail && y.IdDocumentReqFormFieldAnswered == x.IdDocumentReqFormFieldAnswered).All(y => y.IdDocumentReqOption != null) ?

                                                                            getAdditionalFieldsRaw
                                                                            .Where(y => y.IdDocumentReqApplicantDetail == x.IdDocumentReqApplicantDetail && y.IdDocumentReqFormFieldAnswered == x.IdDocumentReqFormFieldAnswered)
                                                                            .Select(y => y.IdDocumentReqOption)
                                                                            .ToList()
                                                                            : null
                                                })
                                                .ToList();

            var getListIdDocumentReqOptionAnsweredRawList = getAdditionalFieldsFinalRaw
                                                            .Where(x => x.IdDocumentReqOptionAnsweredList != null && x.IdDocumentReqOptionAnsweredList.Any())
                                                            .SelectMany(y => y.IdDocumentReqOptionAnsweredList)
                                                            .ToList();

            var getOptionsRaw = await _dbContext.Entity<MsDocumentReqOption>()
                                    .Where(x => (getAdditionalFieldsRaw.Select(y => y.IdDocumentReqOptionCategory).Any(y => y == x.IdDocumentReqOptionCategory) &&
                                                x.Status == true) ||
                                                getListIdDocumentReqOptionAnsweredRawList.Any(y => y == x.Id))
                                    .Select(x => new
                                    {
                                        IdDocumentReqOptionCategory = x.IdDocumentReqOptionCategory,
                                        Options = new GetDocumentRequestDetailForEditResult_AdditionalFieldOptions
                                        {
                                            IdDocumentReqOption = x.Id,
                                            OptionDescription = x.OptionDescription
                                        }
                                    })
                                    .ToListAsync(CancellationToken);

            var documentListFinal = documentListRaw
                                        .Select(x => new GetDocumentRequestDetailForEditResult_Document
                                        {
                                            IdDocumentReqApplicantDetail = x.IdDocumentReqApplicantDetail,
                                            IdDocumentReqType = x.IdDocumentReqType,
                                            DocumentName = x.DocumentName,
                                            DocumentTypeDescription = x.DocumentTypeDescription,
                                            NoOfPages = x.NoOfPages,
                                            NoOfCopy = x.NoOfCopy,
                                            PriceReal = x.PriceReal,
                                            PriceInvoice = x.PriceInvoice,
                                            IsAcademicDocument = x.IsAcademicDocument,
                                            IsMadeFree = x.PriceReal != x.PriceInvoice ? true : false,
                                            AcademicYearDocument = getAcademicYearDocumentRawList
                                                                    .Where(y => y.Id == x.IdAcademicYearDocument)
                                                                    .FirstOrDefault(),
                                            GradeDocument = getGradeDocumentRawList
                                                            .Where(y => y.IdAcademicYear == x.IdAcademicYearDocument)
                                                            .Select(y => y.Grade)
                                                            .FirstOrDefault(),
                                            HomeroomNameDocument = getGradeDocumentRawList
                                                            .Where(y => y.IdAcademicYear == x.IdAcademicYearDocument)
                                                            .Select(y => y.HomeroomName)
                                                            .FirstOrDefault(),
                                            AcademicYearAndGradeDocument = new ItemValueVm
                                            {
                                                Id = getGradeDocumentRawList
                                                            .Where(y => y.IdAcademicYear == x.IdAcademicYearDocument)
                                                            .Select(y => y.Grade.Id)
                                                            .FirstOrDefault(),
                                                Description = string.IsNullOrEmpty(getGradeDocumentRawList
                                                            .Where(y => y.IdAcademicYear == x.IdAcademicYearDocument)
                                                            .Select(y => y.Grade.Id)
                                                            .FirstOrDefault()) ? null :
                                                             getGradeDocumentRawList
                                                            .Where(y => y.IdAcademicYear == x.IdAcademicYearDocument)
                                                            .Select(y => y.HomeroomName)
                                                            .FirstOrDefault() + 
                                                                    " (" +
                                                                    getAcademicYearDocumentRawList
                                                                    .Where(y => y.Id == x.IdAcademicYearDocument)
                                                                    .Select(y => y.Description)
                                                                    .FirstOrDefault() +
                                                                    ")"
                                            },
                                            PeriodDocument = getPeriodDocumentRawList
                                                                .Where(y => y.Id == x.IdPeriodDocument)
                                                                .FirstOrDefault(),
                                            EstimatedProcessDays = x.EstimatedProcessDays,
                                            NeedHardCopy = x.NeedHardCopy,
                                            NeedSoftCopy = x.NeedSoftCopy,
                                            PICList = x.PICList,
                                            AdditionalFieldList = getAdditionalFieldsFinalRaw
                                                                    .Where(y => y.IdDocumentReqApplicantDetail == x.IdDocumentReqApplicantDetail)
                                                                    .Select(y => new GetDocumentRequestDetailForEditResult_AdditionalField
                                                                    {
                                                                        IdDocumentReqFormFieldAnswered = y.IdDocumentReqFormFieldAnswered,
                                                                        FieldType = y.FieldType,
                                                                        HasOption = y.HasOption,
                                                                        QuestionDescription = y.QuestionDescription,
                                                                        OrderNo = y.OrderNumber,
                                                                        IsRequired = y.IsRequired,
                                                                        Options = getOptionsRaw
                                                                                    .Where(z => z.IdDocumentReqOptionCategory == y.IdDocumentReqOptionCategory)
                                                                                    .Any() ?
                                                                                  getOptionsRaw
                                                                                    .Where(z => z.IdDocumentReqOptionCategory == y.IdDocumentReqOptionCategory)
                                                                                    .Select(z => z.Options)
                                                                                    .ToList()
                                                                                    : null,
                                                                        TextValue = y.TextValue,
                                                                        IdDocumentReqOptionAnsweredList =  y.IdDocumentReqOptionAnsweredList
                                                                    })
                                                                    .ToList()
                                        })
                                        .ToList();

            var result = await _dbContext.Entity<MsDocumentReqApplicant>()
                            .Include(x => x.Parent)
                                .ThenInclude(x => x.ParentRole)
                            .Include(x => x.Student)
                            .Include(x => x.DocumentReqStatusTrackingHistories)
                                .ThenInclude(x => x.DocumentReqStatusWorkflow)
                            .Include(x => x.DocumentReqApplicantCollections)
                                .ThenInclude(x => x.Venue)
                            .Where(x => x.Id == param.IdDocumentReqApplicant)
                            .Select(x => new GetDocumentRequestDetailForEditResult
                            {
                                IdDocumentReqApplicant = x.Id,
                                RequestNumber = x.RequestNumber,
                                RequestDate = x.RequestDate,
                                EstimationFinishDate = x.EstimationFinishDate,
                                EstimationFinishDays = x.EstimationFinishDays,
                                StartOnProcessDate = x.DocumentReqStatusTrackingHistories
                                                    .Where(y => y.IsOnProcess)
                                                    .Select(y => y.StatusDate)
                                                    .OrderBy(y => y)
                                                    .FirstOrDefault(),
                                IdSchool = x.IdSchool,
                                RequestedBy = new ItemValueVm
                                {
                                    Id = x.IdParentApplicant,
                                    Description = $"{NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.LastName)} ({x.Parent.ParentRole.ParentRoleNameEng})"
                                },
                                CreatedBy = x.CreatedBy,
                                Student = new NameValueVm
                                {
                                    Id = x.IdStudent,
                                    Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
                                },
                                DocumentList = documentListFinal,
                                Payment = new GetDocumentRequestDetailForEditResult_Payment
                                {
                                    PaymentStatus = getPaymentInfo.PaymentStatus,
                                    TotalAmountInvoice = getPaymentInfo.TotalAmountInvoice,
                                    EndDatePayment = getPaymentInfo.EndDatePayment,
                                    PaymentDate = getPaymentInfo.PaymentDate,
                                    DocumentReqPaymentMethod = getPaymentInfo.PaymentStatus == PaymentStatus.Free ? null : new GetDocumentRequestDetailForEditResult_PaymentMethod
                                    {
                                        Id = getPaymentInfo.DocumentReqPaymentMethod.Id,
                                        Description = getPaymentInfo.DocumentReqPaymentMethod.Description,
                                        IsVirtualAccount = getPaymentInfo.IsVirtualAccount.HasValue ? getPaymentInfo.IsVirtualAccount : null,
                                        UsingManualVerification = getPaymentInfo.UsingManualVerification.HasValue ? getPaymentInfo.UsingManualVerification : null
                                    },
                                    PaidAmount = getPaymentInfo.PaidAmount,
                                    SenderAccountName = getPaymentInfo.SenderAccountName,
                                    AttachmentImageUrl = getPaymentInfo.AttachmentImageUrl
                                },
                            })
                            .FirstOrDefaultAsync(CancellationToken);

            return result;
        }
    }
}
