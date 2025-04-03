using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.ProjectTracking;
using FluentValidation;

namespace BinusSchool.School.FnSchool.ProjectInformation.ProjectTracking.Validator
{
    public class DeleteProjectTrackingFeedbacksValidator : AbstractValidator<DeleteProjectTrackingFeedbacksRequest>
    {
        public DeleteProjectTrackingFeedbacksValidator()
        {
            RuleFor(a => a.IdProjectFeedback).NotEmpty();
        }
    }
}
