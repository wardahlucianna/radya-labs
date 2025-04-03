//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using BinusSchool.Common.Constants;
//using BinusSchool.Common.Exceptions;
//using BinusSchool.Common.Extensions;
//using BinusSchool.Common.Functions.Handler;
//using BinusSchool.Common.Model;
//using BinusSchool.Common.Model.Enums;
//using BinusSchool.Common.Utils;
//using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
//using BinusSchool.Persistence.SchedulingDb.Abstractions;
//using BinusSchool.Persistence.SchedulingDb.Entities;
//using BinusSchool.Persistence.SchedulingDb.Entities.User;
//using BinusSchool.Scheduling.FnSchedule.CalendarEvent.Validator;
//using Microsoft.EntityFrameworkCore;

//namespace BinusSchool.Scheduling.FnSchedule.CalendarEvent
//{
//    public class AddCalendarEvent2Handler : FunctionsHttpSingleHandler
//    {
//        private readonly ISchedulingDbContext _dbContext;
//        private readonly IServiceProvider _provider;

//        public AddCalendarEvent2Handler(ISchedulingDbContext dbContext, IServiceProvider provider)
//        {
//            _dbContext = dbContext;
//            _provider = provider;
//        }

//        protected override async Task<ApiErrorResult<object>> Handler()
//        {
//            var body = await Request.GetBody<AddCalendarEvent2Request>();
//            (await new AddCalendarEvent2Validator(_provider).ValidateAsync(body)).EnsureValid();

//            var ay = await _dbContext.Entity<MsEventType>()
//                .Where(x => x.Id == body.IdEventType)
//                .Select(x => new { x.IdAcademicYear, x.AcademicYear.IdSchool })
//                .FirstOrDefaultAsync(CancellationToken);

//            var newEvent = new MsEvent
//            {
//                Id = Guid.NewGuid().ToString(),
//                IdAcademicYear = ay.IdAcademicYear,
//                IdEventType = body.IdEventType,
//                Name = body.Name,
//                IsShowOnCalendarAcademic = body.Role != RoleConstant.Student || body.ForStudent.ShowOnCalendarAcademic,
//                //AttendanceType = EventAttendanceType.NotSet // NOTE: this field should not exist
//            };
//            _dbContext.Entity<MsEvent>().Add(newEvent);

//            var newIntendedFor = new MsEventIntendedFor
//            {
//                Id = newEvent.Id,
//                IntendedFor = body.Role,
//                Option = body.Option
//            };
//            _dbContext.Entity<MsEventIntendedFor>().Add(newIntendedFor);

//            // generate entity of intended for
//            CreateAndSaveIntendedFor(newEvent.Id, body.Dates, body);

//            // generate event detail per date
//            var eventDetails = new List<MsEventDetail>(body.Dates.Count());
//            foreach (var date in body.Dates)
//            {
//                // var intersectEvents = await _dbContext.Entity<MsEventDetail>()
//                //     .Include(x => x.Event).ThenInclude(x => x.EventIntendedFor)
//                //     .Where(x
//                //         => x.Event.EventType.AcademicYear.IdSchool == ay.IdSchool
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

//                var newEventDetail = new MsEventDetail
//                {
//                    Id = Guid.NewGuid().ToString(),
//                    IdEvent = newEvent.Id,
//                    StartDate = date.Start,
//                    EndDate = date.End
//                };
//                eventDetails.Add(newEventDetail);
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

//        #region Protected Method

//        protected void CreateAndSaveIntendedFor(string idEvent, IEnumerable<DateTimeRange> eventRanges, CalendarEvent2IntendedForVm intendedForVm)
//        {
//            if (intendedForVm.Role == RoleConstant.Teacher)
//            {
//                if (intendedForVm.Option == EventOptionType.Grade)
//                {
//                    foreach (var idGrade in intendedForVm.ForTeacher.IdGrades)
//                    {
//                        var intendedForGrade = new MsEventIntendedForGrade
//                        {
//                            Id = Guid.NewGuid().ToString(),
//                            IdEventIntendedFor = idEvent,
//                            IdGrade = idGrade
//                        };
//                        _dbContext.Entity<MsEventIntendedForGrade>().Add(intendedForGrade);
//                    }
//                }
//                else if (intendedForVm.Option == EventOptionType.Department)
//                {
//                    foreach (var idDepartment in intendedForVm.ForTeacher.IdDepartments)
//                    {
//                        var intendedForDepartment = new MsEventIntendedForDepartment
//                        {
//                            Id = Guid.NewGuid().ToString(),
//                            IdEventIntendedFor = idEvent,
//                            IdDepartment = idDepartment
//                        };
//                        _dbContext.Entity<MsEventIntendedForDepartment>().Add(intendedForDepartment);
//                    }
//                }
//                else if (intendedForVm.Option == EventOptionType.Subject)
//                {
//                    foreach (var subject in intendedForVm.ForTeacher.Subjects)
//                    {
//                        var intendedForGradeSubject = new MsEventIntendedForGradeSubject
//                        {
//                            Id = Guid.NewGuid().ToString(),
//                            IdEventIntendedFor = idEvent,
//                            IdGrade = subject.IdGrade
//                        };
//                        _dbContext.Entity<MsEventIntendedForGradeSubject>().Add(intendedForGradeSubject);

