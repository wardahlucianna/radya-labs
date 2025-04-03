using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator
{
    public class AddDocumentRequestTypeValidator : AbstractValidator<AddDocumentRequestTypeRequest>
    {
        public AddDocumentRequestTypeValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.DocumentName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DocumentDescription).MaximumLength(500);
            RuleFor(x => x.ActiveStatus).NotNull();
            RuleFor(x => x.Price).NotNull();

            When(x => x.Price > 0, () =>
            {
                RuleFor(x => x.InvoicePaymentExpiredHours).NotEmpty();
            });

            RuleFor(x => x.DefaultProcessDays).NotNull();

            RuleFor(x => x.HardCopyAvailable).NotNull().When(x => x.SoftCopyAvailable == false).WithMessage("Should select either Hard Copy Available or Soft Copy Available or both options");
            RuleFor(x => x.SoftCopyAvailable).NotNull().When(x => x.HardCopyAvailable == false).WithMessage("Should select either Hard Copy Available or Soft Copy Available or both options");

            RuleFor(x => x.IsAcademicDocument).NotNull();
            RuleFor(x => x.HasTermOptions).NotNull().When(x => x.IsAcademicDocument);
            RuleFor(x => x.VisibleToParent).NotNull();
            RuleFor(x => x.ParentNeedApproval).NotNull().When(x => x.VisibleToParent);

            RuleFor(x => x.IsUsingNoOfPages).NotNull();
            RuleFor(x => x.DefaultNoOfPages).GreaterThan(0).When(x => x.IsUsingNoOfPages);

            RuleFor(x => x.IsUsingNoOfCopy).NotNull();
            RuleFor(x => x.MaxNoOfCopy).GreaterThan(0).When(x => x.IsUsingNoOfCopy);

            When(x => x.IsAcademicDocument, () =>
            {
                RuleFor(x => x.CodeGrades)
                   .NotNull()
                   .ForEach(data => data.ChildRules(data =>
                   {
                       data.RuleFor(x => x).NotEmpty();
                   }));
            });

            When(x => x.AdditionalFields != null && x.AdditionalFields.Count > 0, () =>
            {
                RuleFor(x => x.AdditionalFields)
                   .ForEach(data => data.ChildRules(data =>
                   {
                       data.RuleFor(x => x.IdDocumentReqFieldType).NotEmpty();
                       data.RuleFor(x => x.QuestionDescription).NotEmpty().MaximumLength(128);
                       data.RuleFor(x => x.OrderNumber).NotNull();
                       data.RuleFor(x => x.IsRequired).NotNull();
                   }));
            });

            RuleFor(x => x.IdBinusianDefaultPICIndividuals)
                   .NotNull()
                   .When(x => x.IdRoleDefaultPICGroups == null || x.IdRoleDefaultPICGroups.Count == 0)
                   .WithMessage("Should select at least one PIC Individual or PIC Group")
                   .ForEach(data => data.ChildRules(data =>
                   {
                       data.RuleFor(x => x).NotEmpty();
                   }));

            RuleFor(x => x.IdRoleDefaultPICGroups)
                   .NotNull()
                   .WithMessage("Should select at least one PIC Individual or PIC Group")
                   .When(x => x.IdBinusianDefaultPICIndividuals == null || x.IdBinusianDefaultPICIndividuals.Count == 0)
                   .ForEach(data => data.ChildRules(data =>
                   {
                       data.RuleFor(x => x).NotEmpty();
                   }));
        }
    }
}
