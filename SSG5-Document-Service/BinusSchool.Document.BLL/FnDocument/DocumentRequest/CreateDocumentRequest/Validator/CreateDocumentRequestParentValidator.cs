using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest.Validator
{
    public class CreateDocumentRequestParentValidator : AbstractValidator<CreateDocumentRequestParentRequest>
    {
        public CreateDocumentRequestParentValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdParentApplicant).NotEmpty();
            RuleFor(x => x.IdStudent).NotEmpty();

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

                    data.RuleFor(x => x.AdditionalFieldsList)
                        .ForEach(data2 => data2.ChildRules(data2 =>
                        {
                            data2.RuleFor(x => x.IdDocumentReqFormField).NotEmpty();
                        }));
                }));

            //RuleFor(x => x.Payment)
            //    .SetValidator(new CreateDocumentRequestParentValidator_Payment());
        }
    }

    //public class CreateDocumentRequestParentValidator_Payment : AbstractValidator<CreateDocumentRequestParentRequest_Payment>
    //{
    //    public CreateDocumentRequestParentValidator_Payment()
    //    {
    //        RuleFor(x => x.IdDocumentReqPaymentMethod).NotEmpty();
    //    }
    //}
}
