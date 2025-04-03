using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using FluentValidation;

namespace BinusSchool.User.FnCommunication.Message.Validator
{
    public class UnsendMessageValidator : AbstractValidator<UnsendMessageRequest>
    {
        public UnsendMessageValidator()
        {
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.IdMessage).NotEmpty();
            RuleFor(x => x.IdSchool).NotEmpty();
        }
    }
}
