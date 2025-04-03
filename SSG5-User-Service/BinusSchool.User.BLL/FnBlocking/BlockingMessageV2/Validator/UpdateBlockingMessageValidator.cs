using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnBlocking.BlockingMessageV2;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.BlockingMessageV2.Validator
{
    public class UpdateBlockingMessageValidator : AbstractValidator<UpdateBlockingMessageRequestV2>
    {
        public UpdateBlockingMessageValidator()
        {
            Include(new AddBlockingMessageValidator());
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
