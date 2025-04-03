using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnLessonPlan.WeekSetting
{
    public class GetWeekSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        private readonly IMachineDateTime _dateTime;

        public GetWeekSettingHandler(
            ITeachingDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetWeekSettingRequest>(nameof(GetWeekSettingRequest.IdAcademicYear));
            var columns = new[] { "academicYear", "level", "grade", "term", "totalWeek","method","status","code" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "level.academicYear.code" },
                { columns[1], "level.code" }
            };

            var query = _dbContext.Entity<MsWeekSetting>()
                .Include(x => x.Period)
                        .ThenInclude(x => x.Grade)
                            .ThenInclude(x => x.Level)
                                .ThenInclude(ay => ay.AcademicYear)
                .Include(x => x.WeekSettingDetails)
                    .ThenInclude(x => x.LessonPlans)
                .AsQueryable();
                            
            if (param.IdAcademicYear != null)
                query = query.Where(x => x.Period.Grade.Level.AcademicYear.Id == param.IdAcademicYear);

            if (param.IdLevel != null)
                query = query.Where(x => x.Period.Grade.Level.Id == param.IdLevel);

            if (param.IdGrade != null)
                query = query.Where(x => x.Period.Grade.Id == param.IdGrade);

            if (param.IdPeriod != null)
                query = query.Where(x => x.Period.Id == param.IdPeriod);

            if (param.Method != null)
                query = query.Where(x => x.Method == param.Method);

            // query = query.OrderByDescending(x => x.DateIn);

            query = param.OrderBy switch
            {
                "academicYear" => param.OrderType == OrderType.Desc
                    ? query.OrderByDescending(x => x.Period.Grade.Level.AcademicYear.Code) 
                        : query.OrderBy(x => x.Period.Grade.Level.AcademicYear.Code),
                "level" => param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Period.Grade.Level.Code) 
                        : query.OrderBy(x => x.Period.Grade.Level.Code),      
                "term" => param.OrderType == OrderType.Desc
                    ? query.OrderByDescending(x => x.Period.Description) 
                        : query.OrderBy(x => x.Period.Description),
                "totalWeek" => param.OrderType == OrderType.Desc
                    ? query.OrderByDescending(x => x.WeekSettingDetails.Count) 
                        : query.OrderBy(x => x.WeekSettingDetails.Count),  
                "method" => param.OrderType == OrderType.Desc
                    ? query.OrderByDescending(x => x.Method) 
                        : query.OrderBy(x => x.Method),    
                "status" => param.OrderType == OrderType.Desc
                    ? query.OrderByDescending(x => x.Status) 
                        : query.OrderBy(x => x.Status),
                "grade" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Period.Grade.Description.Length).ThenBy(x => x.Period.Grade.Description)
                    : query.OrderByDescending(x => x.Period.Grade.Description.Length).ThenByDescending(x => x.Period.Grade.Description),
                _ => query.OrderByDynamic(param, aliasColumns)
            };

            var msWeekSetting = await query.ToListAsync(CancellationToken);
            var weekSetting = msWeekSetting
                .Select(x => new GetWeekSettingResponse
                {
                    Id = x.Id,
                    AcademicYear = x.Period.Grade.Level.AcademicYear.Code,
                    Level = x.Period.Grade.Level.Code,
                    Grade = x.Period.Grade.Description,
                    Term = x.Period.Description,
                    TotalWeek = x.WeekSettingDetails.Count() + " Week",
                    Method = x.Method,
                    Status = x.Status == false ? "No Deadline Set" : "Deadline Set",
                    // IsDeletable = !x.WeekSettingDetails.Any(y => _dateTime.ServerTime > y.DeadlineDate || y.LessonPlans.Any(z => z.Status != "Unsubmitted")),
                    IsDeletable = !x.WeekSettingDetails.Any(y => y.LessonPlans.Any(z => z.Status != "Unsubmitted")),
                    DateSettings = x.WeekSettingDetails.Select(y => new DateSetting
                    {
                        IdWeekSettingDetail = y.Id,
                        WeekNumber = y.WeekNumber,
                        DeadlineDate = y.DeadlineDate,
                        Status = y.Status
                    }).OrderBy(x => x.WeekNumber).ToList()
                }).ToList();

            if (!string.IsNullOrWhiteSpace(param.Search))
                weekSetting = weekSetting.Where(x
                    => x.AcademicYear.ToLower().Contains(param.Search.ToLower())
                    || x.Level.ToLower().Contains(param.Search.ToLower())
                    || x.Grade.ToLower().Contains(param.Search.ToLower())
                    || x.Term.ToLower().Contains(param.Search.ToLower())
                    || x.TotalWeek.ToLower().Contains(param.Search.ToLower())
                    || x.Method.ToLower().Contains(param.Search.ToLower())
                    ).ToList();

            var searchedWeekSetting = weekSetting.SetPagination(param).ToList();

            var count = param.CanCountWithoutFetchDb(searchedWeekSetting.Count()) 
                ? searchedWeekSetting.Count 
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(searchedWeekSetting as object, param.CreatePaginationProperty(count));
        }
    }
}
