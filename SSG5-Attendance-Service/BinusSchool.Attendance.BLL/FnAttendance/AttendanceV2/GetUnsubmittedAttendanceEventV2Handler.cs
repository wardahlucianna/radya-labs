using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class GetUnsubmittedAttendanceEventV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        public GetUnsubmittedAttendanceEventV2Handler(IAttendanceDbContext dbContext, IMachineDateTime Datetime)
        {
            _dbContext = dbContext;
            _datetime = Datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnsubmittedAttendanceEventRequest>();

            var listHomeroomStudentEnroolment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                       .Include(e => e.HomeroomStudent)
                       .Where(x => x.Lesson.IdAcademicYear == param.idAcademicYear)
                       .Select(e => new
                       {
                           e.IdLesson,
                           e.HomeroomStudent.IdHomeroom,
                           e.HomeroomStudent.IdStudent
                       })
                       .AsNoTracking().ToListAsync();

            var listEvent = await _dbContext.Entity<TrEvent>()
                            .Include(e => e.EventDetails).ThenInclude(e=>e.UserEvents).ThenInclude(e=>e.UserEventAttendance2s)
                            .Where(x => x.IdAcademicYear == param.idAcademicYear
                                        && x.EventIntendedFor.Any(e => e.IntendedFor == "STUDENT")
                                        && x.EventIntendedFor.Any(e => e.EventIntendedForAttendanceStudents.Any(e => e.IsSetAttendance))
                                        && x.EventIntendedFor.Any(e => e.EventIntendedForAttendanceStudents.Any(e => e.Type == EventIntendedForAttendanceStudent.Mandatory))
                                        && x.EventDetails.Any(e => e.StartDate.Date <= _datetime.ServerTime.Date)
                                        && x.StatusEvent == "Approved"
                                        && x.EventIntendedFor.Any(e => e.EventIntendedForAttendanceStudents.Any(g => g.EventIntendedForAtdPICStudents.Any(f => f.IdUser == param.idUser)))
                                       && !x.EventDetails.Any(e => e.UserEvents.Any(z => z.UserEventAttendance2s.Any()))
                                        )
                            .AsNoTracking().ToListAsync(CancellationToken);
            var listIdEvent = listEvent.Select(e=>e.Id).ToList();
            var listEventIntendedFor = await _dbContext.Entity<TrEvent>()
                           .Include(e => e.EventDetails)
                           .Include(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForAttendanceStudents).ThenInclude(e => e.EventIntendedForAtdCheckStudents)
                           .Where(x => listIdEvent.Contains(x.Id))
                           .AsNoTracking().ToListAsync(CancellationToken);

            List<GetUnsubmittedAttendanceEventV2Result> result = new List<GetUnsubmittedAttendanceEventV2Result>();
            foreach (var itemEvent in listEvent)
            {
                var listEventDate = listEventIntendedFor.Where(e => e.Id == itemEvent.Id)
                                    .SelectMany(e=>e.EventDetails.Select(f => new
                                    {
                                        f.StartDate,
                                        f.EndDate,
                                    }))
                                    .Distinct().ToList();

                var EventIntendedForAtdCheckStudent = listEventIntendedFor.Where(e => e.Id == itemEvent.Id)
                                    .SelectMany(e => e.EventIntendedFor.SelectMany(f=>f.EventIntendedForAttendanceStudents.SelectMany(g=>g.EventIntendedForAtdCheckStudents))).FirstOrDefault();

                if(EventIntendedForAtdCheckStudent==null)
                    continue;

                foreach (var itemDateEvent in listEventDate)
                {
                    result.Add(new GetUnsubmittedAttendanceEventV2Result
                    {
                        IdEvent = itemEvent.Id,
                        EventName = itemEvent.Name,
                        StartDate = itemDateEvent.StartDate,
                        EndDate = itemDateEvent.EndDate,
                        AttendanceCheckName = EventIntendedForAtdCheckStudent.CheckName,
                        AttendanceTime = EventIntendedForAtdCheckStudent.Time,
                    });
                }
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
