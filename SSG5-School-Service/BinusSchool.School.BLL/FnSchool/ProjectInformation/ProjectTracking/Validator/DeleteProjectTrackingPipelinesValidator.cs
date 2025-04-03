using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using FluentValidation;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking.Validator
{
    public class DeleteProjectTrackingPipelinesValidator : AbstractValidator<DeleteProjectTrackingPipelinesRequest>
    {
        public DeleteProjectTrackingPipelinesValidator()
        {
            RuleFor(a => a.IdProjectPipeline).NotEmpty();
        }
    }
}
