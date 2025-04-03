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
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class GetPaymentRecapListHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly GetDocumentRequestPaymentInfoHandler _getDocumentRequestPaymentInfoHandler;
        private readonly IMachineDateTime _dateTime;

        public GetPaymentRecapListHandler(
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
            var param = Request.ValidateParams<GetPaymentRecapListRequest>(
                            nameof(GetPaymentRecapListRequest.IdSchool),
                            nameof(GetPaymentRecapListRequest.PaymentPeriodStartDate),
                            nameof(GetPaymentRecapListRequest.PaymentPeriodEndDate));

            var result = await GetPaymentRecapList(param);

            return Request.CreateApiResult2(result as object);
        }

        public async Task<List<GetPaymentRecapListResult>> GetPaymentRecapList(GetPaymentRecapListRequest param)
        {
            var getPaidManualIdDocumentReqApplicantList = await _dbContext.Entity<TrDocumentReqPaymentMapping>()
                                                    .Include(x => x.DocumentReqPaymentManual)
                                                    .Where(x => x.DocumentReqPaymentManual.PaymentStatus == 1 &&
                                                                x.DocumentReqApplicant.IdSchool == param.IdSchool &&
                                                                (x.DocumentReqPaymentManual.PaymentDate == null ? false : x.DocumentReqPaymentManual.PaymentDate.Value.Date >= param.PaymentPeriodStartDate.Date) &&
                                                                (x.DocumentReqPaymentManual.PaymentDate == null ? false : x.DocumentReqPaymentManual.PaymentDate.Value.Date <= param.PaymentPeriodEndDate.Date) &&

                                                                // Paid document
                                                                x.DocumentReqApplicant.DocumentReqApplicantDetails.Sum(y => y.PriceInvoice) >= 0)
                                                    .Select(x => x.IdDocumentReqApplicant)
                                                    .ToListAsync(CancellationToken);

            var getDocumentReqApplicantListRaw = await _dbContext.Entity<MsDocumentReqApplicant>()
                            .Where(x => getPaidManualIdDocumentReqApplicantList.Any(y => y == x.Id))
                            .Select(x => new
                            {
                                IdDocumentReqApplicant = x.Id,
                                IdStudent = x.IdStudent
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
                            .Where(x => getPaidManualIdDocumentReqApplicantList.Any(y => y == x.Id))
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
                                    .Select(y => new GetPaymentRecapListResult_StudentStatus
                                    {
                                        Id = y.IdStudentStatus.ToString(),
                                        Description = y.StudentStatus.LongDesc,
                                        StartDate = y.StartDate
                                    })
                                    .FirstOrDefault() :
                                    new GetPaymentRecapListResult_StudentStatus
                                    {
                                        Id = x.IdStudentStatus.ToString(),
                                        Description = x.LtStudentStatus.LongDesc,
                                        StartDate = null
                                    },
                                EstimationFinishDate = x.EstimationFinishDate,
                                LatestDocumentReqStatusWorkflow = x.DocumentReqStatusTrackingHistories
                                    .Select(y => new GetPaymentRecapListResult_StatusWorkflow
                                    {
                                        IdDocumentReqStatusWorkflow = y.IdDocumentReqStatusWorkflow,
                                        Description = y.DocumentReqStatusWorkflow.ParentDescription + ((y.IsOnProcess && y.IdDocumentReqStatusWorkflow != DocumentRequestStatusWorkflow.OnProcess) ? " (On Process)" : ""),
                                        IsOnProcess = y.IsOnProcess,
                                        StatusDate = y.StatusDate
                                    })
                                    .OrderByDescending(y => y.StatusDate)
                                    .FirstOrDefault(),
                                DocumentList = x.DocumentReqApplicantDetails,
                                TotalPrice = x.DocumentReqApplicantDetails.Sum(y => y.PriceInvoice * y.NoOfCopy),
                                IdBinusianApprover = x.IdBinusianApprover,
                                ApprovalStatus = x.ApprovalStatus,
                                ApprovalRemarks = x.ApprovalRemarks
                                //Approval = string.IsNullOrEmpty(x.IdBinusianApprover) ? null : new GetPaymentRecapListResult_Approval
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
                            .Select(x => new GetPaymentRecapListResult
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
                                TotalAmountReal = x.paymentInfo == null ? 0 : x.paymentInfo.TotalAmountReal,
                                PaymentStatus = x.result.result.TotalPrice <= 0 ? PaymentStatus.Free : x.paymentInfo == null ? PaymentStatus.Unpaid : x.paymentInfo.PaymentStatus,
                                DocumentList = x.result.result.DocumentList
                                    .Select(y => new GetPaymentRecapListResult_Document
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
                                        NeedHardCopy = y.NeedHardCopy,
                                        NeedSoftCopy = y.NeedSoftCopy,
                                        DocumentIsReady = new GetPaymentRecapListResult_DocumentIsReady
                                        {
                                            ReceivedDateByStaff = y.ReceivedDateByStaff == null ? null : y.ReceivedDateByStaff
                                        },
                                    })
                                    .OrderBy(y => y.DocumentName)
                                    .ToList(),
                                Approval = string.IsNullOrEmpty(x.result.result.IdBinusianApprover) ? null : new GetPaymentRecapListResult_Approval
                                {
                                    StaffApprover = getStaffApproverRaw
                                                        .Where(y => y.IdDocumentReqApplicant == x.result.result.IdDocumentReqApplicant)
                                                        .Select(y => y.Staff)
                                                        .FirstOrDefault(),
                                    ApprovalStatus = x.result.result.ApprovalStatus,
                                    ApprovalRemarks = x.result.result.ApprovalRemarks
                                },
                                Payment = x.paymentInfo == null ? null : new GetPaymentRecapListResult_Payment
                                {
                                    PaymentStatus = x.paymentInfo.PaymentStatus,
                                    TotalAmountInvoice = x.paymentInfo.TotalAmountInvoice,
                                    EndDatePayment = x.paymentInfo.EndDatePayment,
                                    PaymentDate = x.paymentInfo.PaymentDate,
                                    DocumentReqPaymentMethod = new GetPaymentRecapListResult_PaymentMethod
                                    {
                                        Id = x.paymentInfo.DocumentReqPaymentMethod?.Id,
                                        Description = x.paymentInfo.DocumentReqPaymentMethod?.Description,
                                        IsVirtualAccount = x.paymentInfo.IsVirtualAccount,
                                        UsingManualVerification = x.paymentInfo.UsingManualVerification
                                    },
                                    PaidAmount = x.paymentInfo.PaidAmount,
                                    SenderAccountName = x.paymentInfo.SenderAccountName,
                                    AttachmentImageUrl = x.paymentInfo.AttachmentImageUrl,
                                    PaymentVerificationDate = x.paymentInfo.PaymentVerificationDate
                                }
                            })
                            .OrderBy(x => x.Payment.PaymentDate)
                            .ToList();

            return result;
        }
    }
}
