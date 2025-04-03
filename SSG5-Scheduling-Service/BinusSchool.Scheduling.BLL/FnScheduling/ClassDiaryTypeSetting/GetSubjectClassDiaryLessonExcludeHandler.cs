using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class GetSubjectClassDiaryLessonExcludeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetSubjectClassDiaryLessonExcludeHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSubjectClassDiaryTypeSettingRequest>();

            var query = await _dbContext.Entity<MsClassDiaryLessonExclude>()
                .Include(p => p.Lesson).ThenInclude(p => p.Grade).ThenInclude(p => p.Subjects)
                .Where(x => x.IdClassDiaryTypeSetting == param.IdClassDiaryTypeSetting && x.Lesson.Grade.Id == param.IdGrade)
                .ToListAsync(CancellationToken);

            var Grades = query
                .GroupBy(x => x.Lesson.Subject)
                .Select(x => new GetGradeClassDiaryTypeSettingResult
                {
                    Id = x.Key.Id,
                    Code = x.Key.Code,
                    Description = x.Key.Description
                }).ToList();

            return Request.CreateApiResult2(Grades as object);
        }
    }
}
