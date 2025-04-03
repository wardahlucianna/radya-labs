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
    public class GetDocumentRequestDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly GetDocumentRequestPaymentInfoHandler _getDocumentRequestPaymentInfoHandler;

        public GetDocumentRequestDetailHandler(
            IDocumentDbContext dbContext,
            GetDocumentRequestPaymentInfoHandler getDocumentRequestPaymentInfoHandler)
        {
            _dbContext = dbContext;
            _getDocumentRequestPaymentInfoHandler = getDocumentRequestPaymentInfoHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDocumentRequestDetailRequest>(
                            nameof(GetDocumentRequestDetailRequest.IdDocumentReqApplicant),
                            nameof(GetDocumentRequestDetailRequest.IdStudent),
                            nameof(GetDocumentRequestDetailRequest.IncludePaymentInfo));

            var result = await GetDocumentRequestDetail(new GetDocumentRequestDetailRequest
            {
                IdDocumentReqApplicant = param.IdDocumentReqApplicant,
                IdStudent = param.IdStudent,
                IncludePaymentInfo = param.IncludePaymentInfo
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<GetDocumentRequestDetailResult> GetDocumentRequestDetail(GetDocumentRequestDetailRequest param)
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

            var getBinusianReceiverRawList = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                .Include(x => x.Staff)
                                                .Where(x => x.IdDocumentReqApplicant == param.IdDocumentReqApplicant &&
                                                            !string.IsNullOrEmpty(x.Staff.IdBinusian))
                                                .Select(x => new NameValueVm
                                                {
                                                    Id = x.Staff.IdBinusian,
                                                    Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                                                })
                                                .ToListAsync(CancellationToken);

            var getHomeroomWhenRequestWasMadeRaw = await _dbContext.Entity<MsDocumentReqApplicant>()
                                                    .Include(x => x.Homeroom)
                                                        .ThenInclude(x => x.GradePathwayClassroom)
                                                        .ThenInclude(x => x.Classroom)
                                                    .Include(x => x.Homeroom)
                                                        .ThenInclude(x => x.Grade)
                                                        .ThenInclude(x => x.Level)
                                                    .Where(x => x.Id == param.IdDocumentReqApplicant &&
                                                                !string.IsNullOrEmpty(x.IdHomeroom))
                                                    .Select(x => new 
                                                    {
                                                        Level = new CodeWithIdVm
                                                        {
                                                            Id = x.Homeroom.Grade.Level.Id,
                                                            Code = x.Homeroom.Grade.Level.Code,
                                                            Description = x.Homeroom.Grade.Level.Description
                                                        },
                                                        Homeroom = new ItemValueVm
                                                        {
                                                            Id = x.IdHomeroom,
                                                            Description = x.Homeroom.Grade.Description + x.Homeroom.GradePathwayClassroom.Classroom.Description
                                                        }
                                                    })
                                                    .FirstOrDefaultAsync(CancellationToken);

            var getStaffApproverRaw = await _dbContext.Entity<MsDocumentReqApplicant>()
                                            .Include(x => x.Staff)
                                            .Where(x => x.Id == param.IdDocumentReqApplicant &&
                                                        !string.IsNullOrEmpty(x.IdBinusianApprover))
                                            .Select(x => new NameValueVm
                                            {
                                                Id = x.IdBinusianApprover,
                                                Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                                            })
                                            .FirstOrDefaultAsync(CancellationToken);

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
                                    IsAcademicDocument = x.DocumentReqType.IsAcademicDocument,
                                    NoOfPages = x.NoOfPages,
                                    NoOfCopy = x.NoOfCopy,
                                    PriceReal = x.PriceReal,
                                    PriceInvoice = x.PriceInvoice,
                                    IdAcademicYearDocument = x.IdAcademicYearDocument,
                                    IdPeriodDocument = x.IdPeriodDocument,
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
                                                    Name = y.Staff.FirstName.Trim() + (string.IsNullOrEmpty(y.Staff.LastName) ? "" : (" " + y.Staff.LastName.Trim()))
                                                })
                                                .OrderBy(x => x.Name)
                                                .ToList(),
                                    AdditionalFieldsRaw = x.DocumentReqApplicantFormAnswers == null || x.DocumentReqApplicantFormAnswers.Count == 0 ? null :
                                                        x.DocumentReqApplicantFormAnswers
                                                        .Select(y => new
                                                        {
                                                            IdDocumentReqFormFieldAnswered = y.IdDocumentReqFormFieldAnswered,
                                                            QuestionDescription = y.DocumentReqFormFieldAnswered.QuestionDescription,
                                                            OrderNo = y.DocumentReqFormFieldAnswered.OrderNumber,
                                                            AnswerText = y.TextValue
                                                        })
                                                        .ToList()
                                })
                                .OrderBy(x => x.DocumentName)
                                .ToListAsync(CancellationToken);

            var documentListFinal = documentListRaw
                                        .Select(x => new GetDocumentRequestDetailResult_Document
                                        {
                                            IdDocumentReqApplicantDetail = x.IdDocumentReqApplicantDetail,
                                            IdDocumentReqType = x.IdDocumentReqType,
                                            DocumentName = x.DocumentName,
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
                                            PeriodDocument = getPeriodDocumentRawList
                                                                .Where(y => y.Id == x.IdPeriodDocument)
                                                                .FirstOrDefault(),
                                            NeedHardCopy = x.NeedHardCopy,
                                            NeedSoftCopy = x.NeedSoftCopy,
                                            DocumentIsReady = new GetDocumentRequestDetailResult_DocumentIsReady
                                            {
                                                BinusianReceiver = getBinusianReceiverRawList
                                                    .Where(y => y.Id == x.DocumentReceiver.IdBinusianReceiver)
                                                    .FirstOrDefault(),
                                                ReceivedDateByStaff = x.DocumentReceiver.ReceivedDateByStaff == null ? null : x.DocumentReceiver.ReceivedDateByStaff
                                            },
                                            PICList = x.PICList,
                                            AdditionalFieldList = x.AdditionalFieldsRaw == null || x.AdditionalFieldsRaw.Count == 0 ? null :
                                                        x.AdditionalFieldsRaw
                                                        .GroupBy(y => new
                                                        {
                                                            y.IdDocumentReqFormFieldAnswered,
                                                            y.QuestionDescription,
                                                            y.OrderNo
                                                        })
                                                        .Select(y => new GetDocumentRequestDetailResult_AdditionalField
                                                        {
                                                            IdDocumentReqFormFieldAnswered = y.Key.IdDocumentReqFormFieldAnswered,
                                                            QuestionDescription = y.Key.QuestionDescription,
                                                            OrderNo = y.Key.OrderNo,
                                                            AnswerTextList = y.Select(z => z.AnswerText).ToList()
                                                        })
                                                        .OrderBy(y => y.OrderNo)
                                                        .ToList()
                                        })
                                        .ToList();

            bool softCopyDocumentOnly = documentListFinal.All(y => y.NeedSoftCopy && y.NeedHardCopy == false);
            bool allDocumentIsReady = documentListFinal.Select(y => y.DocumentIsReady == null ? null : y.DocumentIsReady.ReceivedDateByStaff).All(y => y != null);

            var result = await _dbContext.Entity<MsDocumentReqApplicant>()
                            .Include(x => x.Parent)
                                .ThenInclude(x => x.ParentRole)
                            .Include(x => x.Student)
                                .ThenInclude(x => x.TrStudentStatuss)
                            .Include(x => x.LtStudentStatus)
                            .Include(x => x.DocumentReqStatusTrackingHistories)
                                .ThenInclude(x => x.DocumentReqStatusWorkflow)
                            .Include(x => x.DocumentReqApplicantCollections)
                            .Where(x => x.Id == param.IdDocumentReqApplicant)
                            .Select(x => new GetDocumentRequestDetailResult
                            {
                                IdDocumentReqApplicant = x.Id,
                                RequestNumber = x.RequestNumber,
                                RequestDate = x.RequestDate,
                                EstimationFinishDate = x.EstimationFinishDate,
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
                                StudentStatusWhenRequestWasCreated =
                                        // get student status when the document request was created
                                        x.Student.TrStudentStatuss
                                        .Where(y => y.IdStudentStatus == x.IdStudentStatus &&
                                                    x.RequestDate >= y.StartDate)
                                        .OrderByDescending(y => y.StartDate)
                                        .Any() ?
                                        x.Student.TrStudentStatuss
                                        .Where(y => y.IdStudentStatus == x.IdStudentStatus &&
                                                    x.RequestDate >= y.StartDate)
                                        .OrderByDescending(y => y.StartDate)
                                        .Select(y => new GetDocumentRequestDetailResult_StudentStatus
                                        {
                                            Id = y.IdStudentStatus.ToString(),
                                            Description = y.StudentStatus.LongDesc,
                                            StartDate = y.StartDate
                                        })
                                        .FirstOrDefault() :
                                        new GetDocumentRequestDetailResult_StudentStatus
                                        {
                                            Id = x.IdStudentStatus.ToString(),
                                            Description = x.LtStudentStatus.LongDesc,
                                            StartDate = null
                                        },
                                LevelWhenRequestWasMade = getHomeroomWhenRequestWasMadeRaw == null ? null : getHomeroomWhenRequestWasMadeRaw.Level,
                                HomeroomWhenRequestWasMade = getHomeroomWhenRequestWasMadeRaw == null ? null : getHomeroomWhenRequestWasMadeRaw.Homeroom,
                                LatestDocumentReqStatusWorkflow = x.DocumentReqStatusTrackingHistories
                                                            .Select(y => new GetDocumentRequestDetailResult_LatestDocumentReqStatusWorkflow
                                                            {
                                                                IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
                                                                Description = y.DocumentReqStatusWorkflow.ParentDescription + ((y.IsOnProcess && y.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.OnProcess) ? " (On Process)" : ""),
                                                                Remarks = y.Remarks,
                                                                IsOnProcess = y.IsOnProcess,
                                                                StatusDate = y.StatusDate
                                                            })
                                                            .OrderByDescending(y => y.StatusDate)
                                                            .FirstOrDefault(),
                                Approval = string.IsNullOrEmpty(x.IdBinusianApprover) ? null : new GetDocumentRequestDetailResult_Approval
                                {
                                    StaffApprover = getStaffApproverRaw,
                                    ApprovalStatus = x.ApprovalStatus,
                                    ApprovalRemarks = x.ApprovalRemarks
                                },
                                DocumentList = documentListFinal,
                                Payment = param.IncludePaymentInfo ? new GetDocumentRequestDetailResult_Payment
                                {
                                    PaymentStatus = getPaymentInfo.PaymentStatus,
                                    TotalAmountInvoice = getPaymentInfo.TotalAmountInvoice,
                                    EndDatePayment = getPaymentInfo.EndDatePayment,
                                    PaymentDate = getPaymentInfo.PaymentDate,
                                    DocumentReqPaymentMethod = getPaymentInfo.PaymentStatus == PaymentStatus.Free ? null : new GetDocumentRequestDetailResult_PaymentMethod
                                    {
                                        Id = getPaymentInfo.DocumentReqPaymentMethod.Id,
                                        Description = getPaymentInfo.DocumentReqPaymentMethod.Description,
                                        IsVirtualAccount = getPaymentInfo.IsVirtualAccount.HasValue ? getPaymentInfo.IsVirtualAccount : null,
                                        UsingManualVerification = getPaymentInfo.UsingManualVerification.HasValue ? getPaymentInfo.UsingManualVerification : null
                                    },
                                    PaidAmount = getPaymentInfo.PaidAmount,
                                    SenderAccountName = getPaymentInfo.SenderAccountName,
                                    AttachmentImageUrl = getPaymentInfo.AttachmentImageUrl
                                } : null,
                                CollectionInfo = x.DocumentReqApplicantCollections
                                .Select(y => new GetDocumentRequestDetailResult_CollectionInfo
                                {
                                    FinishDate = y.FinishDate,
                                    ScheduleCollectionDateStart = y.ScheduleCollectionDateStart ?? null,
                                    ScheduleCollectionDateEnd = y.ScheduleCollectionDateEnd ?? null,
                                    Remarks = y.Remarks ?? null,
                                    Venue = new ItemValueVm
                                    {
                                        Id = y.IdVenue,
                                        //Description = y.Venue.Description
                                    },
                                    CollectedBy = y.CollectedBy,
                                    CollectedDate = y.CollectedDate
                                })
                                .FirstOrDefault(),
                                //Configuration = new GetDocumentRequestDetailResult_Configuration
                                //{
                                //    CanApproveDocumentRequest = x.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForApproval,
                                //    CanVerifyPayment = getPaymentInfo.PaymentStatus == PaymentStatus.Unpaid &&
                                //    x.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.WaitingForApproval && 
                                //    x.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Canceled &&
                                //    x.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Declined,
                                //    CanFinishRequest = (x.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess && allDocumentIsReady) || (x.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished),
                                //    EnableDocumentIsReadyButton = x.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.OnProcess ,
                                //    SoftCopyDocumentOnly = softCopyDocumentOnly,
                                //    CanDeleteSoftCopy = !softCopyDocumentOnly && (x.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Finished && x.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Collected)
                                //}
                            })
                            .FirstOrDefaultAsync(CancellationToken);

            result.Configuration = new GetDocumentRequestDetailResult_Configuration
            {
                CanApproveDocumentRequest = result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForApproval,
                CanVerifyPayment = getPaymentInfo.PaymentStatus == PaymentStatus.Unpaid &&
                                    result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.WaitingForApproval &&
                                    result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Canceled &&
                                    result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Declined,
                CanFinishRequest = (result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess && allDocumentIsReady) || (result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished),
                EnableDocumentIsReadyButton = result.LatestDocumentReqStatusWorkflow.IsOnProcess,
                SoftCopyDocumentOnly = softCopyDocumentOnly,
                CanDeleteSoftCopy = !softCopyDocumentOnly && (result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Finished && result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.Collected)
            };

            if (result.CollectionInfo != null && result.CollectionInfo.Venue != null)
            {
                var getVenue = await _dbContext.Entity<MsVenue>()
                            .Where(x => x.Id == result.CollectionInfo.Venue.Id)
                            .FirstOrDefaultAsync(CancellationToken);

                result.CollectionInfo.Venue.Description = getVenue?.Description;
            };


            return result;
        }
    }
}
