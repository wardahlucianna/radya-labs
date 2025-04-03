using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchedulingDb.Entities.School;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetTeacherAcademicYearsHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetTeacherAcademicYearsRequest.IdSchool), nameof(GetTeacherAcademicYearsRequest.IdUser) };
        private static readonly string[] _columns = { "code", "description" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[0], "msLevel.msAcademicYear.code" },
            { _columns[1], "msLevel.msAcademicYear.description" }
        };

        private readonly ISchedulingDbContext _dbContext;
        
        public GetTeacherAcademicYearsHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherAcademicYearsRequest>(_requiredParams);
            var gradeIds = await _dbContext.Entity<MsLessonTeacher>()
                                           .Include(x => x.Lesson)
                                           .Where(x => x.IdUser == param.IdUser)
                                           .Select(x => x.Lesson.IdGrade)
                                           .Distinct()
                                           .ToListAsync(CancellationToken);

            var predicate = PredicateBuilder.Create<MsGrade>(x 
                => gradeIds.Contains(x.Id) 
                && param.IdSchool.Contains(x.Level.AcademicYear.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Level.AcademicYear.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Level.AcademicYear.Description, param.SearchPattern()));

            var acadyears = await _dbContext.Entity<MsGrade>()
                .Where(predicate)
                .OrderByDynamic(param, _aliasColumns)
                // .SetPagination(param)
                .Select(x => new CodeWithIdVm
                {
                    Id = x.Level.IdAcademicYear,
                    Code = x.Level.AcademicYear.Code,
                    Description = x.Level.AcademicYear.Description
                })
                .Distinct()
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(acadyears as object, param.CreatePaginationProperty(acadyears.Count).AddColumnProperty(_columns));
        }
    }
}
