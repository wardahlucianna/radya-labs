//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using BinusSchool.Common.Constants;
//using BinusSchool.Common.Exceptions;
//using BinusSchool.Common.Extensions;
//using BinusSchool.Common.Model;
//using BinusSchool.Common.Model.Enums;
//using BinusSchool.Common.Utils;
//using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
//using BinusSchool.Persistence.SchedulingDb.Abstractions;
//using BinusSchool.Persistence.SchedulingDb.Entities;
//using BinusSchool.Scheduling.FnSchedule.CalendarEvent.Validator;
//using Microsoft.EntityFrameworkCore;

//namespace BinusSchool.Scheduling.FnSchedule.CalendarEvent
//{
//    public class UpdateCalendarEvent2Handler : AddCalendarEvent2Handler
//    {
//        private readonly ISchedulingDbContext _dbContext;
//        private readonly IServiceProvider _provider;

//        public UpdateCalendarEvent2Handler(ISchedulingDbContext dbContext, IServiceProvider provider) : base(dbContext, provider)
//        {
//            _dbContext = dbContext;
//            _provider = provider;
//        }

//        protected override async Task<ApiErrorResult<object>> Handler()
//        {
//            var body = await Request.GetBody<UpdateCalendarEvent2Request>();
//            (await new UpdateCalendarEvent2Validator(_provider).ValidateAsync(body)).EnsureValid();

//            var existEvent = await _dbContext.Entity<MsEvent>()
//                .Include(x => x.EventIntendedFor)
//                .Include(x => x.EventDetails).ThenInclude(x => x.UserEvents)
//                .FirstOrDefaultAsync(x => x.Id == body.Id, CancellationToken);
//            if (existEvent is null)
//                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Event"], "Id", body.Id));

//            var ay = await _dbContext.Entity<MsEventType>()
//                .Where(x => x.Id == body.IdEventType)
//                .Select(x => new { x.IdAcademicYear, x.AcademicYear.IdSchool })
//                .FirstOrDefaultAsync(CancellationToken);
//            if (ay is null)
//                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["EventType"], "Id", body.IdEventType));

//            existEvent.IdEventType = body.IdEventType;
//            existEvent.Name = body.Name;
//            existEvent.IsShowOnCalendarAcademic = body.Role != RoleConstant.Student || body.ForStudent.ShowOnCalendarAcademic;
//            //existEvent.AttendanceType = EventAttendanceType.NotSet; // NOTE: this field should not exist
//            _dbContext.Entity<MsEvent>().Update(existEvent);

//            existEvent.EventIntendedFor.IntendedFor = body.Role;
//            existEvent.EventIntendedFor.Option = body.Option;
//            _dbContext.Entity<MsEventIntendedFor>().Update(existEvent.EventIntendedFor);

//            // inactive existing intended for
//            if (existEvent.EventIntendedFor.IntendedFor == RoleConstant.Teacher)
//            {
//                if (existEvent.EventIntendedFor.Option == EventOptionType.Grade)
//                {
//                    var intendedForGrades = await _dbContext.Entity<MsEventIntendedForGrade>()
//                        .Where(x => x.IdEventIntendedFor == existEvent.Id)
//                        .ToListAsync(CancellationToken);

//                    intendedForGrades.ForEach(x => x.IsActive = false);
//                    _dbContext.Entity<MsEventIntendedForGrade>().UpdateRange(intendedForGrades);
//                }
//                else if (existEvent.EventIntendedFor.Option == EventOptionType.Department)
//                {
//                    var intendedForDepartments = await _dbContext.Entity<MsEventIntendedForDepartment>()
//                        .Where(x => x.IdEventIntendedFor == existEvent.Id)
//                        .ToListAsync(CancellationToken);

//                    intendedForDepartments.ForEach(x => x.IsActive = false);
//                    _dbContext.Entity<MsEventIntendedForDepartment>().UpdateRange(intendedForDepartments);
//                }
//                else if (existEvent.EventIntendedFor.Option == EventOptionType.Subject)
//                {
//                    var intendedForGradeSubjects = await _dbContext.Entity<MsEventIntendedForGradeSubject>()
//                        .Include(x => x.EventIntendedForSubjects)
//                        .Where(x => x.IdEventIntendedFor == existEvent.Id)
//                        .ToListAsync(CancellationToken);

//                    foreach (var gradeSubject in intendedForGradeSubjects)
//                    {
//                        gradeSubject.IsActive = false;
//                        _dbContext.Entity<MsEventIntendedForGradeSubject>().Update(gradeSubject);

//                        foreach (var subject in gradeSubject.EventIntendedForSubjects)
//                        {
//                            subject.IsActive = false;
//                            _dbContext.Entity<MsEventIntendedForSubject>().Update(subject);
//                        }
//                    }
//                }
//            }
//            else if (existEvent.EventIntendedFor.IntendedFor == RoleConstant.Student)
//            {
//                if (existEvent.EventIntendedFor.Option == EventOptionType.Personal)
//                {
//                    var intendedForPersonals = await _dbContext.Entity<MsEventIntendedForPersonalStudent>()
//                        .Where(x => x.IdEventIntendedFor == existEvent.Id)
//                        .ToListAsync(CancellationToken);
                    
//                    intendedForPersonals.ForEach(x => x.IsActive = false);
//                    _dbContext.Entity<MsEventIntendedForPersonalStudent>().UpdateRange(intendedForPersonals);
//                }
//                else if (existEvent.EventIntendedFor.Option == EventOptionType.Subject)
//                {
//                    var intendedForSubjects = await _dbContext.Entity<MsEventIntendedForSubjectStudent>()
//                        .Where(x => x.IdEventIntendedFor == existEvent.Id)
//                        .ToListAsync(CancellationToken);
                    
