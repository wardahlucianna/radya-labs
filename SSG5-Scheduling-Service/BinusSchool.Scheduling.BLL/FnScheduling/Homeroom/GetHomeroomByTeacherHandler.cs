using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom
{
    public class GetHomeroomByTeacherHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = new[]
        {
            nameof(GetHomeroomByTeacherRequest.IdAcademicyear),
            nameof(GetHomeroomByTeacherRequest.IdTeacher)
        };
        
        private readonly ISchedulingDbContext _dbContext;

        public GetHomeroomByTeacherHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetHomeroomByTeacherRequest>(_requiredParams);

            var idLessons = await _dbContext.Entity<MsLessonTeacher>()
                .Where(x => x.IdUser == param.IdTeacher && x.Lesson.IdAcademicYear == param.IdAcademicyear)
                .Distinct()
                .Select(x => x.IdLesson)
                .ToListAsync(CancellationToken);

            var predicate = PredicateBuilder.Create<MsLessonPathway>(x => idLessons.Contains(x.IdLesson));
            if (param.Semester.HasValue)
                predicate = predicate.And(x => x.HomeroomPathway.Homeroom.Semester == param.Semester.Value);
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.HomeroomPathway.Homeroom.Grade.Description.Contains(param.Search)
                || x.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Description.Contains(param.Search));
            var query = _dbContext.Entity<MsLessonPathway>()
               .Include(x => x.HomeroomPathway)
                    .ThenInclude(x => x.Homeroom)
                        .ThenInclude(x => x.Grade)
                            .ThenInclude(x => x.Level)
                .Include(x => x.HomeroomPathway)
                    .ThenInclude(x => x.Homeroom)
                        .ThenInclude(x => x.GradePathwayClassroom)
                            .ThenInclude(x => x.Classroom)
                .Where(predicate)
                .AsQueryable();
            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var results = await query
                    .GroupBy(x => new
                    {
                        Id = x.HomeroomPathway.Homeroom.Id,
                        Grade = x.HomeroomPathway.Homeroom.Grade.Code,
                        Classroom = x.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code,
                        OrderGrade = x.HomeroomPathway.Homeroom.Grade.OrderNumber,
                        OrderLevel = x.HomeroomPathway.Homeroom.Grade.Level.OrderNumber,
                    })
                    .OrderBy(x => x.Key.OrderLevel).ThenBy(x => x.Key.OrderGrade).ThenBy(x => x.Key.Classroom)
                    .Select(x => new GetLOVHomeroomResult
                    {
                        Id = x.Key.Id,
                        Code = x.Key.Grade,
                        Description = string.Format("{0}{1}", x.Key.Grade, x.Key.Classroom)
                    })
                    .ToListAsync(CancellationToken);

                items = results;
            }
            else
            {
                var results = await query
                .GroupBy(x => new
                {
                    Id = x.HomeroomPathway.Homeroom.Id,
                    Grade = x.HomeroomPathway.Homeroom.Grade.Code,
                    Classroom = x.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code,
                    OrderGrade = x.HomeroomPathway.Homeroom.Grade.OrderNumber,
                    OrderLevel = x.HomeroomPathway.Homeroom.Grade.Level.OrderNumber,
                }).Select(x => new GetHomeroomByTeacherResult
                {
                    Id = x.Key.Id,
                    Code = x.Key.Grade,
                    Description = string.Format("{0}{1}", x.Key.Grade, x.Key.Classroom)
                }).ToListAsync(CancellationToken);
                items = results;

            }
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty());
        }
    }
}
