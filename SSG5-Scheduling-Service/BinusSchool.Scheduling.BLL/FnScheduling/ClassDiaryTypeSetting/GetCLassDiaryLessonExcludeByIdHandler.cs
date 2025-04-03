using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.ClassDiaryTypeSetting.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiaryTypeSetting
{
    public class GetCLassDiaryLessonExcludeByIdHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetCLassDiaryLessonExcludeByIdHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetClassDiaryLessonExcludeRequest>();

            var columns = new[] { "grade", "homeRoom", "subject", "classId", "semester" };

            var query = _dbContext.Entity<MsClassDiaryLessonExclude>()
                .Include(p => p.Lesson)
                .Include(p => p.Lesson.Grade)
                .Include(p => p.Lesson.Subject)
                .Include(p => p.Lesson.LessonPathways)
                .Where(x => x.IdClassDiaryTypeSetting == param.IdClassDiaryTypeSetting);
                
            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.Lesson.Subject.Description, param.SearchPattern())
                || EF.Functions.Like(x.Lesson.ClassIdGenerated, param.SearchPattern()));
            }

            if (!string.IsNullOrEmpty(param.IdGrader))
            {
                query = query.Where(x => EF.Functions.Like(x.Lesson.Grade.Id, param.IdGrader));
            }

            if (!string.IsNullOrEmpty(param.IdSubject))
            {
                query = query.Where(x => EF.Functions.Like(x.Lesson.Subject.Id, param.IdSubject));
            }

            if (!string.IsNullOrEmpty(param.Semester))
            {
                query = query.Where(x => EF.Functions.Like(x.Lesson.Semester.ToString(), param.Semester));
            }

            //ordering
            switch (param.OrderBy)
            {
                case "grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Lesson.Grade.Description)
                        : query.OrderBy(x => x.Lesson.Grade.Description);
                    break;
                case "homeRoom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Lesson.LessonPathways.FirstOrDefault().HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Description)
                        : query.OrderBy(x => x.Lesson.LessonPathways.FirstOrDefault().HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Description);
                    break;
                case "subject":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Lesson.Subject.Description)
                        : query.OrderBy(x => x.Lesson.Subject.Description);
                    break;
                case "classId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Lesson.ClassIdGenerated)
                        : query.OrderBy(x => x.Lesson.ClassIdGenerated);
                    break;
                case "semester":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Lesson.Semester)
                        : query.OrderBy(x => x.Lesson.Semester);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;

            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Id))
                    .ToListAsync(CancellationToken);
            }
            else
            {
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetClassDiaryLessonExcludeResult
                    {
                        Id = x.Id,
                        IdLesson = x.IdLesson,
                        ClassId = x.Lesson.ClassIdGenerated,
                        Semester = x.Lesson.Semester.ToString(),
                        Grade = new CodeWithIdVm
                        {
                            Id = x.Lesson.Grade.Id,
                            Code = x.Lesson.Grade.Code,
                            Description = x.Lesson.Grade.Description
                        },
                        Subject = new CodeWithIdVm
                        {
                            Id = x.Lesson.Subject.Id,
                            Code = x.Lesson.Subject.Code,
                            Description = x.Lesson.Subject.Description
                        },
                        HomeRoom = string.Join(", ", x.Lesson.LessonPathways
                            .OrderBy(y => y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code)
                            .Select(y => string.Format("{0}{1}{2}",
                                y.HomeroomPathway.Homeroom.Grade.Code,
                                y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code,
                                y.HomeroomPathway.GradePathwayDetail.Pathway.Code.Equals("No Pathway", StringComparison.OrdinalIgnoreCase)
                                    ? string.Empty
                                    : " " + y.HomeroomPathway.GradePathwayDetail.Pathway.Code)))
                    }).ToListAsync(CancellationToken);
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
