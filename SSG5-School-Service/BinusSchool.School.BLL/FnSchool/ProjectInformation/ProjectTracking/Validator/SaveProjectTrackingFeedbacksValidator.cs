using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking.Validator
{
    public class SaveProjectTrackingFeedbacksValidator : AbstractValidator<SaveProjectTrackingFeedbacksRequest>
    {
        public SaveProjectTrackingFeedbacksValidator()
        {
            RuleFor(a => a.RequestDate).NotEmpty();
            RuleFor(a => a.IdSchool).NotEmpty();
            RuleFor(a => a.Requester).NotEmpty();
            RuleFor(a => a.FeatureRequested).NotEmpty();
            RuleFor(a => a.IdStatus).NotEmpty();

            RuleFor(a => a.SprintPlanned)
                .Must(sprints => sprints == null || sprints.Select(s => s.IdProjectPipeline).Distinct().Count() == sprints.Count)
                .WithMessage("Project Pipeline must be unique.")
                .When(a => a.SprintPlanned != null);

            RuleForEach(a => a.SprintPlanned)
                .ChildRules(sprintRule =>
                {
                    sprintRule.RuleFor(s => s.IdProjectPipeline)
                        .NotEmpty()
                        .WithMessage("Project Pipeline cannot be null.");
                });
        }
    }
}
