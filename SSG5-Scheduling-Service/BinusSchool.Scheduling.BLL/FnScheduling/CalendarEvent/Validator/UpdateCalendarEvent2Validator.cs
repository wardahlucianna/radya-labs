using System;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.CalendarEvent.Validator
{
    public class UpdateCalendarEvent2Validator : AbstractValidator<UpdateCalendarEvent2Request>
    {
        public UpdateCalendarEvent2Validator(IServiceProvider provider)
        {
            RuleFor(x => x.Id).NotEmpty();

            Include(new AddCalendarEvent2Validator(provider));
        }
    }
}