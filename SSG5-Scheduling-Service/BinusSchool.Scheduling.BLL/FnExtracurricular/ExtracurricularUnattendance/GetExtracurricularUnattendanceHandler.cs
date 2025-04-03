using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularUnattendance;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Domain.Extensions;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularUnattendance
{
    public class GetExtracurricularUnattendanceHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = new[]
        {
            nameof(GetExtracurricularUnattendanceRequest.IdAcademicYear),
            nameof(GetExtracurricularUnattendanceRequest.Semester)
        };
        private static readonly string[] _columns = new[] { "StartDate", "EndDate", "ExtracurricularName", "Description" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            //{ _columns[0], "ExtracurricularGroup.id" }
        };

        private readonly ISchedulingDbContext _dbContext;

        public GetExtracurricularUnattendanceHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetExtracurricularUnattendanceRequest>(_requiredParams);
            var predicate = PredicateBuilder.True<MsExtracurricularNoAttDateMapping>();

            if (param.IdAcademicYear != null)
                predicate = predicate.And(x => x.Extracurricular.ExtracurricularGradeMappings.Select(y => y.Grade.Level.AcademicYear.Id).Contains(param.IdAcademicYear));
            if (param.Semester != 0)
                predicate = predicate.And(x => x.Extracurricular.Semester == param.Semester);

            var query = _dbContext.Entity<MsExtracurricularNoAttDateMapping>()
                .Include(x => x.Extracurricular)
                .Include(x => x.ExtracurricularNoAttDate)
                
                .SearchByIds(param)
                .Where(predicate);

            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                query = param.OrderBy switch
                {
                    "startDate" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.ExtracurricularNoAttDate.StartDate)
                        : query.OrderByDescending(x => x.ExtracurricularNoAttDate.StartDate),
                    "endDate" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.ExtracurricularNoAttDate.EndDate)
                        : query.OrderByDescending(x => x.ExtracurricularNoAttDate.EndDate),
                    "description" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.ExtracurricularNoAttDate.Description)
                        : query.OrderByDescending(x => x.ExtracurricularNoAttDate.Description),
                    _ => query.OrderByDynamic(param, _aliasColumns)
                };
            }
            var count = await query.Select(x => x.ExtracurricularNoAttDate.Id).Distinct().CountAsync(CancellationToken);
            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.ExtracurricularNoAttDate.Id,
                        Description = x.ExtracurricularNoAttDate.Description
                    })
                    .Distinct()
                    .ToListAsync(CancellationToken);
                items = result;
            }
            else
            {
                var queryResults = await query
                    .ToListAsync(CancellationToken);

                var valresults = queryResults.GroupBy(x => x.IdExtracurricularNoAttDate).ToList();

                var results = valresults.Select(x => new GetExtracurricularUnattendanceResult
                    {
                        Id = x.Key,
                        Description = x.First().ExtracurricularNoAttDate.Description,
                        ExtracurricularName = x.Select(x => x.Extracurricular).ToList().Count != 0
                                ? string.Join("; ", x.Select(x => x.Extracurricular.Name))
                                : Localizer[""],
                        StartDate = x.First().ExtracurricularNoAttDate.StartDate,
                        EndDate = x.First().ExtracurricularNoAttDate.EndDate
                    })
                    .SetPagination(param)
                    .ToList();

                items = results;

                count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : valresults.Select(x => x.Key).Count();
            }

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
