using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Util.FnNotification.SmtpEmail;
using FluentValidation;

namespace BinusSchool.Util.FnNotification.SmtpEmail.Validator
{
    public class SendSmtpEmailValidator : AbstractValidator<SendSmtpEmailRequest>
    {
        public SendSmtpEmailValidator()
        {
            //RuleFor(x => x.SmtpConfiguration)
            //    .NotNull()
            //    .SetValidator(new SmtpConfigurationValidator());

            RuleFor(x => x.IdSchool)
                .NotEmpty();

            RuleFor(x => x.RecepientConfiguration)
                .NotNull()
                .SetValidator(new RecepientConfiguration());

            RuleFor(x => x.MessageContent)
                .NotNull()
                .SetValidator(new MessageContentValidator());
        }

        //private class SmtpConfigurationValidator : AbstractValidator<SendSmtpEmailRequest_SmtpConfiguration>
        //{
        //    public SmtpConfigurationValidator()
        //    {
        //        RuleFor(x => x.HostServer)
        //            .NotEmpty();

        //        RuleFor(x => x.Port)
        //            .NotEmpty();

        //        RuleFor(x => x.FromAddress)
        //            .NotEmpty();

        //        RuleFor(x => x.MailPassword)
        //            .NotEmpty();
        //    }
        //}

        private class RecepientConfiguration : AbstractValidator<SendSmtpEmailRequest_RecepientConfiguration>
        {
            public RecepientConfiguration()
            {
                RuleFor(x => x.ToList)
                    .NotEmpty();

                //RuleFor(x => x.ToList)
                //    .NotEmpty()
                //    .ForEach(tos => tos.ChildRules(
                //    to => to.RuleFor(x => x.Address).NotEmpty()
                //    ));
            }
        }

        private class MessageContentValidator : AbstractValidator<SendSmtpEmailRequest_MessageContent>
        {
            public MessageContentValidator()
            {
                RuleFor(x => x.Subject)
                    .NotEmpty();

                RuleFor(x => x.BodyHtml)
                    .NotEmpty();
            }
        }
    }
}
