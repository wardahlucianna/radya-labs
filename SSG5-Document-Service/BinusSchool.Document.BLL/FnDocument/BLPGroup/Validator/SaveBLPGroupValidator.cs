using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;
using FluentValidation;

namespace BinusSchool.Document.FnDocument.BLPGroup.Validator
{
    public class SaveBLPGroupValidator : AbstractValidator<SaveBLPGroupRequest>
    {
        public SaveBLPGroupValidator()
        {
            RuleFor(x => x.IdLevel).NotEmpty();
            RuleFor(x => x.GroupName).NotEmpty();
        }
    }
}
