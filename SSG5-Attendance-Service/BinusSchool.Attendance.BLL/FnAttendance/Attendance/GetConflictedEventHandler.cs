using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class GetConflictedEventHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        public GetConflictedEventHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var result = new List<GetStudentConflictedEventsResult>();
            // get homerooms that teached by user
            var homerooms = await _dbContext.Entity<MsHomeroom>()
                                            .Include(x => x.HomeroomTeachers)
                                            .Include(x => x.HomeroomStudents)
                                            .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                                            .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade)
                                            .Where(x => x.HomeroomTeachers.Any(y => y.IdBinusian == AuthInfo.UserId && y.IsAttendance))
                                            .ToListAsync();

            foreach (var homeroom in homerooms)
            {
                // get conflicted event on each student
                // var studentsIds = homeroom.HomeroomStudents.Select(x => x.IdStudent).ToList();
                // var events = await _dbContext.Entity<TrUserEventAttendance>()
                //                              .Include(x => x.UserEvent).ThenInclude(x => x.User)
                //                              .Include(x => x.EventIntendedForAttendanceCheckStudent).ThenInclude(x => x.EventIntendedForAttendanceStudent)
                //                                 .ThenInclude(x => x.EventIntendedFor).ThenInclude(x => x.Event).ThenInclude(x => x.EventDetails)
                //                              .Where(x => !x.HasBeenResolved
                //                                          && !x.HasBeenChoose
                //                                          && studentsIds.Contains(x.UserEvent.IdUser)
                //                                          && x.EventIntendedForAttendanceCheckStudent.IsPrimary)
                //                              .ToListAsync();
                // var conflicteds = events.GroupBy(x => new { x.UserEvent.User, x.EventIntendedForAttendanceCheckStudent.StartDate });
                // if (conflicteds.Any(x => x.Count() >= 2))
                // {
                //     // add to result for any conflicted event
                //     foreach (var conflict in conflicteds.Where(x => x.Count() >= 2))
                //     {
                //         if (!result.Any(x => x.ConflictCode == string.Join("_", conflict.Select(x => x.Id))))
                //             result.Add(new GetStudentConflictedEventsResult
                //             {
                //                 ConflictCode = string.Join("_", conflict.Select(x => x.Id)),
                //                 Homeroom = new ItemValueVm
                //                 {
                //                     Id = homeroom.Id,
                //                     Description = $"{homeroom.GradePathwayClassroom.GradePathway.Grade.Code}{homeroom.GradePathwayClassroom.Classroom.Code}"
                //                 },
                //                 Student = new NameValueVm
                //                 {
                //                     Id = conflict.Key.User.Id,
                //                     Name = conflict.Key.User.DisplayName
                //                 },
                //                 ConflictEvents = conflict.Select(x => new ConflictedEventResult
                //                 {
                //                     Id = x.EventIntendedForAttendanceCheckStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.Id,
                //                     Description = x.EventIntendedForAttendanceCheckStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.Name,
                //                     Dates = x.EventIntendedForAttendanceCheckStudent.EventIntendedForAttendanceStudent
                //                              .EventIntendedFor.Event.EventDetails.Select(y => new EventDate
                //                              {
                //                                  StartDate = y.StartDate,
                //                                  StartTime = y.StartDate.TimeOfDay,
                //                                  EndDate = y.EndDate,
                //                                  EndTime = y.EndDate.TimeOfDay
                //                              }).ToList(),
                //                     EventCheck = new EventCheck
                //                     {
                //                         Id = x.EventIntendedForAttendanceCheckStudent.Id,
                //                         Description = x.EventIntendedForAttendanceCheckStudent.CheckName,
                //                         Date = x.EventIntendedForAttendanceCheckStudent.StartDate,
                //                         AttendanceTime = x.EventIntendedForAttendanceCheckStudent.Time
                //                     }
                //                 }).ToList()
                //             });
                //     }
                // }
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
