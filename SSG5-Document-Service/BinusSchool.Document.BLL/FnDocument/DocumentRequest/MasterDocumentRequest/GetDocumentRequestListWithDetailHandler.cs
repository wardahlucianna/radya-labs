using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
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
    public class GetDocumentRequestListWithDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly GetDocumentRequestPaymentInfoHandler _getDocumentRequestPaymentInfoHandler;
        private readonly IMachineDateTime _dateTime;

        public GetDocumentRequestListWithDetailHandler(
            IDocumentDbContext dbContext,
            GetDocumentRequestPaymentInfoHandler getDocumentRequestPaymentInfoHandler,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _getDocumentRequestPaymentInfoHandler = getDocumentRequestPaymentInfoHandler;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDocumentRequestListWithDetailRequest>(
                            nameof(GetDocumentRequestListWithDetailRequest.IdSchool),
                            nameof(GetDocumentRequestListWithDetailRequest.RequestYear));

            var result = await GetDocumentRequestListWithDetail(param);

            return Request.CreateApiResult2(result as object);
        }

        public async Task<List<GetDocumentRequestListWithDetailResult>> GetDocumentRequestListWithDetail(GetDocumentRequestListWithDetailRequest param)
        {
            var getDocumentReqApplicantListRaw = await _dbContext.Entity<MsDocumentReqApplicant>()
                            .Where(x => x.IdSchool == param.IdSchool &&
                                        (
                                            param.RequestYear == 1 ?
                                                // within last one year
                                                x.RequestDate >= _dateTime.ServerTime.Date.AddYears(-1)

                                                // specific year
                                                : x.RequestDate.Year == param.RequestYear
                                        ) &&
                                        (string.IsNullOrEmpty(param.IdDocumentReqType) ? true : x.DocumentReqApplicantDetails.Select(y => y.IdDocumentReqType).Any(y => y == param.IdDocumentReqType)) &&
                                        (param.ApprovalStatus == null ? true :
                                            param.ApprovalStatus == x.ApprovalStatus) &&
                                        (param.IdDocumentReqStatusWorkflow == null ? true : param.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess ? (x.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess || x.DocumentReqStatusTrackingHistories.OrderByDescending(y => y.StatusDate).FirstOrDefault().IsOnProcess == true) : x.IdDocumentReqStatusWorkflow == param.IdDocumentReqStatusWorkflow) &&

                                        // Filter SearchQuery
                                        (
                                            string.IsNullOrEmpty(param.SearchQuery) ? true :
                                            (
                                                // by RequestNumber
                                                x.RequestNumber.ToUpper().Contains(param.SearchQuery.ToUpper()) ||
                                                // by IdStudent
                                                x.IdStudent.Contains(param.SearchQuery) ||

                                                // by StudentName
                                                ((string.IsNullOrEmpty(x.Student.FirstName) ? "" : (x.Student.FirstName.ToUpper())) +
                                                (string.IsNullOrEmpty(x.Student.MiddleName) ? "" : (" " + x.Student.MiddleName.ToUpper())) +
                                                (string.IsNullOrEmpty(x.Student.LastName) ? "" : (" " + x.Student.LastName.ToUpper()))).Contains(param.SearchQuery.ToUpper())
                                            )
                                        ) &&

                                        // Filter Payment Status FREE
                                        (param.PaymentStatus == null ? true :
                                            param.PaymentStatus == PaymentStatus.Free ? x.DocumentReqApplicantDetails.Sum(y => y.PriceInvoice) == 0 : true
                                        )
                            )
                            .Select(x => new
                            {
                                IdDocumentReqApplicant = x.Id,
                                IdStudent = x.IdStudent
                            })
                            .ToListAsync(CancellationToken);

            var getAcademicYearDocumentRawList = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                            .Include(x => x.AcademicYear)
                                                            .Where(x => getDocumentReqApplicantListRaw.Select(y => y.IdDocumentReqApplicant).Any(y => y == x.IdDocumentReqApplicant) &&
                                                                        !string.IsNullOrEmpty(x.IdAcademicYearDocument))
                                                            .Select(x => new
                                                            {
                                                                IdDocumentReqApplicantDetail = x.Id,
                                                                AcademicYear = new ItemValueVm
                                                                {
                                                                    Id = x.IdAcademicYearDocument,
                                                                    Description = x.AcademicYear.Description
                                                                }
                                                            })
                                                            .ToListAsync(CancellationToken);

            var getGradeDocumentRawList = await _dbContext.Entity<MsHomeroomStudent>()
                                                .Include(x => x.Homeroom)
                                                    .ThenInclude(x => x.Grade)
                                                    .ThenInclude(x => x.Level)
                                                .Include(x => x.Homeroom)
                                                    .ThenInclude(x => x.GradePathwayClassroom)
                                                    .ThenInclude(x => x.Classroom)
                                                .Where(x => getDocumentReqApplicantListRaw.Select(y => y.IdStudent).Any(y => y == x.IdStudent) &&
                                                            getAcademicYearDocumentRawList.Select(y => y.AcademicYear.Id).Any(y => y == x.Homeroom.Grade.Level.IdAcademicYear))
                                                .Select(x => new
                                                {
                                                    IdStudent = x.IdStudent,
                                                    IdAcademicYear = x.Homeroom.Grade.Level.IdAcademicYear,
                                                    Grade = new ItemValueVm
                                                    {
                                                        Id = x.Homeroom.IdGrade,
                                                        Description = x.Homeroom.Grade.Description
                                                    },
                                                    HomeroomName = x.Homeroom.Grade.Description + " " + x.Homeroom.GradePathwayClassroom.Classroom.Description
                                                })
                                                .Distinct()
                                                .ToListAsync(CancellationToken);

            var getPeriodDocumentRawList = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                            .Include(x => x.Period)
                                                            .Where(x => getDocumentReqApplicantListRaw.Select(y => y.IdDocumentReqApplicant).Any(y => y == x.IdDocumentReqApplicant) &&
                                                                        !string.IsNullOrEmpty(x.IdPeriodDocument))
                                                            .Select(x => new
                                                            {
                                                                IdDocumentReqApplicantDetail = x.Id,
                                                                Period = new ItemValueVm
                                                                {
                                                                    Id = x.IdPeriodDocument,
                                                                    Description = x.Period.Description
                                                                }
                                                            })
                                                            .ToListAsync(CancellationToken);

            var getBinusianReceiverRawList = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                                .Include(x => x.Staff)
                                                .Where(x => getDocumentReqApplicantListRaw.Select(y => y.IdDocumentReqApplicant).Any(y => y == x.IdDocumentReqApplicant) &&
                                                            !string.IsNullOrEmpty(x.Staff.IdBinusian))
                                                .Select(x => new
                                                {
                                                    IdDocumentReqApplicantDetail = x.Id,
                                                    Staff = new NameValueVm
                                                    {
                                                        Id = x.Staff.IdBinusian,
                                                        Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                                                    }
                                                })
                                                .ToListAsync(CancellationToken);

            var getHomeroomWhenRequestWasMadeRaw = await _dbContext.Entity<MsDocumentReqApplicant>()
                                                    .Include(x => x.Homeroom)
                                                        .ThenInclude(x => x.GradePathwayClassroom)
                                                        .ThenInclude(x => x.Classroom)
                                                    .Include(x => x.Homeroom)
                                                        .ThenInclude(x => x.Grade)
                                                    .Where(x => getDocumentReqApplicantListRaw.Select(y => y.IdDocumentReqApplicant).Any(y => y == x.Id) &&
                                                                !string.IsNullOrEmpty(x.IdHomeroom))
                                                    .Select(x => new
                                                    {
                                                        IdDocumentReqApplicant = x.Id,
                                                        Homeroom = new ItemValueVm
                                                        {
                                                            Id = x.IdHomeroom,
                                                            Description = x.Homeroom.Grade.Description + x.Homeroom.GradePathwayClassroom.Classroom.Description
                                                        }
                                                    })
                                                    .ToListAsync(CancellationToken);

            var getStaffApproverRaw = await _dbContext.Entity<MsDocumentReqApplicant>()
                                            .Include(x => x.Staff)
                                            .Where(x => getDocumentReqApplicantListRaw.Select(y => y.IdDocumentReqApplicant).Any(y => y == x.Id) &&
                                                        !string.IsNullOrEmpty(x.IdBinusianApprover))
                                            .Select(x => new
                                            {
                                                IdDocumentReqApplicant = x.Id,
                                                Staff = new NameValueVm
                                                {
                                                    Id = x.IdBinusianApprover,
                                                    Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                                                }
                                            })
                                            .ToListAsync(CancellationToken);

            var additionalFieldsRaw = await _dbContext.Entity<TrDocumentReqApplicantDetail>()
                                .Include(x => x.DocumentReqApplicantFormAnswers)
                                    .ThenInclude(x => x.DocumentReqFormFieldAnswered)
                                .Where(x => getDocumentReqApplicantListRaw.Select(y => y.IdDocumentReqApplicant).Any(y => y == x.IdDocumentReqApplicant))
                                .Select(x => new
                                {
                                    IdDocumentReqApplicantDetail = x.Id,
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
                                .ToListAsync(CancellationToken);

            var resultRaw = await _dbContext.Entity<MsDocumentReqApplicant>()
                            .Include(x => x.Student)
                                .ThenInclude(x => x.TrStudentStatuss)
                            .Include(x => x.DocumentReqApplicantDetails)
                                .ThenInclude(x => x.DocumentReqType)
                            .Include(x => x.DocumentReqApplicantDetails)
                                .ThenInclude(x => x.DocumentReqAttachments)
                            .Include(x => x.DocumentReqApplicantDetails)
                                .ThenInclude(x => x.DocumentReqPICs)
                                .ThenInclude(x => x.Staff)
                            .Include(x => x.DocumentReqStatusTrackingHistories)
                                .ThenInclude(x => x.DocumentReqStatusWorkflow)
                            .Include(x => x.Parent)
                                .ThenInclude(x => x.ParentRole)
                            .Where(x => x.IdSchool == param.IdSchool &&
                                        (
                                            param.RequestYear == 1 ?
                                                // within last one year
                                                x.RequestDate >= _dateTime.ServerTime.Date.AddYears(-1)

                                                // specific year
                                                : x.RequestDate.Year == param.RequestYear
                                        ) &&
                                        (string.IsNullOrEmpty(param.IdDocumentReqType) ? true : x.DocumentReqApplicantDetails.Select(y => y.IdDocumentReqType).Any(y => y == param.IdDocumentReqType)) &&
                                        (param.ApprovalStatus == null ? true :
                                            param.ApprovalStatus == x.ApprovalStatus) &&
                                        (param.IdDocumentReqStatusWorkflow == null ? true : param.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess ? (x.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess || x.DocumentReqStatusTrackingHistories.OrderByDescending(y => y.StatusDate).FirstOrDefault().IsOnProcess == true) : x.IdDocumentReqStatusWorkflow == param.IdDocumentReqStatusWorkflow) &&

                                        // Filter SearchQuery
                                        (
                                            string.IsNullOrEmpty(param.SearchQuery) ? true :
                                            (
                                                // by RequestNumber
                                                x.RequestNumber.ToUpper().Contains(param.SearchQuery.ToUpper()) ||
                                                // by IdStudent
                                                x.IdStudent.Contains(param.SearchQuery) ||

                                                // by StudentName
                                                ((string.IsNullOrEmpty(x.Student.FirstName) ? "" : (x.Student.FirstName.ToUpper())) +
                                                (string.IsNullOrEmpty(x.Student.MiddleName) ? "" : (" " + x.Student.MiddleName.ToUpper())) +
                                                (string.IsNullOrEmpty(x.Student.LastName) ? "" : (" " + x.Student.LastName.ToUpper()))).Contains(param.SearchQuery.ToUpper())
                                            )
                                        ) &&

                                        // Filter Payment Status FREE
                                        (param.PaymentStatus == null ? true :
                                            param.PaymentStatus == PaymentStatus.Free ? x.DocumentReqApplicantDetails.Sum(y => y.PriceInvoice) == 0 : true
                                        )
                            )
                            .Select(x => new
                            {
                                IdDocumentReqApplicant = x.Id,
                                IdHomeroom = x.IdHomeroom,
                                RequestNumber = x.RequestNumber,
                                RequestDate = x.RequestDate,
                                RequestedBy = new NameValueVm
                                {
                                    Id = x.IdParentApplicant,
                                    Name = NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName) + " (" + x.Parent.ParentRole.ParentRoleNameEng + ")",
                                },
                                CreatedBy = x.CreatedBy,
                                Student = new NameValueVm
                                {
                                    Id = x.IdStudent,
                                    Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
                                },
                                //HomeroomWhenRequestWasMade = getHomeroomWhenRequestWasMadeRaw
                                //                                .Where(y => x.Id == y.IdDocumentReqApplicant)
                                //                                .Select(y => y.Homeroom)
                                //                                .FirstOrDefault(),
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
                                    .Select(y => new GetDocumentRequestListWithDetailResult_StudentStatus
                                    {
                                        Id = y.IdStudentStatus.ToString(),
                                        Description = y.StudentStatus.LongDesc,
                                        StartDate = y.StartDate
                                    })
                                    .FirstOrDefault() :
                                    new GetDocumentRequestListWithDetailResult_StudentStatus
                                    {
                                        Id = x.IdStudentStatus.ToString(),
                                        Description = x.LtStudentStatus.LongDesc,
                                        StartDate = null
                                    },
                                EstimationFinishDate = x.EstimationFinishDate,
                                LatestDocumentReqStatusWorkflow = x.DocumentReqStatusTrackingHistories
                                    .Select(y => new GetDocumentRequestListWithDetailResult_StatusWorkflow
                                    {
                                        IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
                                        Description = y.DocumentReqStatusWorkflow.ParentDescription + ((y.IsOnProcess && y.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.OnProcess) ? " (On Process)" : ""),
                                        IsOnProcess = y.IsOnProcess,
                                        StatusDate = y.StatusDate,
                                        Remarks = y.Remarks
                                    })
                                    .OrderByDescending(y => y.StatusDate)
                                    .FirstOrDefault(),
                                DocumentReqStatusWorkflowHistoryList = x.DocumentReqStatusTrackingHistories
                                    .Select(y => new GetDocumentRequestListWithDetailResult_StatusWorkflow
                                    {
                                        IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
                                        Description = y.DocumentReqStatusWorkflow.ParentDescription + ((y.IsOnProcess && y.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.OnProcess) ? " (On Process)" : ""),
                                        StatusDate = y.StatusDate,
                                        Remarks = y.Remarks
                                    })
                                    .OrderByDescending(y => y.StatusDate)
                                    .ToList(),
                                DocumentList = x.DocumentReqApplicantDetails,
                                TotalPrice = x.DocumentReqApplicantDetails.Sum(y => y.PriceInvoice * y.NoOfCopy),
                                IdBinusianApprover = x.IdBinusianApprover,
                                ApprovalStatus = x.ApprovalStatus,
                                ApprovalRemarks = x.ApprovalRemarks
                                //Approval = string.IsNullOrEmpty(x.IdBinusianApprover) ? null : new GetDocumentRequestListWithDetailResult_Approval
                                //{
                                //    StaffApprover = getStaffApproverRaw
                                //                        .Where(y => y.IdDocumentReqApplicant == x.Id)
                                //                        .Select(y => y.Staff)
                                //                        .FirstOrDefault(),
                                //    ApprovalStatus = x.ApprovalStatus,
                                //    ApprovalRemarks = x.ApprovalRemarks
                                //},
                            })
                            .ToListAsync(CancellationToken);

            // collection info
            var collectionInfoRawList = await _dbContext.Entity<MsDocumentReqApplicantCollection>()
                                            .Where(x => resultRaw.Select(y => y.IdDocumentReqApplicant).Any(y => y == x.IdDocumentReqApplicant))
                                            .Select(x => new
                                            {
                                                IdDocumentReqApplicant = x.IdDocumentReqApplicant,
                                                FinishDate = x.FinishDate,
                                                ScheduleCollectionDateStart = x.ScheduleCollectionDateStart ?? null,
                                                ScheduleCollectionDateEnd = x.ScheduleCollectionDateEnd ?? null,
                                                IdVenue = x.IdVenue ?? null,
                                                CollectedBy = x.CollectedBy ?? null,
                                                CollectedDate = x.CollectedDate ?? null
                                            })
                                            .ToListAsync(CancellationToken);

            var getVenueRawList = await _dbContext.Entity<MsVenue>()
                                    .Where(x => collectionInfoRawList.Select(y => y.IdVenue).Any(y => y == x.Id))
                                    .ToListAsync(CancellationToken);

            var collectionInfoRawFinalList = collectionInfoRawList
                                                .Select(x => new
                                                {
                                                    IdDocumentReqApplicant = x.IdDocumentReqApplicant,
                                                    FinishDate = x.FinishDate,
                                                    ScheduleCollectionDateStart = x.ScheduleCollectionDateStart,
                                                    ScheduleCollectionDateEnd = x.ScheduleCollectionDateEnd,
                                                    ScheduleCollectionDateText = x.ScheduleCollectionDateStart == null && x.ScheduleCollectionDateEnd == null ? "Anytime" : (x.ScheduleCollectionDateStart.Value.ToString("dd MMMM yyyy, HH:mm:ss") + " - " + x.ScheduleCollectionDateEnd.Value.ToString("dd MMMM yyyy, HH:mm:ss")),
                                                    Venue = string.IsNullOrEmpty(x.IdVenue) ? null : new ItemValueVm
                                                    {
                                                        Id = x.IdVenue,
                                                        Description = getVenueRawList
                                                                        .Where(y => y.Id == x.IdVenue)
                                                                        .Select(y => y.Description)
                                                                        .FirstOrDefault()
                                                    },
                                                    CollectedBy = x.CollectedBy,
                                                    CollectedDate = x.CollectedDate
                                                })
                                                .ToList();

            var getPaymentInfo = await _getDocumentRequestPaymentInfoHandler.GetDocumentRequestPaymentInfo(new GetDocumentRequestPaymentInfoRequest
            {
                IdDocumentReqApplicantList = resultRaw.Select(x => x.IdDocumentReqApplicant).ToList()
            });

            var resultRawJoinPaymentInfo = resultRaw
                                            .GroupJoin(
                                                getPaymentInfo,
                                                result => result.IdDocumentReqApplicant,
                                                paymentInfo => paymentInfo.IdDocumentReqApplicant,
                                                (result, paymentInfo) => new { result, paymentInfo }
                                            )
                                            .SelectMany(x =>
                                                x.paymentInfo.DefaultIfEmpty(),
                                                (result, paymentInfo) => new { result, paymentInfo }
                                            );

            var result = resultRawJoinPaymentInfo
                            .Where(x => param.PaymentStatus == null ? true :
                                        param.PaymentStatus == x.paymentInfo?.PaymentStatus
                            )
                            .Select(x => new GetDocumentRequestListWithDetailResult
                            {
                                IdDocumentReqApplicant = x.result.result.IdDocumentReqApplicant,
                                RequestNumber = x.result.result.RequestNumber,
                                RequestDate = x.result.result.RequestDate,
                                RequestedBy = x.result.result.RequestedBy,
                                CreatedBy = x.result.result.CreatedBy,
                                Student = x.result.result.Student,
                                HomeroomWhenRequestWasMade = getHomeroomWhenRequestWasMadeRaw
                                                                .Where(y => x.result.result.IdDocumentReqApplicant == y.IdDocumentReqApplicant)
                                                                .Select(y => y.Homeroom)
                                                                .FirstOrDefault(),
                                StudentStatusWhenRequestWasCreated = x.result.result.StudentStatusWhenRequestWasCreated,
                                EstimationFinishDate = x.result.result.EstimationFinishDate,
                                LatestDocumentReqStatusWorkflow = x.result.result.LatestDocumentReqStatusWorkflow,
                                DocumentReqStatusWorkflowHistoryList = x.result.result.DocumentReqStatusWorkflowHistoryList.Select(y => new GetDocumentRequestListWithDetailResult_StatusWorkflow
                                {
                                    IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
                                    Description = y.Description,
                                    IsOnProcess = y.IsOnProcess,
                                    StatusDate = y.StatusDate,
                                    Remarks = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished ? string.Format("Collection Date Schedule: {0}\n" +
                                                                "Venue: {1}\n" +
                                                                "Notes: {2}",
                                                                collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.result.result.IdDocumentReqApplicant).Select(z => z.ScheduleCollectionDateText).FirstOrDefault(),
                                                                collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.result.result.IdDocumentReqApplicant).Select(z => z.Venue == null ? "-" : z.Venue.Description).FirstOrDefault(),
                                                                string.IsNullOrEmpty(y.Remarks) ? "-" : y.Remarks
                                                                ) :

                                                                y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Collected ? string.Format("Collected By: {0}\n" +
                                                                "Collected Date: {1}\n" +
                                                                "Notes: {2}",
                                                                collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.result.result.IdDocumentReqApplicant).Select(z => z.CollectedBy).FirstOrDefault(),
                                                                collectionInfoRawFinalList.Where(z => z.IdDocumentReqApplicant == x.result.result.IdDocumentReqApplicant).Select(z => z.CollectedDate == null ? "-" : z.CollectedDate.Value.ToString("dd MMMM yyyy, HH:mm:ss")).FirstOrDefault(),
                                                                string.IsNullOrEmpty(y.Remarks) ? "-" : y.Remarks
                                                                ) :
                                                                string.IsNullOrEmpty(y.Remarks) ? "-" : y.Remarks
                                })
                                .ToList(),
                                TotalAmountReal = x.paymentInfo == null ? 0 : x.paymentInfo.TotalAmountReal,
                                PaymentStatus = x.result.result.TotalPrice <= 0 ? PaymentStatus.Free : x.paymentInfo == null ? PaymentStatus.Unpaid : x.paymentInfo.PaymentStatus,
                                DocumentList = x.result.result.DocumentList
                                    .Select(y => new GetDocumentRequestListWithDetailResult_Document
                                    {
                                        IdDocumentReqApplicantDetail = y.Id,
                                        IdDocumentReqType = y.IdDocumentReqType,
                                        DocumentName = y.DocumentReqType.Name,
                                        NoOfPages = y.NoOfPages,
                                        NoOfCopy = y.NoOfCopy,
                                        PriceReal = y.PriceReal,
                                        PriceInvoice = y.PriceInvoice,
                                        IsAcademicDocument = y.DocumentReqType.IsAcademicDocument,
                                        IsMadeFree = y.PriceReal != y.PriceInvoice ? true : false,
                                        AcademicYearDocument = getAcademicYearDocumentRawList
                                                                .Where(z => z.IdDocumentReqApplicantDetail == y.Id)
                                                                .Select(z => z.AcademicYear)
                                                                .FirstOrDefault(),
                                        GradeDocument = getGradeDocumentRawList
                                                                .Where(z => z.IdAcademicYear == y.IdAcademicYearDocument &&
                                                                            z.IdStudent == x.result.result.Student.Id)
                                                                .Select(z => z.Grade)
                                                                .FirstOrDefault(),
                                        HomeroomName = getGradeDocumentRawList
                                                                .Where(z => z.IdAcademicYear == y.IdAcademicYearDocument &&
                                                                            z.IdStudent == x.result.result.Student.Id)
                                                                .Select(z => z.HomeroomName)
                                                                .FirstOrDefault(),
                                        PeriodDocument = getPeriodDocumentRawList
                                                            .Where(z => z.IdDocumentReqApplicantDetail == y.Id)
                                                            .Select(z => z.Period)
                                                            .FirstOrDefault(),
                                        NeedHardCopy = y.NeedHardCopy,
                                        NeedSoftCopy = y.NeedSoftCopy,
                                        DocumentIsReady = new GetDocumentRequestListWithDetailResult_DocumentIsReady
                                        {
                                            BinusianReceiver = getBinusianReceiverRawList
                                                                .Where(z => z.IdDocumentReqApplicantDetail == y.Id)
                                                                .Select(z => z.Staff)
                                                                .FirstOrDefault(),
                                            ReceivedDateByStaff = y.ReceivedDateByStaff == null ? null : y.ReceivedDateByStaff
                                        },
                                        PICList = y.DocumentReqPICs == null || y.DocumentReqPICs.Count == 0 ? null :
                                                y.DocumentReqPICs
                                                .Select(y => new NameValueVm
                                                {
                                                    Id = y.IdBinusian,
                                                    Name = y.Staff.FirstName.Trim() + (string.IsNullOrEmpty(y.Staff.LastName) ? "" : (" " + y.Staff.LastName.Trim()))
                                                })
                                                .OrderBy(x => x.Name)
                                                .ToList(),
                                        AdditionalFieldList = additionalFieldsRaw
                                                                .Where(z => z.IdDocumentReqApplicantDetail == y.Id)
                                                                .Select(z => z.AdditionalFieldsRaw)
                                                                .FirstOrDefault() == null ?
                                                                null :
                                                                additionalFieldsRaw
                                                                .Where(z => z.IdDocumentReqApplicantDetail == y.Id)
                                                                .Select(z => z.AdditionalFieldsRaw)
                                                                .FirstOrDefault()
                                                                .GroupBy(z => new
                                                                {
                                                                    z.IdDocumentReqFormFieldAnswered,
                                                                    z.QuestionDescription,
                                                                    z.OrderNo
                                                                })
                                                                .Select(z => new GetDocumentRequestListWithDetailResult_AdditionalField
                                                                {
                                                                    IdDocumentReqFormFieldAnswered = z.Key.IdDocumentReqFormFieldAnswered,
                                                                    QuestionDescription = z.Key.QuestionDescription,
                                                                    OrderNo = z.Key.OrderNo,
                                                                    AnswerTextList = z.Select(z => z.AnswerText).ToList()
                                                                })
                                                                .OrderBy(z => z.OrderNo)
                                                                .ToList()
                                    })
                                    .OrderBy(y => y.DocumentName)
                                    .ToList(),
                                Approval = string.IsNullOrEmpty(x.result.result.IdBinusianApprover) ? null : new GetDocumentRequestListWithDetailResult_Approval
                                {
                                    StaffApprover = getStaffApproverRaw
                                                        .Where(y => y.IdDocumentReqApplicant == x.result.result.IdDocumentReqApplicant)
                                                        .Select(y => y.Staff)
                                                        .FirstOrDefault(),
                                    ApprovalStatus = x.result.result.ApprovalStatus,
                                    ApprovalRemarks = x.result.result.ApprovalRemarks
                                },
                                Payment = x.paymentInfo == null ? null : new GetDocumentRequestListWithDetailResult_Payment
                                {
                                    PaymentStatus = x.paymentInfo.PaymentStatus,
                                    TotalAmountInvoice = x.paymentInfo.TotalAmountInvoice,
                                    EndDatePayment = x.paymentInfo.EndDatePayment,
                                    PaymentDate = x.paymentInfo.PaymentDate,
                                    DocumentReqPaymentMethod = new GetDocumentRequestListWithDetailResult_PaymentMethod
                                    {
                                        Id = x.paymentInfo.DocumentReqPaymentMethod?.Id,
                                        Description = x.paymentInfo.DocumentReqPaymentMethod?.Description,
                                        IsVirtualAccount = x.paymentInfo.IsVirtualAccount,
                                        UsingManualVerification = x.paymentInfo.UsingManualVerification
                                    },
                                    PaidAmount = x.paymentInfo.PaidAmount,
                                    SenderAccountName = x.paymentInfo.SenderAccountName,
                                    AttachmentImageUrl = x.paymentInfo.AttachmentImageUrl
                                }
                            })
                            .OrderBy(x => x.RequestNumber)
                            .ToList();

            return result;
        }
    }
}
