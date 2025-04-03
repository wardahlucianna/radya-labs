using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetStudentAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetStudentAttendanceHandler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        private static readonly string[] _columns = { "Date", "Session" };

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentAttendanceRequest>(nameof(GetStudentAttendanceRequest.IdSchool), nameof(GetStudentAttendanceRequest.Date), nameof(GetStudentAttendanceRequest.IdHomeroom), nameof(GetStudentAttendanceRequest.ClassId), nameof(GetStudentAttendanceRequest.SessionID));

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.ScheduleDate == param.Date && x.IdHomeroom == param.IdHomeroom && x.ClassID == param.ClassId && x.SessionID == param.SessionID);

            var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                 .Include(x => x.GeneratedScheduleStudent).ThenInclude(x => x.Student)
                                 .Include(x => x.User)
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Include(x => x.Venue)
                                 .Where(predicate);

            //ordering
            switch (param.OrderBy)
            {
                case "Date":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ScheduleDate)
                        : query.OrderBy(x => x.ScheduleDate);
                    break;

                case "Session":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.SessionID)
                        : query.OrderBy(x => x.SessionID);
                    break;
            };

            IReadOnlyList<GetStudentAttendanceResult> items;
            IReadOnlyList<GetStudentAttendanceResult> dataItems;

            items = await query
                .Select(x => new GetStudentAttendanceResult
                    {
                        
                        IdHomeroom = x.IdHomeroom,
                        HomeroomName = x.HomeroomName,
                        ClassId = x.ClassID,
                        StudentId = x.GeneratedScheduleStudent.IdStudent,
                        StudentName = x.GeneratedScheduleStudent.Student.FirstName + " " + x.GeneratedScheduleStudent.Student.LastName 
                    }
                )
                .Distinct()
                .OrderBy(x => x.StudentId)
                .SetPagination(param)
                .ToListAsync(CancellationToken);

            dataItems = items
                    .Select(x => new GetStudentAttendanceResult
                    {
                        IdHomeroom = x.IdHomeroom,
                        HomeroomName = x.HomeroomName,
                        ClassId = x.ClassId,
                        StudentId = x.StudentId,
                        StudentName = x.StudentName
                    
                    }    
                ).ToList();

            var countAll = await query
                .Select(x => new GetStudentAttendanceResult
                    {
                        
                        IdHomeroom = x.IdHomeroom,
                        HomeroomName = x.HomeroomName,
                        ClassId = x.ClassID,
                        StudentId = x.GeneratedScheduleStudent.IdStudent,
                        StudentName = x.GeneratedScheduleStudent.Student.FirstName + " " + x.GeneratedScheduleStudent.Student.LastName 
                    }
                )
                .Distinct().CountAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : countAll;

            return Request.CreateApiResult2(dataItems as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
