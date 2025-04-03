using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using FluentValidation;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking.Validator
{
    public class SaveProjectTrackingPipelinesValidator : AbstractValidator<SaveProjectTrackingPipelinesRequest>
    {
        public SaveProjectTrackingPipelinesValidator()
        {
            RuleFor(a => a.IdSection).NotEmpty();
            RuleFor(a => a.SprintName).NotEmpty();
            RuleFor(a => a.StartDate).NotEmpty();
            RuleFor(a => a.EndDate).NotEmpty();
            RuleFor(a => a.IdStatus).NotEmpty();
            RuleFor(a => a.IdPhase).NotEmpty();
        }
    }
}
