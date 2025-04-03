using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Data.Model.School.FnSchool.Level;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetTeacherLevelsHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetTeacherLevelsRequest.IdSchool), nameof(GetTeacherLevelsRequest.IdUser) };
        private static readonly string[] _columns = { "code", "description" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[0], "msLevel.code" },
            { _columns[1], "msLevel.description" }
        };
        
        private readonly ISchedulingDbContext _dbContext;
        
        public GetTeacherLevelsHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherLevelsRequest>(_requiredParams);
            var gradeIds = await _dbContext.Entity<MsLessonTeacher>()
                                           .Include(x => x.Lesson)
                                           .Where(x => x.IdUser == param.IdUser)
                                           .Select(x => x.Lesson.IdGrade)
                                           .Distinct()
                                           .ToListAsync(CancellationToken);

            var predicate = PredicateBuilder.Create<MsGrade>(x
                => gradeIds.Contains(x.Id)
                && param.IdSchool.Contains(x.Level.AcademicYear.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.IdAcadYear))
                predicate = predicate.And(x => x.Level.IdAcademicYear == param.IdAcadYear);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Level.Code, param.SearchPattern())
                    && EF.Functions.Like(x.Level.Description, param.SearchPattern()));

            var levels = await _dbContext.Entity<MsGrade>()
                .Where(predicate)
                .OrderByDynamic(param, _aliasColumns)
                // .SetPagination(param)
                .Select(x => new GetLevelResult
                {
                    Id = x.IdLevel,
                    Code = x.Level.Code,
                    Description = x.Level.Description,
                    Acadyear = x.Level.AcademicYear.Description
                })
                .Distinct()
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(levels as object, param.CreatePaginationProperty(levels.Count).AddColumnProperty(_columns));
        }
    }
}
