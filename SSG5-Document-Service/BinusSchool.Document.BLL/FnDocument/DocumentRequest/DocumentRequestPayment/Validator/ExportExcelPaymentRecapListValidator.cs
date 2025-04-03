using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestPayment.Validator
{
    public class ExportExcelPaymentRecapListValidator : AbstractValidator<ExportExcelPaymentRecapListRequest>
    {
        public ExportExcelPaymentRecapListValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.PaymentPeriodStartDate).NotNull();
            RuleFor(x => x.PaymentPeriodEndDate).NotNull();
        }
    }
}
