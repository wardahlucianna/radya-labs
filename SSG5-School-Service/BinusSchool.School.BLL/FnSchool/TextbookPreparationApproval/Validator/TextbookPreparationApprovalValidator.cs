using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApproval;
using FluentValidation;

namespace BinusSchool.School.FnSchool.TextbookPreparationApproval.Validator
{
    public class TextbookPreparationApprovalValidator : AbstractValidator<TextbookPreparationApprovalRequest>
    {
        public TextbookPreparationApprovalValidator()
        {
            RuleFor(x => x.IdUser).NotEmpty().WithMessage("Id user cant empty");
            RuleFor(x => x.Ids).NotEmpty().WithMessage("Ids cant empty");
        }
    }
}
