using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator
{
    public class GetDownloadStudentCertificationValidator : AbstractValidator<GetDownloadStudentCertificationRequest>
    {
        public GetDownloadStudentCertificationValidator()
        {
            RuleFor(x => x.IdStudent).NotEmpty().WithMessage("Event name cannot empty");
            RuleFor(x => x.IdAcadYears).NotEmpty().WithMessage("Acadmic Year cannot empty");
        }
    }
}
