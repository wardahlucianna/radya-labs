using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.MapAttendance
{
    public class GetMapAttendanceLevelHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "code", "description" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[0], "msLevel.code" },
            { _columns[1], "msLevel.description" }
        };
        
        private readonly IAttendanceDbContext _dbContext;

        public GetMapAttendanceLevelHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMapAttendanceLevelRequest>(nameof(GetMapAttendanceLevelRequest.IdAcadyear));

            var query = _dbContext.Entity<MsMappingAttendance>()
                .SearchByIds(param)
                .Where(x => x.Level.IdAcademicYear == param.IdAcadyear)
                .SearchByDynamic(param)
                .OrderByDynamic(param, _aliasColumns);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.IdLevel, x.Level.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetMapAttendanceLevelResult
                    {
                        Id = x.IdLevel,
                        Code = x.Level.Code,
                        Description = x.Level.Description,
                        Term = x.AbsentTerms,
                        Acadyear = new CodeWithIdVm(x.Level.IdAcademicYear, x.Level.AcademicYear.Code, x.Level.AcademicYear.Description)
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);
            
            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
