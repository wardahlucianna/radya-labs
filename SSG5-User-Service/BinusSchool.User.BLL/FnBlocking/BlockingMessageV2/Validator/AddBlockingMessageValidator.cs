using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnBlocking.BlockingMessageV2;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.BlockingMessageV2.Validator
{
    public class AddBlockingMessageValidator : AbstractValidator<AddBlockingMessageRequestV2>
    {
        public AddBlockingMessageValidator()
        {
            RuleFor(x => x.IdCategory).NotEmpty();
            RuleFor(x => x.Content).NotEmpty();
        }
    }
}
