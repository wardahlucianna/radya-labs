using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Session
{
    public class GetSessionHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _columns = new Lazy<string[]>(new[] 
        { 
            "grade", "pathway", "schoolDay", "name", "alias", "startTime", "endTime", "durationInMinutes", "sessionID"
        });
        private static readonly Lazy<IDictionary<string, string>> _aliasColumns = new Lazy<IDictionary<string, string>>(new Dictionary<string, string>
        {
            { _columns.Value[0], "gradePathway.grade.description" }
        });
        
        private readonly ISchedulingDbContext _dbContext;

        public GetSessionHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetSessionRequest>();
            var predicate = PredicateBuilder.True<MsSession>();

            if (!(param.Ids?.Any() ?? false) && (!(param.IdSchool?.Any() ?? false) || string.IsNullOrEmpty(param.IdSessionSet)))
                throw new BadRequestException("Please provide value for parameter ids or idSchool & idSessionSet.");

            if (param.IdSchool?.Any() ?? false)
                predicate = predicate.And(x => param.IdSchool.Contains(x.SessionSet.IdSchool));
            if (!string.IsNullOrEmpty(param.IdSessionSet))
                predicate = predicate.And(x => x.IdSessionSet == param.IdSessionSet);
            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.GradePathway.Grade.Level.IdAcademicYear == param.IdAcadyear);
            if (!string.IsNullOrWhiteSpace(param.IdGrade))
                predicate = predicate.And(x => x.GradePathway.IdGrade == param.IdGrade);
            if (!string.IsNullOrWhiteSpace(param.IdPathway))
                predicate = predicate.And(x => x.GradePathway.GradePathwayDetails.Any(y => y.IdPathway == param.IdPathway));
            if (!string.IsNullOrWhiteSpace(param.IdDay))
                predicate = predicate.And(x => x.IdDay == param.IdDay);
            
            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                var predicate2 = PredicateBuilder.Create<MsSession>(x
                    => EF.Functions.Like(x.GradePathway.Grade.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Day.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Name, param.SearchPattern())
                    || EF.Functions.Like(x.Alias, param.SearchPattern())
                    || EF.Functions.Like(Convert.ToString(x.DurationInMinutes), param.SearchPattern())
                    || x.GradePathway.GradePathwayDetails.Any(y => y.Pathway.Description.Contains(param.Search)));

                // don't parse to TimeSpan when Search can parse to int
                if (!int.TryParse(param.Search, out _) && TimeSpan.TryParse(param.Search, out var time))
                    predicate2 = predicate2.Or(x => x.StartTime == time || x.EndTime == time);

                predicate = predicate.And(predicate2);
            }

            var query = _dbContext.Entity<MsSession>().SearchByIds(param).Where(predicate);
            query = param.OrderBy switch
            {
                "schoolDay" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Day.Code.Length)
                    : query.OrderByDescending(x => x.Day.Code.Length),
                "pathway" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.GradePathway.GradePathwayDetails.Min(y => y.Pathway.Description))
                    : query.OrderByDescending(x => x.GradePathway.GradePathwayDetails.Min(y => y.Pathway.Description)),
                _ => query.OrderByDynamic(param, _aliasColumns.Value)
            };
            
            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.SessionID.ToString()))
                    .ToListAsync(CancellationToken);
            else
            {
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetPathwayResult
                    {
                        Id = x.Id,
                        Alias = x.Alias,
                        Name = x.Name,
                        SessionId = x.SessionID,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime,
                        DurationInMinutes = x.DurationInMinutes,
                        SchoolDay = x.Day.Description,
                        DayCode = x.Day.Code,
                        Pathway = string.Join(", ", x.GradePathway.GradePathwayDetails // force order asc one-to-many relation
                            .OrderBy(y => y.Pathway.Description).Select(p => p.Pathway.Description)),
                        Grade = x.GradePathway.Grade.Description
                    })
                    .ToListAsync(CancellationToken);
            }
            
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns.Value));
        }
    }
}