//                        foreach (var idSubject in subject.IdSubjects)
//                        {
//                            var intendedForSubject = new MsEventIntendedForSubject
//                            {
//                                Id = Guid.NewGuid().ToString(),
//                                IdEventIntendedForGradeSubject = intendedForGradeSubject.Id,
//                                IdSubject = idSubject
//                            };
//                            _dbContext.Entity<MsEventIntendedForSubject>().Add(intendedForSubject);
//                        }
//                    }
//                }
//            }
//            else if (intendedForVm.Role == RoleConstant.Student)
//            {
//                if (intendedForVm.Option == EventOptionType.Personal)
//                {
//                    foreach (var idStudent in intendedForVm.ForStudent.IdStudents)
//                    {
//                        var intendedForPersonal = new MsEventIntendedForPersonalStudent
//                        {
//                            Id = Guid.NewGuid().ToString(),
//                            IdEventIntendedFor = idEvent,
//                            IdStudent = idStudent
//                        };
//                        _dbContext.Entity<MsEventIntendedForPersonalStudent>().Add(intendedForPersonal);
//                    }
//                }
//                else if (intendedForVm.Option == EventOptionType.Subject)
//                {
//                    foreach (var idSubject in intendedForVm.ForStudent.IdSubjects)
//                    {
//                        var intendedForSubject = new MsEventIntendedForSubjectStudent
//                        {
//                            Id = Guid.NewGuid().ToString(),
//                            IdEventIntendedFor = idEvent,
//                            IdSubject = idSubject
//                        };
//                        _dbContext.Entity<MsEventIntendedForSubjectStudent>().Add(intendedForSubject);
//                    }
//                }
//                else if (intendedForVm.Option == EventOptionType.Grade)
//                {
//                    foreach (var idHomeroom in intendedForVm.ForStudent.IdHomerooms)
//                    {
//                        var intendedForGrade = new MsEventIntendedForGradeStudent
//                        {
//                            Id = Guid.NewGuid().ToString(),
//                            IdEventIntendedFor = idEvent,
//                            IdHomeroom = idHomeroom
//                        };
//                        _dbContext.Entity<MsEventIntendedForGradeStudent>().Add(intendedForGrade);
//                    }
//                }

//                var attStudent = new MsEventIntendedForAttendanceStudent
//                {
//                    Id = Guid.NewGuid().ToString(),
//                    IdEventIntendedFor = idEvent,
//                    Type = intendedForVm.ForStudent.AttendanceOption,
//                    IsSetAttendance = intendedForVm.ForStudent.SetAttendanceEntry,
//                    IsRepeat = intendedForVm.ForStudent.RepeatAttendanceCheck ?? false
//                };
//                _dbContext.Entity<MsEventIntendedForAttendanceStudent>().Add(attStudent);

//                if (intendedForVm.ForStudent.SetAttendanceEntry)
//                {
//                    var attPic = new MsEventIntendedForAtdPICStudent
//                    {
//                        Id = Guid.NewGuid().ToString(),
//                        IdEventIntendedForAttendanceStudent = attStudent.Id,
//                        Type = intendedForVm.ForStudent.PicAttendance.Value,
//                        IdUser = intendedForVm.ForStudent.IdUserPic
//                    };
//                    _dbContext.Entity<MsEventIntendedForAtdPICStudent>().Add(attPic);

//                    var eachDates = eventRanges.SelectMany(x => DateTimeUtil.ToEachDay(x.Start, x.End));
//                    foreach (var eachDate in eachDates)
//                    {
//                        var attCheckDate = intendedForVm.ForStudent.RepeatAttendanceCheck ?? false
//                            ? intendedForVm.ForStudent.AttendanceCheckDates.FirstOrDefault()
//                            : intendedForVm.ForStudent.AttendanceCheckDates.FirstOrDefault(x => (x.StartDate - eachDate.start).TotalDays == 0);
//                        if (attCheckDate is null)
//                            throw new BadRequestException($"Attendance Check for date {eachDate.start:dd-MM-yyyy} is not found.");
                        
