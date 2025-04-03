using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.CreativityActivityService.Validator
{
    public class EmailDownloadValidator : AbstractValidator<EmailDownloadResult>
    {
        public EmailDownloadValidator()
        {
            RuleFor(x => x.IdUser)
                .NotEmpty()
                .WithName("Id User");

            RuleFor(x => x.Link)
                .NotNull()
                .WithName("Link");

        }
    }
}
