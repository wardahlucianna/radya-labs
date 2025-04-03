using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest.Validator
{
    public class UpdateDocumentRequestStaffValidator : AbstractValidator<UpdateDocumentRequestStaffRequest>
    {
        public UpdateDocumentRequestStaffValidator()
        {
            RuleFor(x => x.IdDocumentReqApplicant).NotEmpty();
            RuleFor(x => x.IdParentApplicant).NotEmpty();
            RuleFor(x => x.EstimationFinishDays).NotNull();

            RuleFor(x => x.DocumentRequestList)
                .NotEmpty()
                .ForEach(data => data.ChildRules(data =>
                {
                    data.RuleFor(x => x.IdBinusianPICList)
                        .NotEmpty()
                        .ForEach(data2 => data2.ChildRules(data2 =>
                        {
                            data2.RuleFor(x => x).NotEmpty();
                        }));

                    data.RuleFor(x => x.AdditionalFieldsList)
                        .ForEach(data2 => data2.ChildRules(data2 =>
                        {
                            data2.RuleFor(x => x.IdDocumentReqFormFieldAnswered).NotEmpty();
                        }));
                }));

            //RuleFor(x => x.Payment)
            //    .SetValidator(new CreateDocumentRequestStaffValidator_Payment());
        }
    }
}
