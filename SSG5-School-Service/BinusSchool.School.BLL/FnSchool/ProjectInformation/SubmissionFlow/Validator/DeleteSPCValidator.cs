using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.SubmissionFlow;
using FluentValidation;

namespace BinusSchool.School.FnSchool.ProjectInformation.SubmissionFlow.Validator
{
    internal class DeleteSPCValidator : AbstractValidator<DeleteSPCRequest>
    {
        public DeleteSPCValidator() {
            RuleFor(x => x.IdSchoolProjectCoordinator).NotEmpty();
        }
    }
}
