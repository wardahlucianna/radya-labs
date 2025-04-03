using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest.Validator
{
    public class SaveReadyDocumentValidator : AbstractValidator<SaveReadyDocumentRequest>
    {
        public SaveReadyDocumentValidator()
        {
            RuleFor(x => x.ChecklistStatusList)
                .NotEmpty()
                .ForEach(data => data
                                .ChildRules(child =>
                                {
                                    child.RuleFor(x => x.IdDocumentReqApplicantDetail).NotEmpty();
                                    child.RuleFor(x => x.IsChecked).NotNull();
                                }));
        }
    }
}
