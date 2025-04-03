using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class GetLessonHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[] 
        { 
            nameof(GetLessonRequest.IdAcadyear) 
        });
        private static readonly Lazy<string[]> _columns = new Lazy<string[]>(new[] 
        { 
            "grade", "classId", "subject", "teachers.name", "totalPerWeek", "homeroom" 
        });
        private static readonly Lazy<IDictionary<string, string>> _aliasColumns = new Lazy<IDictionary<string, string>>(new Dictionary<string, string>
        {
            { _columns.Value[0], "grade.description" },
            { _columns.Value[1], "classIdGenerated" },
            { _columns.Value[2], "subject.description" }
        });
        
        private readonly ISchedulingDbContext _dbContext;

        public GetLessonHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLessonRequest>(_requiredParams.Value);
            
            var predicate = PredicateBuilder.Create<MsLesson>(x => x.IdAcademicYear == param.IdAcadyear);
            if (param.Semester > 0)
                predicate = predicate.And(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdSubject))
                predicate = predicate.And(x => x.IdSubject == param.IdSubject);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.LessonPathways.Any(y => y.HomeroomPathway.IdHomeroom == param.IdHomeroom));
            if (param.ExceptIds != null && param.ExceptIds.Any())
                predicate = predicate.And(x => !param.ExceptIds.Contains(x.Id));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Grade.Description, param.SearchPattern())
                    || EF.Functions.Like(x.ClassIdGenerated, param.SearchPattern())
                    || EF.Functions.Like(x.Subject.Description, param.SearchPattern())
                    || EF.Functions.Like(x.TotalPerWeek.ToString(), param.SearchPattern())
                    || x.LessonTeachers.Any(y => EF.Functions.Like(y.Staff.FirstName, param.SearchPattern()))
                    || x.LessonPathways.Any(y => EF.Functions.Like(y.HomeroomPathway.Homeroom.Grade.Code, param.SearchPattern()))
                    || x.LessonPathways.Any(y => EF.Functions.Like(y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code, param.SearchPattern())));

            var query = _dbContext.Entity<MsLesson>()
                .SearchByIds(param)
                .Where(predicate);
            
            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                query = param.OrderBy switch
                {
                    "teachers.name" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.LessonTeachers.First().Staff.FirstName)
                        : query.OrderByDescending(x => x.LessonTeachers.First().Staff.FirstName),
                    "homeroom" => param.OrderType == OrderType.Asc
                        ? query
                            .OrderBy(x => x.LessonPathways.First().HomeroomPathway.Homeroom.Grade.Code)
                            .ThenBy(x => x.LessonPathways.First().HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code)
                        : query
                            .OrderByDescending(x => x.LessonPathways.First().HomeroomPathway.Homeroom.Grade.Code)
                            .ThenByDescending(x => x.LessonPathways.First().HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code),
                    "grade.code" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Grade.Code.Length).ThenBy(x => x.Grade.Code)
                        : query.OrderByDescending(x => x.Grade.Code.Length).ThenByDescending(x => x.Grade.Code),
                    _ => query.OrderByDynamic(param, _aliasColumns.Value)
                };
            }

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.ClassIdGenerated))
                    .ToListAsync(CancellationToken);
            }
            else
            {
                items = await query
                    .Select(x => new GetLessonResult
                    {
                        Id = x.Id,
                        Grade = x.Grade.Description,
                        Semester = x.Semester,
                        ClassId = x.ClassIdGenerated,
                        Subject = x.Subject.Description,
                        Teachers = x.LessonTeachers.Select(y => new NameValueVm(y.IdUser, !string.IsNullOrEmpty(y.Staff.FirstName) ? y.Staff.FirstName: y.Staff.LastName)),
                        TotalPerWeek = x.TotalPerWeek,
                        Homeroom = string.Join(", ", x.LessonPathways
                            .OrderBy(y => y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code)
                            .Select(y => string.Format("{0} {1}{2}",
                                y.HomeroomPathway.Homeroom.Grade.Code,
                                y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code,
                                y.HomeroomPathway.GradePathwayDetail.Pathway.Code.Equals("No Pathway", StringComparison.OrdinalIgnoreCase)
                                    ? string.Empty
                                    : " " + y.HomeroomPathway.GradePathwayDetail.Pathway.Code)))
                    })
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
            }
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns.Value));
        }
    }
}
