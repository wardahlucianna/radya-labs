using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator
{
    public class UpdateDocumentRequestOptionCategoryValidator : AbstractValidator<UpdateDocumentRequestOptionCategoryRequest>
    {
        public UpdateDocumentRequestOptionCategoryValidator()
        {
            RuleFor(x => x.IdDocumentReqOptionCategory).NotEmpty();
            RuleFor(x => x.CategoryDescription).NotEmpty();

            RuleFor(x => x.NewOptionList)
               .ForEach(data => data.ChildRules(data =>
               {
                   data.RuleFor(x => x.OptionDescription).NotEmpty();
               }));
        }
    }
}
