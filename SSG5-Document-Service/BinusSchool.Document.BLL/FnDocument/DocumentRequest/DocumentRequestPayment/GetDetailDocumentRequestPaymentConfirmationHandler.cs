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
    public class GetDetailDocumentRequestPaymentConfirmationHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;

        public GetDetailDocumentRequestPaymentConfirmationHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailDocumentRequestPaymentConfirmationRequest>(
                            nameof(GetDetailDocumentRequestPaymentConfirmationRequest.IdDocumentReqApplicant));

            var getDocumentRequestInfo = await _dbContext.Entity<MsDocumentReqApplicant>()
                                            .Include(x => x.Parent)
                                                .ThenInclude(x => x.ParentRole)
                                            .Include(x => x.Student)
                                            .Include(x => x.DocumentReqApplicantDetails)
                                                .ThenInclude(x => x.DocumentReqType)
                                            .Include(x => x.DocumentReqStatusTrackingHistories)
                                            .Where(x => x.Id == param.IdDocumentReqApplicant)
                                            .Select(x => new
                                            {
                                                IdDocumentReqApplicant = x.Id,
                                                IdSchool = x.IdSchool,
                                                RequestNumber = x.RequestNumber,
                                                ParentApplicant = new ItemValueVm
                                                {
                                                    Id = x.IdParentApplicant,
                                                    Description = $"{NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName)} ({x.Parent.ParentRole.ParentRoleNameEng})"
                                                },
                                                RequestDate = x.RequestDate,
                                                Student = new NameValueVm
                                                {
                                                    Id = x.IdStudent,
                                                    Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
                                                },
                                                CurrentIdHomeroom = x.IdHomeroom,
                                                DocumentList = x.DocumentReqApplicantDetails
                                                                .Select(y => new GetDetailDocumentRequestPaymentConfirmation_Document
                                                                {
                                                                    Document = new ItemValueVm
                                                                    {
                                                                        Id = y.Id,
                                                                        Description = y.DocumentReqType.Name
                                                                    },
                                                                    NoOfCopy = y.NoOfCopy,
                                                                    PricePerCopy = y.PriceReal,
                                                                    TotalPrice = y.NoOfCopy * y.PriceReal
                                                                })
                                                                .OrderBy(y => y.Document.Description)
                                                                .ToList(),
                                                LatestIdDocumentReqStatusWorkflow = x.DocumentReqStatusTrackingHistories
                                                .OrderByDescending(y => y.StatusDate)
                                                .Select(y => y.IdDocumentReqStatusWorkflow)
                                                .FirstOrDefault()
                                            })
                                            .FirstOrDefaultAsync(CancellationToken);

            var currentHomeroomInfo = await _dbContext.Entity<MsHomeroom>()
                                        .Include(x => x.Grade)
                                        .Include(x => x.GradePathwayClassroom)
                                            .ThenInclude(x => x.Classroom)
                                        .Where(x => string.IsNullOrEmpty(getDocumentRequestInfo.CurrentIdHomeroom) ? false : x.Id == getDocumentRequestInfo.CurrentIdHomeroom)
                                        .Select(x => new ItemValueVm
                                        {
                                            Id = x.Id,
                                            Description = x.Grade.Description + x.GradePathwayClassroom.Classroom.Description
                                        })
                                        .FirstOrDefaultAsync(CancellationToken);

            var getDocumentInvoiceInfo = await _dbContext.Entity<TrDocumentReqPaymentMapping>()
                                            .Include(x => x.LtDocumentReqPaymentMethod)
                                            .Where(x => x.IdDocumentReqApplicant == param.IdDocumentReqApplicant)
                                            .Select(x => new
                                            {
                                                PaymentDueDate = x.EndDatePayment,
                                                TotalAmountInvoice = x.TotalAmountInvoice,
                                                PaymentMethod = new ItemValueVm
                                                {
                                                    Id = x.IdDocumentReqPaymentMethod,
                                                    Description = x.LtDocumentReqPaymentMethod.Name
                                                },
                                                IdDocumentReqPaymentManual = x.IdDocumentReqPaymentManual,
                                                UsingManualVerification = x.UsingManualVerification,
                                                IsVirtualAccount = x.IsVirtualAccount
                                            })
                                            .FirstOrDefaultAsync(CancellationToken);

            var getDocumentPaymentManualInfo = getDocumentInvoiceInfo == null ? null : 
                                                await _dbContext.Entity<TrDocumentReqPaymentManual>()
                                                .Where(x => string.IsNullOrEmpty(getDocumentInvoiceInfo.IdDocumentReqPaymentManual) ? false : x.Id == getDocumentInvoiceInfo.IdDocumentReqPaymentManual)
                                                .FirstOrDefaultAsync(CancellationToken);

            var result = new GetDetailDocumentRequestPaymentConfirmationResult
            {
                IdDocumentReqApplicant = getDocumentRequestInfo.IdDocumentReqApplicant,
                IdSchool = getDocumentRequestInfo.IdSchool,
                RequestNumber = getDocumentRequestInfo.RequestNumber,
                ParentApplicant = getDocumentRequestInfo.ParentApplicant,
                RequestDate = getDocumentRequestInfo.RequestDate,
                Student = getDocumentRequestInfo.Student,
                CurrentHomeroom = currentHomeroomInfo,
                PaymentDueDate = getDocumentInvoiceInfo?.PaymentDueDate,
                TotalAmountInvoice = getDocumentInvoiceInfo?.TotalAmountInvoice,
                DocumentList = getDocumentRequestInfo.DocumentList,
                PaymentMethod = getDocumentInvoiceInfo?.PaymentMethod,
                ParentCanConfirmPayment = getDocumentRequestInfo.LatestIdDocumentReqStatusWorkflow == DocumentRequestStatusWorkflow.WaitingForPayment ? true : false
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
