using System;
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
    public class AddCalendarEvent2Validator : AbstractValidator<AddCalendarEvent2Request>
    {
        private MsEventType _eventType;

        private readonly ISchedulingDbContext _dbContext;

        public AddCalendarEvent2Validator(IServiceProvider provider)
        {
            _dbContext = provider.GetService<ISchedulingDbContext>();
            var localizer = provider.GetService<IStringLocalizer>();

            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.Dates)
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
                .MustAsync((model, _, token) => IsEventTypeValid(model.IdEventType, token))
                .WithMessage(x => string.Format(localizer["ExNotExist"], localizer["EventType"], "Id", x.IdEventType));
                
            RuleFor(x => x.Role).NotEmpty()
                .Must(x => x.IsConstantOf<RoleConstant>() || x == "ALL")
                .WithMessage(x => string.Format(localizer["ExNotExist"], localizer["Role"], "Code", x.Role));

            When(x => x.Role == RoleConstant.Teacher || x.Role == RoleConstant.Student, () =>
            {
                When(x => x.Role == RoleConstant.Teacher, () =>
                {
                    RuleFor(x => x.ForTeacher).NotEmpty();

                    RuleFor(x => x.Option).IsInEnum()
                        .Must(x => x != EventOptionType.None && x != EventOptionType.Personal)
                        .WithMessage("Option for Teacher must not be None or Personal.");

                    When(x => x.Option == EventOptionType.Grade, () => RuleFor(x => x.ForTeacher.IdGrades).NotEmpty());
                
                    When(x => x.Option == EventOptionType.Department, () => RuleFor(x => x.ForTeacher.IdDepartments).NotEmpty());

                    When(x => x.Option == EventOptionType.Subject, () => 
                    {
                        RuleFor(x => x.ForTeacher.Subjects)
                            .NotEmpty()
                            .ForEach(subjects => subjects.ChildRules(subject =>
                            {
                                subject.RuleFor(x => x.IdGrade).NotEmpty();

                                subject.RuleFor(x => x.IdSubjects).NotEmpty();
                            }));
                    });
                })
                .Otherwise(() =>
                {
                    RuleFor(x => x.ForStudent).NotEmpty();

                    RuleFor(x => x.Option).IsInEnum()
                        .Must(x => x != EventOptionType.None && x != EventOptionType.Department)
                        .WithMessage("Option for Student must not be None or Department.");

                    When(x => x.Option == EventOptionType.Personal, () =>
                        RuleFor(x => x.ForStudent.ShowOnCalendarAcademic)
                            .Must(x => !x)
                            .WithMessage("ShowOnCalendarAcademic must be false when Option is Personal."));

                    RuleFor(x => x.ForStudent.AttendanceOption).IsInEnum();

                    When(x => new[] { EventIntendedForAttendanceStudent.All }.Contains(x.ForStudent.AttendanceOption), () =>
                        RuleFor(x => x.ForStudent.SetAttendanceEntry)
                            .Must(x => !x)
                            .WithMessage("SetAttendanceEntry must be false when AttendanceOption is No or All."));
                        
                    When(x => new[] { EventIntendedForAttendanceStudent.Mandatory, EventIntendedForAttendanceStudent.Excuse }.Contains(x.ForStudent.AttendanceOption), () =>
                    {
                        When(x => x.ForStudent.AttendanceOption == EventIntendedForAttendanceStudent.Mandatory, () =>
                            RuleFor(x => x.ForStudent.SetAttendanceEntry)
                                .Must(x => x)
                                .WithMessage("SetAttendanceEntry must be true when AttendanceOption is Mandatory."));

                        When(x => x.ForStudent.SetAttendanceEntry, () =>
                        {
                            RuleFor(x => x.ForStudent.PicAttendance).IsInEnum().NotEmpty();

                            When(x => x.ForStudent.PicAttendance.HasValue && 
                                new[] { EventIntendedForAttendancePICStudent.UserTeacher, EventIntendedForAttendancePICStudent.UserStaff }.Contains(x.ForStudent.PicAttendance.Value), () =>
                                RuleFor(x => x.ForStudent.IdUserPic).NotEmpty());

                            RuleFor(x => x.ForStudent.RepeatAttendanceCheck).NotEmpty();

                            When(x => x.ForStudent.RepeatAttendanceCheck.Value, () => 
                            {
                                RuleFor(x => x.ForStudent.AttendanceCheckDates)
                                    .NotEmpty()
                                    .Must(x => x.Count() == 1)
                                    .WithMessage("Attendance Check must be set once.")
                                    .ForEach(checks => checks.ChildRules(check => 
                                    {
                                        check.RuleFor(x => x.AttendanceChecks)
                                            .NotEmpty()
                                            .Must(x => x.Count(y => y.IsMandatory) == 1)
                                            .WithMessage("Mandatory Attendance Check must be set once.")
                                            .ForEach(days => days.ChildRules(day =>
                                            {
                                                day.RuleFor(x => x.Name).NotEmpty();
                                            }));
                                    }));
                            })
                            .Otherwise(() =>
                            {
                                RuleFor(x => x.ForStudent.AttendanceCheckDates)
                                    .NotEmpty()
                                    .Must((x, y) => DateTimeUtil.GetTotalDayBetweenRange(x.Dates.ToArray()) == y.Count())
                                    .WithMessage("Attendance Check must be set through selected date.")
                                    .ForEach(checks => checks.ChildRules(check => 
                                    {
                                        check.RuleFor(x => x.StartDate)
                                            .NotEmpty();

                                        check.RuleFor(x => x.AttendanceChecks)
                                            .NotEmpty()
                                            .Must(x => x.Count(y => y.IsMandatory) == 1)
                                            .WithMessage("Mandatory Attendance Check must be set once.")
                                            .ForEach(days => days.ChildRules(day =>
                                            {
                                                day.RuleFor(x => x.Name).NotEmpty();
                                            }));
                                    }));
                            });
                        });
                    });
                        
                    When(x => x.Option == EventOptionType.Personal, () => RuleFor(x => x.ForStudent.IdStudents).NotEmpty());

                    When(x => x.Option == EventOptionType.Subject, () => RuleFor(x => x.ForStudent.IdSubjects).NotEmpty());

                    When(x => x.Option == EventOptionType.Grade, () => RuleFor(x => x.ForStudent.IdHomerooms).NotEmpty());
                });
            })
            .Otherwise(() =>
            {
                RuleFor(x => x.Option).IsInEnum()
                    .Must(x => x == EventOptionType.None)
                    .WithMessage("Option for All/Staff/Parent must be None.");
            });
            
        }

        private async Task<bool> IsEventTypeValid(string id, CancellationToken token)
        {
            _eventType = await _dbContext.Entity<MsEventType>()
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync(token);

            return _eventType != null;
        }
    }
}