//                    intendedForSubjects.ForEach(x => x.IsActive = false);
//                    _dbContext.Entity<MsEventIntendedForSubjectStudent>().UpdateRange(intendedForSubjects);
//                }
//                else if (existEvent.EventIntendedFor.Option == EventOptionType.Grade)
//                {
//                    var intendedForGrades = await _dbContext.Entity<MsEventIntendedForGradeStudent>()
//                        .Where(x => x.IdEventIntendedFor == existEvent.Id)
//                        .ToListAsync(CancellationToken);
                    
//                    intendedForGrades.ForEach(x => x.IsActive = false);
//                    _dbContext.Entity<MsEventIntendedForGradeStudent>().UpdateRange(intendedForGrades);
//                }

//                var existAttStudent = await _dbContext.Entity<MsEventIntendedForAttendanceStudent>()
//                    .Include(x => x.EventIntendedForAtdPICStudents)
//                    .Include(x => x.EventIntendedForAtdCheckStudents)
//                    .Where(x => x.IdEventIntendedFor == existEvent.Id)
//                    .FirstOrDefaultAsync(CancellationToken);
                
//                existAttStudent.IsActive = false;
//                _dbContext.Entity<MsEventIntendedForAttendanceStudent>().UpdateRange(existAttStudent);

//                foreach (var existAttPic in existAttStudent.EventIntendedForAtdPICStudents)
//                {
//                    existAttPic.IsActive = false;
//                    _dbContext.Entity<MsEventIntendedForAtdPICStudent>().Update(existAttPic);
//                }

//                foreach (var existAttCheckStudent in existAttStudent.EventIntendedForAtdCheckStudents)
//                {
//                    existAttCheckStudent.IsActive = false;
//                    _dbContext.Entity<MsEventIntendedForAtdCheckStudent>().Update(existAttCheckStudent);
//                }
//            }
            
//            // create new intended for
//            CreateAndSaveIntendedFor(existEvent.Id, body.Dates, body);

//            // inactive existing event detail & user event
//            foreach (var existEventDetail in existEvent.EventDetails)
//            {
//                existEventDetail.IsActive = false;
//                _dbContext.Entity<MsEventDetail>().Update(existEventDetail);

//                foreach (var userEvent in existEventDetail.UserEvents)
//                {
//                    userEvent.IsActive = false;
//                    _dbContext.Entity<MsUserEvent>().Update(userEvent);
//                }
//            }

//            // create new event detail
//            var eventDetails = new List<MsEventDetail>(body.Dates.Count());
//            foreach (var date in body.Dates)
//            {
//                // var intersectEvents = await _dbContext.Entity<MsEventDetail>()
//                //     .Include(x => x.Event).ThenInclude(x => x.EventIntendedFor)
//                //     .Where(x 
//                //         => x.IdEvent != existEvent.Id
//                //         && x.Event.EventType.AcademicYear.IdSchool == ay.IdSchool
//                //         && (x.StartDate == date.Start || x.EndDate == date.End 
//                //         || (x.StartDate < date.Start
//                //             ? (x.EndDate > date.Start && x.EndDate < date.End) || x.EndDate > date.End
//                //             : (date.End > x.StartDate && date.End < x.EndDate) || date.End > x.EndDate)))
//                //     .ToListAsync(CancellationToken);
                
//                // // check date & time conflict with existing intersect event
//                // var conflictEvents = Enumerable.Empty<string>();
//                // if (intersectEvents.Count != 0)
//                // {
//                //     // get each date of new event
//                //     var eachDate = DateTimeUtil.ToEachDay(date.Start, date.End);

//                //     foreach (var (start, end) in eachDate)
//                //     {
//                //         // select event that intersect date & time with day
//                //         var dayOfEvents = intersectEvents.Where(x => DateTimeUtil.IsIntersect(x.StartDate, x.EndDate, start, end));
//                //         // select event that intersect time with day
//                //         var intersectDayOfEvents = dayOfEvents
//                //             .Where(x 
//                //                 => TimeSpanUtil.IsIntersect(x.StartDate.TimeOfDay, x.EndDate.TimeOfDay, start.TimeOfDay, end.TimeOfDay)
//                //                 && (body.Role == "ALL" || x.Event.EventIntendedFor.IntendedFor == body.Role)
//                //                 && x.Event.Name == body.Name);
                        
//                //         if (intersectDayOfEvents.Any())
//                //             conflictEvents = conflictEvents.Concat(intersectEvents.Select(x => x.Event.Name));
//                //     }
//                // }

//                // if (conflictEvents.Any())
//                //     throw new BadRequestException("There is another event with same name, intended for, date and time.");

//                var eventDetail = new MsEventDetail
//                {
//                    Id = Guid.NewGuid().ToString(),
//                    IdEvent = existEvent.Id,
//                    StartDate = date.Start,
//                    EndDate = date.End
//                };
//                eventDetails.Add(eventDetail);
//            }
//            _dbContext.Entity<MsEventDetail>().AddRange(eventDetails);

//            // get users by IntendedForVm without create PK
//            var userEvents = await GetUserEvents(ay.IdSchool, ay.IdAcademicYear, body);
            
//            // add userEvents to each eventDetail
//            foreach (var eventDetail in eventDetails)
//            {
//                foreach (var userEvent in userEvents)
//                {
//                    userEvent.Id = Guid.NewGuid().ToString();
//                    userEvent.IdEventDetail = eventDetail.Id;
//                    _dbContext.Entity<MsUserEvent>().Add(userEvent);
//                }
//            }

//            await _dbContext.SaveChangesAsync(CancellationToken);

//            return Request.CreateApiResult2();
//        }
//    }
//}
