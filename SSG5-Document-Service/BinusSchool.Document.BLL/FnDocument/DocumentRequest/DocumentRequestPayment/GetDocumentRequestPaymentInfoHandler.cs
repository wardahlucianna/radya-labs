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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class GetDocumentRequestPaymentInfoHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetDocumentRequestPaymentInfoHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDocumentRequestPaymentInfoRequest>(
                            nameof(GetDocumentRequestPaymentInfoRequest.IdDocumentReqApplicantList));

            var result = await GetDocumentRequestPaymentInfo(new GetDocumentRequestPaymentInfoRequest
            {
                IdDocumentReqApplicantList = param.IdDocumentReqApplicantList
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<List<GetDocumentRequestPaymentInfoResult>> GetDocumentRequestPaymentInfo(GetDocumentRequestPaymentInfoRequest param)
        {
            var getDocumentReqApplicantList = await _dbContext.Entity<MsDocumentReqApplicant>()
                                                .Include(x => x.DocumentReqApplicantDetails)
                                                .Include(x => x.DocumentReqPaymentMappings)
                                                    .ThenInclude(x => x.LtDocumentReqPaymentMethod)
                                                .Where(x => param.IdDocumentReqApplicantList.Any(y => y == x.Id))
                                                .Select(x => new
                                                {
                                                    IdDocumentReqApplicant = x.Id,
                                                    DocumentList = x.DocumentReqApplicantDetails
                                                            .Select(y => new
                                                            {
                                                                IdDocumentReqApplicantDetail = y.Id,
                                                                PriceInvoice = y.PriceInvoice
                                                            })
                                                            .ToList(),
                                                    PaymentMappings = x.DocumentReqPaymentMappings
                                                })
                                                .ToListAsync(CancellationToken);

            //var getDocumentReqPaymentMapping = await _dbContext.Entity<TrDocumentReqPaymentMapping>()
            //                                    .Include(x => x.LtDocumentReqPaymentMethod)
            //                                    .Where(x => param.IdDocumentReqApplicantList.Any(y => y == x.IdDocumentReqApplicant))
            //                                    .ToListAsync(CancellationToken);


            var resultList = new List<GetDocumentRequestPaymentInfoResult>();

            // get free document request
            var resultTempFree = getDocumentReqApplicantList
                                    .Where(x => x.DocumentList.Sum(y => y.PriceInvoice) == 0)
                                    .Select(x => new GetDocumentRequestPaymentInfoResult
                                    {
                                        IdDocumentReqApplicant = x.IdDocumentReqApplicant,
                                        PaymentStatus = PaymentStatus.Free,
                                        PaymentStatusDescription = PaymentStatus.Free.GetDescription(),
                                        TotalAmountReal = 0,
                                        TotalAmountInvoice = 0,
                                        UsingManualVerification = null,
                                        IsVirtualAccount = null,
                                        StartDatePayment = null,
                                        EndDatePayment = null,
                                        DocumentReqPaymentMethod = null,
                                        PaidAmount = null,
                                        PaymentDate = null,
                                        HasConfirmPayment = null
                                    })
                                    .ToList();
            resultList.AddRange(resultTempFree);

            // get document request that using payment manual
            var paymentMappingLeftJoinPaymentManual = getDocumentReqApplicantList
                                                        .SelectMany(x => x.PaymentMappings)
                                                        .Where(x => x.UsingManualVerification)
                                                        .GroupJoin(
                                                            _dbContext.Entity<TrDocumentReqPaymentManual>(),
                                                            paymentMapping => paymentMapping.IdDocumentReqPaymentManual,
                                                            paymentManual => paymentManual.Id,
                                                            (paymentMapping, paymentManual) => new { paymentMapping, paymentManual }
                                                            )
                                                        .SelectMany(x => x.paymentManual.DefaultIfEmpty(),
                                                                (paymentMapping, paymentManual) => new { paymentMapping, paymentManual }
                                                            )
                                                        .Distinct()
                                                        .ToList();

            var resultTempPaymentManual = paymentMappingLeftJoinPaymentManual
                            .Select(x => new GetDocumentRequestPaymentInfoResult
                            {
                                IdDocumentReqApplicant = x.paymentMapping.paymentMapping.IdDocumentReqApplicant,

                                PaymentStatus = getDocumentReqApplicantList
                                                .Where(y => y.IdDocumentReqApplicant == x.paymentMapping.paymentMapping.IdDocumentReqApplicant)
                                                .Select(y => y.DocumentList.Sum(z => z.PriceInvoice) == 0 ? PaymentStatus.Free :
                                                    (x.paymentManual?.PaymentStatus == 1 && x.paymentManual?.VerificationStatus == 1) ?
                                                    PaymentStatus.Paid : 
                                                    _dateTime.ServerTime > x.paymentMapping.paymentMapping.EndDatePayment ? PaymentStatus.Expired :
                                                    PaymentStatus.Unpaid
                                                )
                                                .FirstOrDefault(),

                                PaymentStatusDescription = getDocumentReqApplicantList
                                                .Where(y => y.IdDocumentReqApplicant == x.paymentMapping.paymentMapping.IdDocumentReqApplicant)
                                                .Select(y => y.DocumentList.Sum(z => z.PriceInvoice) == 0 ? PaymentStatus.Free.GetDescription() :
                                                    (x.paymentManual?.PaymentStatus == 1 && x.paymentManual?.VerificationStatus == 1) ?
                                                    PaymentStatus.Paid.GetDescription() :
                                                    _dateTime.ServerTime > x.paymentMapping.paymentMapping.EndDatePayment ? PaymentStatus.Expired.GetDescription() :
                                                    PaymentStatus.Unpaid.GetDescription()
                                                )
                                                .FirstOrDefault(),

                                TotalAmountReal = x.paymentMapping.paymentMapping.TotalAmountReal,
                                TotalAmountInvoice = x.paymentMapping.paymentMapping.TotalAmountInvoice,
                                UsingManualVerification = x.paymentMapping.paymentMapping.UsingManualVerification,
                                IsVirtualAccount = x.paymentMapping.paymentMapping.IsVirtualAccount,
                                StartDatePayment = x.paymentMapping.paymentMapping.StartDatePayment,
                                EndDatePayment = x.paymentMapping.paymentMapping.EndDatePayment,
                                DocumentReqPaymentMethod = new ItemValueVm
                                {
                                    Id = x.paymentMapping.paymentMapping.LtDocumentReqPaymentMethod.Id,
                                    Description = x.paymentMapping.paymentMapping.LtDocumentReqPaymentMethod.Name
                                },
                                PaidAmount = x.paymentManual?.PaidAmount,
                                PaymentDate = x.paymentManual?.PaymentDate,
                                HasConfirmPayment = x.paymentManual?.PaymentStatus == 1 && (x.paymentManual?.VerificationStatus == 0 || x.paymentManual?.VerificationStatus == null),
                                AttachmentImageUrl = x.paymentManual?.AttachmentImageUrl,
                                SenderAccountName = x.paymentManual?.SenderAccountName,
                                PaymentVerificationDate = x.paymentManual?.VerificationDate
                            })
                            .Distinct()
                            .ToList();
            resultList.AddRange(resultTempPaymentManual);

            return resultList;
        }
    }
}
