using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.BLPGroup.Validator
{
    public class UpdateBLPGroupValidator : AbstractValidator<UpdateBLPGroupRequest>
    {
        public UpdateBLPGroupValidator()
        {
            RuleFor(x => x.IdBLPGroup).NotEmpty();
            RuleFor(x => x.IdLevel).NotEmpty();
        }
    }
}
