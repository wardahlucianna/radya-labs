using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Scheduling.FnSchedule.CalendarEvent.Validator
{
    public class AddCalendarEventValidator : AbstractValidator<AddCalendarEventRequest>
    {
        private MsEventType _eventType;
        
        private readonly ISchedulingDbContext _dbContext;

        public AddCalendarEventValidator(IServiceProvider provider)
        {
            _dbContext = provider.GetService<ISchedulingDbContext>();
            var localizer = provider.GetService<IStringLocalizer>();

            RuleFor(x => x.IdSchool).NotEmpty();
            
            RuleFor(x => x.IdAcadyear).NotEmpty();

            RuleFor(x => x.Name).NotEmpty().MaximumLength(50);

            RuleFor(x => x.Dates)
                .NotEmpty()
                .ForEach(dates => dates.ChildRules(date =>
                {
                    date.RuleFor(x => x.Start)
                        .NotEmpty()
                        .GreaterThan(DateTimeUtil.ServerTime)
                        .LessThan(x => x.End);

                    date.RuleFor(x => x.End)
                        .NotEmpty()
                        .GreaterThan(x => x.Start);
                }));

            RuleFor(x => x.IdEventType)
                .NotEmpty()
                .MustAsync((model, _, token) => IsEventTypeValid(model.IdEventType, model.IdSchool, token))
                .WithMessage(x => string.Format(localizer["ExNotExist"], localizer["EventType"], "Id", x.IdEventType));

            RuleFor(x => x.Attendance)
                .IsInEnum()
                .Must(x => _eventType.Code == "Holiday" ? x == EventAttendanceType.NotSet : true)
                .WithMessage(x => "Holiday must not have attendance.")
                .Must(x => _eventType.Code == "Event" ? x != EventAttendanceType.NotSet : true)
                .WithMessage(x => "Event must have attendance.");

            RuleFor(x => x.Role).NotEmpty()
                .Must(x => x.IsConstantOf<RoleConstant>() || x == "ALL")
                .WithMessage(x => string.Format(localizer["ExNotExist"], localizer["Role"], "Code", x.Role));
            
            When(x => x.Role == RoleConstant.Teacher || x.Role == RoleConstant.Student, () =>
            {
                RuleFor(x => x.Option).IsInEnum()
                    .Must(x => x != EventOptionType.None)
                    .WithMessage("Option for Teacher/Student must not None.");

                When(x => x.Option == EventOptionType.Grade, () => RuleFor(x => x.IdGrades).NotEmpty());
                
                When(x => x.Option == EventOptionType.Department, () => RuleFor(x => x.IdDepartments).NotEmpty());

                When(x => x.Option == EventOptionType.Subject, () => 
                {
                    RuleFor(x => x.Subjects)
                        .NotEmpty()
                        .ForEach(subjects => subjects.ChildRules(subject =>
                        {
                            subject.RuleFor(x => x.IdGrade).NotEmpty();

                            subject.RuleFor(x => x.IdSubjects).NotEmpty();
                        }));
                });
            }).Otherwise(() => 
            {
                RuleFor(x => x.Option).IsInEnum()
                    .Must(x => x == EventOptionType.None)
                    .WithMessage("Option for All/Staff must be None.");
            });
        }

        private async Task<bool> IsEventTypeValid(string id, string idSchool, CancellationToken token)
        {
            //Note : Change By Robby
            _eventType = await _dbContext.Entity<MsEventType>()
                .Where(x => x.Id == id && x.IdAcademicYear == idSchool)
                .FirstOrDefaultAsync(token);

            return _eventType != null;
        }
    }
}
