using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.SubmissionFlow;
using FluentValidation;

namespace BinusSchool.School.FnSchool.ProjectInformation.SubmissionFlow.Validator
{
    public class SaveSPCValidator : AbstractValidator<SaveSPCRequest>
    {
        public SaveSPCValidator()
        {
            RuleFor(x => x.IdSchool).NotNull();
            RuleFor(x => x.IdBinusian).NotEmpty();
        }
    }
}
