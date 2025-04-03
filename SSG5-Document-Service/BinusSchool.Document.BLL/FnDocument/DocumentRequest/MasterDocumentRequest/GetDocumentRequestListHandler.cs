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
    public class GetDocumentRequestListHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[]
        { 
            "RequestNumber", 
            "RequestDate", 
            "IdStudent", 
            "StudentName", 
            "HomeroomWhenRequestWasMade", 
            "EstimatedFinishDate", 
            "TotalAmount", 
            "PaymentStatus", 
            "PaymentDate", 
            "LatestDocumentReqStatusWorkflow"
        };
        //private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        //{
        //    { _columns[0], "staff.staff.IdBinusian" },
        //    { _columns[1], "staff.staff.FirstName" }
        //};

        private readonly IDocumentDbContext _dbContext;
        private readonly GetDocumentRequestPaymentInfoHandler _getDocumentRequestPaymentInfoHandler;
        private readonly IMachineDateTime _dateTime;

        public GetDocumentRequestListHandler(
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
            var param = Request.ValidateParams<GetDocumentRequestListRequest>(
                nameof(GetDocumentRequestListRequest.IdSchool),
                nameof(GetDocumentRequestListRequest.RequestYear)
            );

            // Query base
            var query = _dbContext.Entity<MsDocumentReqApplicant>()
                .Include(x => x.Student)
                .Include(x => x.Parent)
                    .ThenInclude(x => x.ParentRole)
                .AsQueryable();

            // Filter berdasarkan IdSchool
            query = query.Where(x => x.IdSchool == param.IdSchool);

            // Filter berdasarkan RequestYear
            if (param.RequestYear == 1)
            {
                query = query.Where(x => x.RequestDate >= _dateTime.ServerTime.Date.AddYears(-1));
            }
            else
            {
                query = query.Where(x => x.RequestDate.Year == param.RequestYear);
            }

            // Filter berdasarkan IdDocumentReqType
            if (!string.IsNullOrEmpty(param.IdDocumentReqType))
            {
                query = query.Where(x => x.DocumentReqApplicantDetails
                    .Any(y => y.IdDocumentReqType == param.IdDocumentReqType));
            }

            // Filter berdasarkan ApprovalStatus
            if (param.ApprovalStatus.HasValue)
            {
                query = query.Where(x => x.ApprovalStatus == param.ApprovalStatus);
            }

            // Filter berdasarkan IdDocumentReqStatusWorkflow
            if (param.IdDocumentReqStatusWorkflow.HasValue)
            {
                var statusWorkflow = param.IdDocumentReqStatusWorkflow.Value;
                query = query.Where(x =>
                    statusWorkflow == DocumentRequestStatusWorkflow.OnProcess
                        ? (x.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess ||
                           x.DocumentReqStatusTrackingHistories
                            .OrderByDescending(y => y.StatusDate)
                            .FirstOrDefault().IsOnProcess == true)
                        : x.IdDocumentReqStatusWorkflow == statusWorkflow
                );
            }

            // Filter berdasarkan SearchQuery
            if (!string.IsNullOrEmpty(param.SearchQuery))
            {
                var searchQueryUpper = param.SearchQuery.ToUpper();
                query = query.Where(x => x.RequestNumber.ToUpper().Contains(searchQueryUpper) ||
                                         x.IdStudent.Contains(searchQueryUpper) ||
                                         ((string.IsNullOrEmpty(x.Student.FirstName) ? "" : (x.Student.FirstName.ToUpper())) +
                                          (string.IsNullOrEmpty(x.Student.MiddleName) ? "" : (" " + x.Student.MiddleName.ToUpper())) +
                                          (string.IsNullOrEmpty(x.Student.LastName) ? "" : (" " + x.Student.LastName.ToUpper()))).Contains(searchQueryUpper));
            }

            // Filter berdasarkan Payment Status FREE
            if (param.PaymentStatus.HasValue && param.PaymentStatus == PaymentStatus.Free)
            {
                query = query.Where(x => x.DocumentReqApplicantDetails.Sum(y => y.PriceInvoice) == 0);
            }

            // Ambil hasil dan proyeksikan hanya yang diperlukan
            var resultRaw = await query
                .Select(x => new
                {
                    IdDocumentReqApplicant = x.Id,
                    IdHomeroom = x.IdHomeroom,
                    RequestNumber = x.RequestNumber,
                    RequestDate = x.RequestDate,
                    RequestedBy = new NameValueVm
                    {
                        Id = x.IdParentApplicant,
                        Name = NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName) + " (" + x.Parent.ParentRole.ParentRoleNameEng + ")"
                    },
                    CreatedBy = x.CreatedBy,
                    Student = new NameValueVm
                    {
                        Id = x.IdStudent,
                        Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
                    },
                    StudentStatusWhenRequestWasCreated = new ItemValueVm
                    {
                        Id = x.IdStudentStatus.ToString(),
                        Description = x.LtStudentStatus.LongDesc
                    },
                    EstimationFinishDate = x.EstimationFinishDate,
                    TotalPrice = x.DocumentReqApplicantDetails.Sum(y => y.PriceInvoice * y.NoOfCopy),
                    DocumentList = x.DocumentReqApplicantDetails
                        .Select(y => new
                        {
                            Document = new ItemValueVm
                            {
                                Id = y.Id,
                                Description = y.DocumentReqType.Name
                            },
                            DocumentIsReady = y.ReceivedDateByStaff != null
                        })
                        .OrderBy(y => y.Document.Description)
                        .ToList(),
                    LatestDocumentReqStatusWorkflow = x.DocumentReqStatusTrackingHistories
                        .OrderByDescending(y => y.StatusDate)
                        .Select(y => new GetDocumentRequestListResult_StatusWorkflow
                        {
                            IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
                            Description = y.DocumentReqStatusWorkflow.ParentDescription +
                                          (y.IsOnProcess && y.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.OnProcess ? " (On Process)" : ""),
                            IsOnProcess = y.IsOnProcess,
                            StatusDate = y.StatusDate,
                            Remarks = y.Remarks
                        })
                        .FirstOrDefault(),
                    DocumentReqStatusWorkflowHistoryList = x.DocumentReqStatusTrackingHistories
                        .Select(y => new GetDocumentRequestListResult_StatusWorkflow
                        {
                            IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
                            Description = y.DocumentReqStatusWorkflow.ParentDescription +
                                          (y.IsOnProcess && y.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.OnProcess ? " (On Process)" : ""),
                            StatusDate = y.StatusDate,
                            Remarks = y.Remarks
                        })
                        .OrderByDescending(y => y.StatusDate)
                        .ToList(),
                    ApprovalStatus = x.ApprovalStatus
                })
                .ToListAsync(CancellationToken);

            // Ambil informasi homeroom
            var getIdHomeroomListRaw = resultRaw.Select(x => x.IdHomeroom).Distinct().ToList();
            var getHomeroomListRaw = await _dbContext.Entity<MsHomeroom>()
                .Include(x => x.Grade)
                .Include(x => x.GradePathwayClassroom)
                    .ThenInclude(x => x.Classroom)
                .Where(x => getIdHomeroomListRaw.Contains(x.Id))
                .Select(x => new ItemValueVm
                {
                    Id = x.Id,
                    Description = x.Grade.Description + x.GradePathwayClassroom.Classroom.Description
                })
                .ToListAsync(CancellationToken);

            // Ambil info collection
            var collectionInfoRawList = await _dbContext.Entity<MsDocumentReqApplicantCollection>()
                .Where(x => resultRaw.Select(y => y.IdDocumentReqApplicant).Contains(x.IdDocumentReqApplicant))
                .Select(x => new
                {
                    IdDocumentReqApplicant = x.IdDocumentReqApplicant,
                    FinishDate = x.FinishDate,
                    ScheduleCollectionDateStart = x.ScheduleCollectionDateStart,
                    ScheduleCollectionDateEnd = x.ScheduleCollectionDateEnd,
                    IdVenue = x.IdVenue,
                    CollectedBy = x.CollectedBy,
                    CollectedDate = x.CollectedDate
                })
                .ToListAsync(CancellationToken);

            // Info venue
            var getVenueRawList = await _dbContext.Entity<MsVenue>()
                .Where(x => collectionInfoRawList.Select(y => y.IdVenue).Contains(x.Id))
                .Select(y => new { y.Id, y.Description })
                .ToListAsync(CancellationToken);

            var collectionInfoRawFinalList = collectionInfoRawList
                .Select(x => new
                {
                    IdDocumentReqApplicant = x.IdDocumentReqApplicant,
                    FinishDate = x.FinishDate,
                    ScheduleCollectionDateText = x.ScheduleCollectionDateStart == null && x.ScheduleCollectionDateEnd == null ?
                        "Anytime" : $"{x.ScheduleCollectionDateStart:dd MMMM yyyy, HH:mm:ss} - {x.ScheduleCollectionDateEnd:dd MMMM yyyy, HH:mm:ss}",
                    Venue = string.IsNullOrEmpty(x.IdVenue) ? null : new ItemValueVm
                    {
                        Id = x.IdVenue,
                        Description = getVenueRawList.FirstOrDefault(y => y.Id == x.IdVenue)?.Description
                    },
                    CollectedBy = x.CollectedBy,
                    CollectedDate = x.CollectedDate
                })
                .ToList();

            // Info pembayaran
            var getPaymentInfo = await _getDocumentRequestPaymentInfoHandler.GetDocumentRequestPaymentInfo(new GetDocumentRequestPaymentInfoRequest
            {
                IdDocumentReqApplicantList = resultRaw.Select(x => x.IdDocumentReqApplicant).ToList()
            });

            // Mempersiapkan hasil akhir
            var resultFinal = resultRaw.GroupJoin(
                getPaymentInfo,
                result => result.IdDocumentReqApplicant,
                paymentInfo => paymentInfo.IdDocumentReqApplicant,
                (result, paymentInfo) => new { result, paymentInfo }
            )
            .SelectMany(x => x.paymentInfo.DefaultIfEmpty(),
                (result, paymentInfo) => new
                {
                    result.result,
                    paymentInfo
                })
                .Select(x => new GetDocumentRequestListResult
                {
                    IdDocumentReqApplicant = x.result.IdDocumentReqApplicant,
                    RequestNumber = x.result.RequestNumber,
                    RequestDate = x.result.RequestDate,
                    RequestedBy = x.result.RequestedBy,
                    CreatedBy = x.result.CreatedBy,
                    Student = x.result.Student,
                    HomeroomWhenRequestWasMade = getHomeroomListRaw.FirstOrDefault(y => y.Id == x.result.IdHomeroom),
                    StudentStatusWhenRequestWasCreated = x.result.StudentStatusWhenRequestWasCreated,
                    EstimationFinishDate = x.result.EstimationFinishDate,
                    DocumentList = x.result.DocumentList.Select(y => y.Document).ToList(),
                    LatestDocumentReqStatusWorkflow = x.result.LatestDocumentReqStatusWorkflow,
                    DocumentReqStatusWorkflowHistoryList = x.result.DocumentReqStatusWorkflowHistoryList
                        .Select(y => new GetDocumentRequestListResult_StatusWorkflow
                        {
                            IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
                            Description = y.Description,
                            IsOnProcess = y.IsOnProcess,
                            StatusDate = y.StatusDate,
                            Remarks = y.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.Finished ?
                                $"Collection Date Schedule: {collectionInfoRawFinalList.FirstOrDefault(z => z.IdDocumentReqApplicant == x.result.IdDocumentReqApplicant)?.ScheduleCollectionDateText}<br>" +
                                $"Venue: {collectionInfoRawFinalList.FirstOrDefault(z => z.IdDocumentReqApplicant == x.result.IdDocumentReqApplicant)?.Venue?.Description ?? "-"}<br>" +
                                $"Notes: {y.Remarks}" :
                                string.IsNullOrEmpty(y.Remarks) ? "-" : y.Remarks
                        })
                        .ToList(),
                    TotalAmountReal = x.paymentInfo?.TotalAmountReal ?? 0,
                    PaymentStatus = x.result.TotalPrice <= 0 ? PaymentStatus.Free : x.paymentInfo?.PaymentStatus ?? PaymentStatus.Unpaid,
                    PaymentDate = x.paymentInfo?.PaymentDate,
                    ApprovalStatus = x.result.ApprovalStatus,
                    Configuration = new GetDocumentRequestListResult_Configuration
                    {
                        CanApproveDocumentRequest = x.result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForApproval,
                        CanCancelRequest = !new[]
                        {
                    DocumentRequestStatusWorkflow.Finished,
                    DocumentRequestStatusWorkflow.Collected,
                    DocumentRequestStatusWorkflow.Canceled,
                    DocumentRequestStatusWorkflow.Declined
                        }.Contains(x.result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow) &&
                        !(x.paymentInfo == null && x.paymentInfo?.PaymentStatus == PaymentStatus.Expired),
                        CanEditRequest = !new[]
                        {
                    DocumentRequestStatusWorkflow.Finished,
                    DocumentRequestStatusWorkflow.Collected,
                    DocumentRequestStatusWorkflow.Canceled,
                    DocumentRequestStatusWorkflow.Declined
                        }.Contains(x.result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow),
                        CanManageSoftCopy = x.result.LatestDocumentReqStatusWorkflow.IsOnProcess ||
                                            new[]
                                            {
                                        DocumentRequestStatusWorkflow.Finished,
                                        DocumentRequestStatusWorkflow.Collected
                                            }.Contains(x.result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow),
                        CanFinishRequest = x.result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.OnProcess &&
                                           x.result.DocumentList.All(y => y.DocumentIsReady) ||
                                           new[]
                                           {
                                       DocumentRequestStatusWorkflow.Finished,
                                       DocumentRequestStatusWorkflow.Collected
                                           }.Contains(x.result.LatestDocumentReqStatusWorkflow.IdDocumentReqStatusWorkflow)
                    }
                });

            // Pagination dan pengembalian hasil
            var orderedResults = param.OrderBy switch
            {
                "RequestNumber" => param.OrderType == OrderType.Asc ? resultFinal.OrderBy(x => x.RequestNumber) : resultFinal.OrderByDescending(x => x.RequestNumber),
                "RequestDate" => param.OrderType == OrderType.Asc ? resultFinal.OrderBy(x => x.RequestDate) : resultFinal.OrderByDescending(x => x.RequestDate),
                "IdStudent" => param.OrderType == OrderType.Asc ? resultFinal.OrderBy(x => x.Student.Id) : resultFinal.OrderByDescending(x => x.Student.Id),
                "StudentName" => param.OrderType == OrderType.Asc ? resultFinal.OrderBy(x => x.Student.Name) : resultFinal.OrderByDescending(x => x.Student.Name),
                "HomeroomWhenRequestWasMade" => param.OrderType == OrderType.Asc ? resultFinal.OrderBy(x => x.HomeroomWhenRequestWasMade.Description) : resultFinal.OrderByDescending(x => x.HomeroomWhenRequestWasMade.Description),
                "EstimatedFinishDate" => param.OrderType == OrderType.Asc ? resultFinal.OrderBy(x => x.EstimationFinishDate) : resultFinal.OrderByDescending(x => x.EstimationFinishDate),
                "TotalAmount" => param.OrderType == OrderType.Asc ? resultFinal.OrderBy(x => x.TotalAmountReal) : resultFinal.OrderByDescending(x => x.TotalAmountReal),
                "PaymentStatus" => param.OrderType == OrderType.Asc ? resultFinal.OrderBy(x => x.PaymentStatus.GetDescription()) : resultFinal.OrderByDescending(x => x.PaymentStatus.GetDescription()),
                "PaymentDate" => param.OrderType == OrderType.Asc ? resultFinal.OrderBy(x => x.PaymentDate) : resultFinal.OrderByDescending(x => x.PaymentDate),
                "LatestDocumentReqStatusWorkflow" => param.OrderType == OrderType.Asc ? resultFinal.OrderBy(x => x.LatestDocumentReqStatusWorkflow.Description) : resultFinal.OrderByDescending(x => x.LatestDocumentReqStatusWorkflow.Description),
                _ => resultFinal.OrderBy(x => x.RequestNumber)
            };

            // Mengaplikasikan pagination pada hasil akhir
            var paginatedResults = orderedResults.SetPagination(param).ToList();
            var count = orderedResults.Count();

            return Request.CreateApiResult2(paginatedResults as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
