using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventAcademicHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredProps = new Lazy<string[]>(new[]
        {
            nameof(GetSchoolEventAcademicRequest.IdAcadyear),
            nameof(GetSchoolEventAcademicRequest.IdLevel),
            nameof(GetSchoolEventAcademicRequest.StartDate),
            nameof(GetSchoolEventAcademicRequest.EndDate),
        });

        private readonly ISchedulingDbContext _dbContext;
        
        public GetSchoolEventAcademicHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSchoolEventAcademicRequest>(_requiredProps.Value);

            // convert param date to start until end day
            param.StartDate = new DateTime(param.StartDate.Year, param.StartDate.Month, param.StartDate.Day, 0, 0, 0);
            param.EndDate = new DateTime(param.EndDate.Year, param.EndDate.Month, param.EndDate.Day, 23, 59, 59);

            var predicate = PredicateBuilder.Create<TrEvent>(x => x.IdAcademicYear == param.IdAcadyear && x.IsShowOnCalendarAcademic);
            predicate = predicate.And(x => x.EventDetails.Any(y
                => y.StartDate == param.StartDate || y.EndDate == param.EndDate 
                || (y.StartDate < param.StartDate   
                    ? (y.EndDate > param.StartDate && y.EndDate < param.EndDate) || y.EndDate > param.EndDate
                    : (param.EndDate > y.StartDate && param.EndDate < y.EndDate) || param.EndDate > y.EndDate)));

            if(param.IdLevel != null)
            {
                 predicate = predicate.And(x => 
                    x.EventIntendedFor.Any(y => y.EventIntendedForGradeStudents.Any(z => z.Homeroom.Grade.IdLevel == param.IdLevel)
                    || x.EventIntendedFor.Any(y => y.EventIntendedForLevelStudents.Any(z => z.IdLevel == param.IdLevel))
                    || x.EventIntendedFor.Any(y => y.IntendedFor == "ALL")
                    ));
            }

            var events = await _dbContext.Entity<TrEvent>()
                .Include(x => x.EventIntendedFor)
                .Where(predicate)
                .Select(x => new GetSchoolEventAcademicResult
                {
                    Id = x.Id,
                    Name = x.Name,
                    Dates = x.EventDetails.Select(y => new DateTimeRange
                    {
                        Start = y.StartDate,
                        End = y.EndDate
                    }),
                    EventType = new CalendarEventTypeVm
                    {
                        Id = x.IdEventType,
                        Code = x.EventType.Code,
                        Description = x.EventType.Description,
                        Color = x.EventType.Color
                    },
                    IntendedFor = string.Join(", ", x.EventIntendedFor.Select(y => y.IntendedFor)),
                    LastUpdate = x.DateUp ?? x.DateIn ?? DateTime.MinValue
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(events as object);
        }
    }
}
