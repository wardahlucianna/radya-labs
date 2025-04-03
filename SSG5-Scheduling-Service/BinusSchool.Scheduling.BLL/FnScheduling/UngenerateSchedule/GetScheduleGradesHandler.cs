using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.UngenerateSchedule
{
    public class GetScheduleGradesHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetScheduleGradesHandler(
            ISchedulingDbContext dbContext,
            IApiService<IGrade> gradeService)
        {
            _dbContext = dbContext;

        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetScheduleGradesRequest>(nameof(GetScheduleGradesRequest.IdAscTimetable));
            var gradeIds = await _dbContext.Entity<TrGeneratedScheduleGrade>()
                                           .Include(x => x.GeneratedSchedule)
                                           .Where(x => x.GeneratedSchedule.IdAscTimetable == param.IdAscTimetable
                                                       && (param.StartDate.Date >= x.StartPeriod.Date || param.EndDate.Date >= x.StartPeriod.Date))
                                           .Select(x => x.IdGrade)
                                           .Distinct()
                                           .ToListAsync();

            var columns = new[] { "acadyear", "level", "description", "code" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "level.academicYear.code" },
                { columns[1], "level.code" }
            };

            var predicate = PredicateBuilder.Create<MsSubject>(x => param.IdSchool.Any(y => y == x.Grade.Level.AcademicYear.School.Id));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Grade.Level.Description, $"%{param.Search}%")
                    || EF.Functions.Like(x.Grade.Level.AcademicYear.Description, $"%{param.Search}%"));

            if (gradeIds.Any())
                predicate = predicate.And(x => gradeIds.Contains(x.IdGrade));

            var query = _dbContext.Entity<MsSubject>()
                .SearchByIds(param)
                .Where(predicate);

            query = param.OrderBy switch
            {
                "code" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Code.Length).ThenBy(x => x.Code)
                    : query.OrderByDescending(x => x.Code.Length).ThenByDescending(x => x.Code),
                "description" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Description.Length).ThenBy(x => x.Description)
                    : query.OrderByDescending(x => x.Description.Length).ThenByDescending(x => x.Description),
                _ => query.OrderByDynamic(param, aliasColumns)
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new CodeWithIdVm(x.Id, x.Code, x.Description))
                    .ToListAsync();
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetGradeResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Acadyear = new CodeWithIdVm
                        {
                            Id = x.Grade.Level.IdAcademicYear,
                            Code = x.Grade.Level.AcademicYear.Code,
                            Description = x.Grade.Level.AcademicYear.Description
                        },
                        Level = new CodeWithIdVm
                        {
                            Id = x.Grade.IdLevel,
                            Code = x.Grade.Level.Code,
                            Description = x.Grade.Level.Description
                        },
                        School = new CodeWithIdVm
                        {
                            Id = x.Grade.Level.AcademicYear.IdSchool,
                            Code = x.Grade.Level.AcademicYear.School.Name,
                            Description = x.Grade.Level.AcademicYear.School.Description
                        }
                    })
                    .ToListAsync();
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync();

            var result = Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));

            return new ApiErrorResult<object>
            {
                IsSuccess = result.IsSuccess,
                StatusCode = result.StatusCode,
                Errors = result.Errors,
                Message = result.Message,
                InnerMessage = result.InnerMessage,
                Path = result.Path,
                Properties = result.Properties,
                Payload = result.Payload
            };
        }
    }
}
