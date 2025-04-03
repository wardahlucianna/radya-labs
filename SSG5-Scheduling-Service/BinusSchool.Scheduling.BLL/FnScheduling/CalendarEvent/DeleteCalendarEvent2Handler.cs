//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using BinusSchool.Common.Constants;
//using BinusSchool.Common.Extensions;
//using BinusSchool.Common.Functions.Handler;
//using BinusSchool.Common.Model;
//using BinusSchool.Common.Utils;
//using BinusSchool.Persistence.SchedulingDb.Abstractions;
//using BinusSchool.Persistence.SchedulingDb.Entities;
//using Microsoft.EntityFrameworkCore;

//namespace BinusSchool.Scheduling.FnSchedule.CalendarEvent
//{
//    public class DeleteCalendarEvent2Handler : FunctionsHttpSingleHandler
//    {
//        private readonly ISchedulingDbContext _dbContext;

//        public DeleteCalendarEvent2Handler(ISchedulingDbContext dbContext)
//        {
//            _dbContext = dbContext;
//        }

//        protected override async Task<ApiErrorResult<object>> Handler()
//        {
//            var ids = (await GetIdsFromBody()).Distinct();
            
//            var items = await _dbContext.Entity<MsEvent>()
//                .Include(x => x.EventDetails).ThenInclude(x => x.UserEvents)
//                .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForGrades)
//                .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForDepartments)
//                .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForGradeSubjects).ThenInclude(x => x.EventIntendedForSubjects)
//                .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForPersonalStudents)
//                .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForSubjectStudents)
//                .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForGradeStudents)
//                .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForAttendanceStudents)
//                .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdPICStudents)
//                .Include(x => x.EventIntendedFor).ThenInclude(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdCheckStudents)
//                .Where(x => ids.Contains(x.Id))
//                .ToListAsync(CancellationToken);

//            var undeleted = new UndeletedResult2();

//            // find not found ids
//            ids = ids.Except(ids.Intersect(items.Select(x => x.Id)));
//            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

//            // find already used ids
//            foreach (var item in items)
//            {
//                var cantDelete = item.EventDetails.Any(x => DateTimeUtil.IsIntersect(x.StartDate, x.EndDate, DateTimeUtil.ServerTime, DateTimeUtil.ServerTime));
//                if (cantDelete)
//                {
//                    undeleted.CurrentlyRun ??= new Dictionary<string, string>();
//                    undeleted.CurrentlyRun.Add(item.Id, string.Format(Localizer["ExCurrentlyRun"], item.Name));
//                }
//                else
//                {
//                    item.IsActive = false;
//                    _dbContext.Entity<MsEvent>().Update(item);

//                    foreach (var eventDetail in item.EventDetails)
//                    {
//                        eventDetail.IsActive = false;
//                        _dbContext.Entity<MsEventDetail>().Update(eventDetail);

//                        foreach (var userEvent in eventDetail.UserEvents)
//                        {
//                            userEvent.IsActive = false;
//                            _dbContext.Entity<MsUserEvent>().Update(userEvent);
//                        }
//                    }

//                    item.EventIntendedFor.IsActive = false;
//                    _dbContext.Entity<MsEventIntendedFor>().Update(item.EventIntendedFor);

//                    if (item.EventIntendedFor.IntendedFor == RoleConstant.Teacher)
//                    {
//                        foreach (var intendedForGrade in item.EventIntendedFor.EventIntendedForGrades)
//                        {
//                            intendedForGrade.IsActive = false;
//                            _dbContext.Entity<MsEventIntendedForGrade>().Update(intendedForGrade);
//                        }
//                        foreach (var intendedForDepartment in item.EventIntendedFor.EventIntendedForDepartments)
//                        {
//                            intendedForDepartment.IsActive = false;
//                            _dbContext.Entity<MsEventIntendedForDepartment>().Update(intendedForDepartment);
//                        }
//                        foreach (var intendedForGradeSubject in item.EventIntendedFor.EventIntendedForGradeSubjects)
//                        {
//                            intendedForGradeSubject.IsActive = false;
//                            _dbContext.Entity<MsEventIntendedForGradeSubject>().Update(intendedForGradeSubject);

//                            foreach (var intendedForSubject in intendedForGradeSubject.EventIntendedForSubjects)
//                            {
//                                intendedForGradeSubject.IsActive = false;
//                                _dbContext.Entity<MsEventIntendedForSubject>().Update(intendedForSubject);
//                            }
//                        }
//                    }
//                    else if (item.EventIntendedFor.IntendedFor == RoleConstant.Student)
//                    {
//                        foreach (var intendedForPersonal in item.EventIntendedFor.EventIntendedForPersonalStudents)
//                        {
//                            intendedForPersonal.IsActive = false;
//                            _dbContext.Entity<MsEventIntendedForPersonalStudent>().Update(intendedForPersonal);
//                        }
//                        foreach (var intendedForSubject in item.EventIntendedFor.EventIntendedForSubjectStudents)
//                        {
//                            intendedForSubject.IsActive = false;
//                            _dbContext.Entity<MsEventIntendedForSubjectStudent>().Update(intendedForSubject);
//                        }
//                        foreach (var intendedForGrade in item.EventIntendedFor.EventIntendedForGradeStudents)
//                        {
//                            intendedForGrade.IsActive = false;
//                            _dbContext.Entity<MsEventIntendedForGradeStudent>().Update(intendedForGrade);
//                        }

//                        foreach (var attStudent in item.EventIntendedFor.EventIntendedForAttendanceStudents)
//                        {
//                            attStudent.IsActive = false;
//                            _dbContext.Entity<MsEventIntendedForAttendanceStudent>().Update(attStudent);

//                            foreach (var attPic in attStudent.EventIntendedForAtdPICStudents)
//                            {
//                                attPic.IsActive = false;
//                                _dbContext.Entity<MsEventIntendedForAtdPICStudent>().Update(attPic);
//                            }
//                            foreach (var attCheckStudent in attStudent.EventIntendedForAtdCheckStudents)
//                            {
//                                attCheckStudent.IsActive = false;
//                                _dbContext.Entity<MsEventIntendedForAtdCheckStudent>().Update(attCheckStudent);
//                            }
//                        }
//                    }
//                }
//            }

//            await _dbContext.SaveChangesAsync(CancellationToken);
            
//            return ProceedDeleteResult(undeleted.AsErrors());
//        }
//    }
//}
