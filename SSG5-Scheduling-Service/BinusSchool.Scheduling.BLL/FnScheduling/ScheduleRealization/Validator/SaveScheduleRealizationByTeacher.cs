using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Validator
{
    public class SaveScheduleRealizationByTeacherValidator : AbstractValidator<SaveScheduleRealizationByTeacherRequest>
    {
        public SaveScheduleRealizationByTeacherValidator()
        {
            RuleFor(x => x.DataScheduleRealizations).NotNull().WithMessage("Ids cannot empty");
            // RuleFor(x => x.Ids).NotNull().WithMessage("Ids cannot empty");
            // RuleFor(x => x.Date).NotNull().WithMessage("Date cannot empty");
            // RuleFor(x => x.SessionID).NotNull().WithMessage("Session cannot empty");
            // RuleFor(x => x.ClassID).NotNull().WithMessage("Class ID cannot empty");
            // RuleFor(x => x.IdUserTeacher).NotEmpty().WithMessage("Id User Teacer cannot empty");
            // RuleFor(x => x.IdUserSubtituteTeacher).NotEmpty().WithMessage("Id User Subtitute Teacer cannot empty");
            // RuleFor(x => x.IdRegularVenue).NotEmpty().WithMessage("Id Regular Venue cannot empty");
            // RuleFor(x => x.IdChangeVenue).NotEmpty().WithMessage("Id Change Venue cannot empty");
        }
    }
}
