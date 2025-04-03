using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetDetailGenerateScheduleHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDetailGenerateScheduleHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            var result = new List<GetDetailGenerateScheduleResult>();
            var param = Request.ValidateParams<GetDetailGenerateScheduleRequest>(
               nameof(GetDetailGenerateScheduleRequest.IdAcademicYears),
               nameof(GetDetailGenerateScheduleRequest.IdGrade),
               nameof(GetDetailGenerateScheduleRequest.DateMountYers)
               );

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x =>
                                            x.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.TrAscTimetable.IdAcademicYear == param.IdAcademicYears
                                            && x.GeneratedScheduleStudent.GeneratedScheduleGrade.IdGrade == param.IdGrade
                                            && x.ScheduleDate.Month == param.DateMountYers.Value.Month
                                            && x.ScheduleDate.Year == param.DateMountYers.Value.Year);

            if (!string.IsNullOrWhiteSpace(param.IdAscTimetable))
                predicate = predicate.And(p => p.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable);
            if (!string.IsNullOrWhiteSpace(param.IdClass))
                predicate = predicate.And(p => p.IdHomeroom == param.IdClass);
            if (!string.IsNullOrWhiteSpace(param.IdSubject))
                predicate = predicate.And(p => p.IdSubject == param.IdSubject);

            var getData = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                         .Include(p => p.GeneratedScheduleStudent.GeneratedScheduleGrade.GeneratedSchedule)
                                         .Include(p => p.Week)
                                         .Where(predicate);

            if (getData.Count() == 0)
            {
                return Task.FromResult(Request.CreateApiResult2(result as object));
            }

            var group = getData.AsEnumerable().GroupBy(p => new
            {
                p.ScheduleDate,
                p.StartTime,
                p.EndTime
            });

            foreach (var item in group)
            {
                var getTriItem = item.Take(3);
                foreach (var dataItem in getTriItem)
                {
                    var _data = new GetDetailGenerateScheduleResult();
                    _data.ClassId = dataItem.ClassID;
                    _data.EndDate = dataItem.ScheduleDate;
                    _data.StartDate = dataItem.ScheduleDate;
                    _data.Week = dataItem.Week.Description;
                    _data.Venue = new CodeWithIdVm
                    {
                        Id = dataItem.IdVenue,
                        Description = dataItem.VenueName,

                    };
                    _data.Teacher = new CodeWithIdVm
                    {
                        Id = dataItem.IdUser,
                        Description = dataItem.TeacherName,

                    };
                    _data.Summary = dataItem.ClassID;
                    _data.Session = new SessionVm
                    {
                        Id = dataItem.IdSession,
                        StartTime = dataItem.StartTime,
                        EndTime = dataItem.EndTime,
                        SessionID = dataItem.SessionID,
                    };
                    _data.Subject = new CodeWithIdVm
                    {
                        Id = dataItem.IdSubject,
                        Description = dataItem.SubjectName,
                        Code = dataItem.SubjectName
                    };
                    _data.Homeroom = new CodeWithIdVm
                    {
                        Id = dataItem.IdHomeroom,
                        Description = dataItem.HomeroomName,
                        Code = dataItem.HomeroomName
                    };
                    result.Add(_data);
                }
            }
            return Task.FromResult(Request.CreateApiResult2(result as object));
        }
    }
}