//                        foreach (var attCheck in attCheckDate.AttendanceChecks)
//                        {
//                            var attCheckStudent = new MsEventIntendedForAtdCheckStudent
//                            {
//                                Id = Guid.NewGuid().ToString(),
//                                IdEventIntendedForAttendanceStudent = attStudent.Id,
//                                StartDate = intendedForVm.ForStudent.RepeatAttendanceCheck ?? false ? eachDate.start : attCheckDate.StartDate,
//                                //EndDate = attCheckDate.Date?.End ?? default,
//                                StartTime = eachDate.start.TimeOfDay,
//                                EndTime = eachDate.end.TimeOfDay,
//                                CheckName = attCheck.Name,
//                                Time = TimeSpan.FromMinutes(attCheck.TimeInMinute),
//                                IsPrimary = attCheck.IsMandatory
//                            };
//                            _dbContext.Entity<MsEventIntendedForAtdCheckStudent>().Add(attCheckStudent);
//                        }
//                    }
//                }
//            }
//        }

//        protected async Task<IEnumerable<MsUserEvent>> GetUserEvents(string idSchool, string idAy, CalendarEvent2IntendedForVm intendedForVm)
//        {
//            FillConfiguration();

//            if (intendedForVm.Role == "ALL" || intendedForVm.Role == RoleConstant.Staff || intendedForVm.Role == RoleConstant.Parent)
//            {
//                var predicate = PredicateBuilder.Create<MsUser>(x => x.UserSchools.Any(y => y.IdSchool == idSchool));
//                if (intendedForVm.Role != "ALL")
//                    predicate = predicate.And(x => x.UserRoles.Any(y => y.Role.RoleGroup.Code == intendedForVm.Role));
                
//                var idUsers = await _dbContext.Entity<MsUser>()
//                    .Where(predicate)
//                    .Select(x => x.Id)
//                    .ToListAsync(CancellationToken);

//                if (idUsers.Count == 0)
//                    throw new BadRequestException($"There is no user with role {intendedForVm.Role} in this school.");
                
//                return idUsers.Select(x => new MsUserEvent { IdUser = x });
//            }
//            else if (intendedForVm.Role == RoleConstant.Teacher)
//            {
//                var idSubjects = Enumerable.Empty<string>();
//                if (intendedForVm.Option == EventOptionType.Subject)
//                    idSubjects = intendedForVm.ForTeacher.Subjects.SelectMany(x => x.IdSubjects);
                
//                var predicate = PredicateBuilder.True<MsLessonTeacher>();
//                predicate = intendedForVm.Option switch
//                {
//                    EventOptionType.Grade => predicate.And(x => intendedForVm.ForTeacher.IdGrades.Contains(x.Lesson.IdGrade)),
//                    EventOptionType.Department => predicate.And(x => intendedForVm.ForTeacher.IdDepartments.Contains(x.Lesson.Subject.IdDepartment)),
//                    EventOptionType.Subject => predicate.And(x => idSubjects.Contains(x.Lesson.IdSubject)),
//                    _ => throw new BadRequestException(null)
//                };
                
//                var idTeachers = await _dbContext.Entity<MsLessonTeacher>()
//                    .Where(predicate)
//                    .Select(x => x.IdUser)
//                    .Distinct()
//                    .ToListAsync(CancellationToken);

//                return idTeachers.Select(x => new MsUserEvent { IdUser = x });
//            }
//            else if (intendedForVm.Role == RoleConstant.Student)
//            {
//                if (intendedForVm.Option == EventOptionType.Personal)
//                {
//                    return intendedForVm.ForStudent.IdStudents.Select(x => new MsUserEvent
//                    {
//                        IdUser = x
//                    }).ToList();
//                }
//                else if (intendedForVm.Option == EventOptionType.Subject)
//                {
//                    var students = _dbContext.Entity<MsHomeroomStudentEnrollment>()
//                        .Include(x => x.Subject)
//                        .Include(x => x.HomeroomStudent)
//                            .ThenInclude(x => x.Student)
//                        .Where(x => intendedForVm.ForStudent.IdSubjects.Contains(x.IdSubject))
//                        .GroupBy(x => x.HomeroomStudent.IdStudent)
//                        .Select(x => x.Key).ToList();
//                    return _dbContext.Entity<MsUser>()
//                        .Where(x => students.Any(p => p == x.Id))
//                        .Select(x => new MsUserEvent
//                        {
//                            IdUser = x.Id
//                        })
//                        .ToList();

//                }
//                else if (intendedForVm.Option == EventOptionType.Grade)
//                {
//                    var students = _dbContext.Entity<MsHomeroomStudent>()
//                        .Include(x => x.Homeroom)
//                        .Include(x => x.Student)
//                        .Where(x => intendedForVm.ForStudent.IdHomerooms.Contains(x.IdHomeroom))
//                        .GroupBy(x => x.IdStudent)
//                        .Select(x => x.Key).ToList();
//                    return _dbContext.Entity<MsUser>()
//                       .Where(x => students.Any(p => p == x.Id))
//                       .Select(x => new MsUserEvent
//                       {
//                           IdUser = x.Id
//                       })
//                       .ToList();
//                }
//            }
//            return Enumerable.Empty<MsUserEvent>();
//        }

//        #endregion
//    }
//}
