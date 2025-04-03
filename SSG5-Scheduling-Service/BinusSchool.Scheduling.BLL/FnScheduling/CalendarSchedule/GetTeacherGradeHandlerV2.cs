using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetTeacherGradesHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetTeacherGradesRequest.IdSchool), nameof(GetTeacherGradesRequest.IdUser) };
        private static readonly string[] _columns = { "code", "description" };

        private readonly ISchedulingDbContext _dbContext;
        
        public GetTeacherGradesHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherGradesRequest>(_requiredParams);

            var gradeIds = await _dbContext.Entity<MsLessonTeacher>()
                                           .Include(x => x.Lesson)
                                           .Where(x => x.IdUser == param.IdUser)
                                           .Select(x => x.Lesson.IdGrade)
                                           .Distinct()
                                           .ToListAsync(CancellationToken);

            var predicate = PredicateBuilder.Create<MsGrade>(x=> true);
            if (gradeIds.Count != 0)
            {
                predicate = PredicateBuilder.Create<MsGrade>(x
                => gradeIds.Contains(x.Id)
                && param.IdSchool.Contains(x.Level.AcademicYear.IdSchool));
            }
            else
            {
                predicate = PredicateBuilder.Create<MsGrade>(x=> param.IdSchool.Contains(x.Level.AcademicYear.IdSchool));
            }

            if (!string.IsNullOrWhiteSpace(param.IdLevel))
                predicate = predicate.And(x => x.IdLevel == param.IdLevel);

            if (!string.IsNullOrWhiteSpace(param.IdAcadYear))
                predicate = predicate.And(x => x.Level.IdAcademicYear == param.IdAcadYear);


            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, param.SearchPattern()) 
                    || EF.Functions.Like(x.Description, param.SearchPattern()));

            var grades = await _dbContext.Entity<MsGrade>()
                .Where(predicate)
                .OrderByDynamic(param)
                // .SetPagination(param)
                .Select(x => new GetGradeResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description,
                    Level = new CodeWithIdVm(x.IdLevel, x.Level.Code, x.Level.Description),
                    Acadyear = new CodeWithIdVm
                    {
                        Id = x.Level.IdAcademicYear,
                        Code = x.Level.AcademicYear.Code,
                        Description = x.Level.AcademicYear.Description
                    },
                    School = new CodeWithIdVm
                    {
                        Id = x.Level.AcademicYear.IdSchool,
                        Code = x.Level.AcademicYear.School.Name,
                        Description = x.Level.AcademicYear.School.Description
                    }
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(grades as object, param.CreatePaginationProperty(grades.Count).AddColumnProperty(_columns));
        }
    }
}
