using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest.Validator
{
    public class CreateDocumentRequestStaffValidator : AbstractValidator<CreateDocumentRequestStaffRequest>
    {
        public CreateDocumentRequestStaffValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdParentApplicant).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();
            RuleFor(x => x.CanProcessBeforePaid).NotNull();
            RuleFor(x => x.EstimationFinishDays).NotNull();

            RuleFor(x => x.DocumentRequestList)
                .NotEmpty()
                .ForEach(data => data.ChildRules(data =>
                {
                    data.RuleFor(x => x.IdDocumentReqType).NotEmpty();
                    data.RuleFor(x => x.IsAcademicDocument).NotNull();

                    data.When(x => x.IsAcademicDocument, () =>
                    {
                        data.RuleFor(x => x.IdGradeDocument).NotEmpty();
                    });

                    data.RuleFor(x => x.IdBinusianPICList)
                        .NotEmpty()
                        .ForEach(data2 => data2.ChildRules(data2 =>
                        {
                            data2.RuleFor(x => x).NotEmpty();
                        }));

                    data.RuleFor(x => x.IsMakeFree).NotNull();

                    data.RuleFor(x => x.AdditionalFieldsList)
                        .ForEach(data2 => data2.ChildRules(data2 =>
                        {
                            data2.RuleFor(x => x.IdDocumentReqFormField).NotEmpty();
                        }));
                }));

            //RuleFor(x => x.Payment)
            //    .SetValidator(new CreateDocumentRequestStaffValidator_Payment());
        }
    }

    //public class CreateDocumentRequestStaffValidator_Payment : AbstractValidator<CreateDocumentRequestStaffRequest_Payment>
    //{
    //    public CreateDocumentRequestStaffValidator_Payment()
    //    {
    //        RuleFor(x => x.IsPaid).NotNull();
    //        RuleFor(x => x.IdDocumentReqPaymentMethod).NotEmpty();

    //        When(x => x.IsPaid, () =>
    //        {
    //            RuleFor(x => x.PaidAmount).NotNull();
    //        });
    //    }
    //}
}
