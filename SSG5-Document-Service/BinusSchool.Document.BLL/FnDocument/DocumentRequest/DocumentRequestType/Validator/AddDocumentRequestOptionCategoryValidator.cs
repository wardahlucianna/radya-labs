using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator
{
    public class AddDocumentRequestOptionCategoryValidator : AbstractValidator<AddDocumentRequestOptionCategoryRequest>
    {
        public AddDocumentRequestOptionCategoryValidator()
        {
            RuleFor(x => x.CategoryDescription).NotEmpty();
            RuleFor(x => x.IdDocumentReqFieldType).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.OptionList)
               .NotEmpty()
               .ForEach(data => data.ChildRules(data =>
               {
                   data.RuleFor(x => x.OptionDescription).NotEmpty();
               }));
        }
    }
}
