using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnBlocking.BlockingMessage;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.BlockingMessage.Validator
{
    public class UpdateBlockingMessageValidator : AbstractValidator<UpdateBlockingMessageRequest>
    {
        public UpdateBlockingMessageValidator()
        {

            RuleFor(x => x.BlockingMessage).NotEmpty();

        }
    }
}
