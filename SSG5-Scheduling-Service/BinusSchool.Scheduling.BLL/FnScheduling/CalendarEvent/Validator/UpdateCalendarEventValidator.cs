using System;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using FluentValidation;

namespace BinusSchool.Scheduling.FnSchedule.CalendarEvent.Validator
{
    public class UpdateCalendarEventValidator : AbstractValidator<UpdateCalendarEventRequest>
    {
        public UpdateCalendarEventValidator(IServiceProvider provider)
        {
            RuleFor(x => x.Id).NotEmpty();

            Include(new AddCalendarEventValidator(provider));
        }
    }
}