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
    public class ClassDiaryTypeSettingHandler : FunctionsHttpCrudHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public ClassDiaryTypeSettingHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetClassDiaryTypeSettings = await _dbContext.Entity<MsClassDiaryTypeSetting>()
               .Where(x => ids.Contains(x.Id))
               .ToListAsync(CancellationToken);

            var GetExcludeClassIds = await _dbContext.Entity<MsClassDiaryLessonExclude>()
                .Where(x => ids.Contains(x.IdClassDiaryTypeSetting))
                .ToListAsync(CancellationToken);

            GetClassDiaryTypeSettings.ForEach(x => x.IsActive = false);

            GetExcludeClassIds.ForEach(x => x.IsActive = false);

            _dbContext.Entity<MsClassDiaryTypeSetting>().UpdateRange(GetClassDiaryTypeSettings);

            _dbContext.Entity<MsClassDiaryLessonExclude>().UpdateRange(GetExcludeClassIds);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var query = await _dbContext.Entity<MsClassDiaryTypeSetting>()
                .Include(p => p.ClassDiaryLessonExcludes)
                .Where(x => x.Id == id)
                .Select(x => new GetClassDiaryTypeSettingDetailResult
                {
                    Id = x.Id,
                    IdAcademicYear = new CodeWithIdVm
                    {
                        Id = x.Academicyear.Id,
                        Code = x.Academicyear.Code,
                        Description = x.Academicyear.Description
                    },
                    TypeName = x.TypeName,
                    OccurencePerDayLimit = x.OccurrencePerDay,
                    MinimumStartDay = x.MinimumStartDay,
                    AllowStudentEntryClassDiary = x.AllowStudentEntryClassDiary
                    //ExcludeClassIds = x.ClassDiaryLessonExcludes.Select(y => y.Id).ToList()
                }).FirstOrDefaultAsync(CancellationToken);

            var excludeClassId = await _dbContext.Entity<MsClassDiaryLessonExclude>()
                .Include(p => p.Lesson)
                .Include(p => p.Lesson.Grade)
                .Include(p => p.Lesson.Subject)
                .Include(p => p.Lesson.LessonPathways)
                .Where(x => x.IdClassDiaryTypeSetting == id)
                .Select(x => new ExcludeClassId
                {
                    Id = x.Id,
                    IdLesson = x.IdLesson,
                    ClassId = x.Lesson.ClassIdGenerated,
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
                                    : " " + y.HomeroomPathway.GradePathwayDetail.Pathway.Code))),

                }).ToListAsync();

            query.ExcludeClassIds = excludeClassId;

            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetClassDiaryTypeSettingRequest>();

            var columns = new[] { "academicYear", "typeName", "occurencePerDayLimit", "minimumStartDay" };

            var predicate = PredicateBuilder.Create<MsClassDiaryTypeSetting>(x => param.IdSchool.Contains(x.Academicyear.IdSchool));

            var query = _dbContext.Entity<MsClassDiaryTypeSetting>()
                .Include(x => x.Academicyear)
                .Include(x => x.ClassDiaries)
                .Where(predicate);

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.TypeName, param.SearchPattern()));
            }

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
            {
                query = query.Where(x => EF.Functions.Like(x.Academicyear.Id, param.IdAcademicYear));
            }

            //ordering
            switch (param.OrderBy)
            {
                case "academicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Academicyear.Description)
                        : query.OrderBy(x => x.Academicyear.Description);
                    break;
                case "typeName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.TypeName)
                        : query.OrderBy(x => x.TypeName);
                    break;
                case "occurencePerDayLimit":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.OccurrencePerDay)
                        : query.OrderBy(x => x.OccurrencePerDay);
                    break;
                case "minimumStartDay":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.MinimumStartDay)
                        : query.OrderBy(x => x.MinimumStartDay);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetClassDiaryTypeSettingResult
                    {
                        Id = x.Id,
                        IdAcademicYear = new CodeWithIdVm
                        {
                            Id = x.Academicyear.Id,
                            Code = x.Academicyear.Code,
                            Description = x.Academicyear.Description
                        },
                        TypeName = x.TypeName,
                        MinimumStartDay = x.MinimumStartDay.ToString(),
                        OccurencePerDayLimit = x.OccurrencePerDay.ToString(),
                        AllowStudentEntryClassDiary = x.AllowStudentEntryClassDiary,
                        CanModified = !x.ClassDiaries.Any()
                    })
                    .ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result
                    .Select(x => new GetClassDiaryTypeSettingResult
                    {
                        Id = x.Id,
                        IdAcademicYear = new CodeWithIdVm
                        {
                            Id = x.Academicyear.Id,
                            Code = x.Academicyear.Code,
                            Description = x.Academicyear.Description
                        },
                        TypeName = x.TypeName,
                        MinimumStartDay = x.MinimumStartDay.ToString(),
                        OccurencePerDayLimit = x.OccurrencePerDay.ToString(),
                        AllowStudentEntryClassDiary = x.AllowStudentEntryClassDiary,
                        CanModified = !x.ClassDiaries.Any()
                    }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.ToList().Count;

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<AddClassDiaryTypeSettingRequest, AddClassDiaryTypeSettingValidator>();

            var existsData = _dbContext.Entity<MsClassDiaryTypeSetting>()
                .Any(x => x.IdAcademicyear == body.IdAcademicYear && x.TypeName == body.TypeName);

            if (existsData)
            {
                throw new BadRequestException($"Type Name { body.TypeName} already exists.");
            }

            var idClassDiaryTypeSetting = Guid.NewGuid().ToString();

            var newClassDiaryTypeSetting = new MsClassDiaryTypeSetting
            {
                Id = idClassDiaryTypeSetting,
                IdAcademicyear = body.IdAcademicYear,
                TypeName = body.TypeName,
                OccurrencePerDay = body.OccurrencePerDay.Value,
                MinimumStartDay = body.MinimumStartDay.Value,
                AllowStudentEntryClassDiary = body.AllowStudentEntryClassDiary.Value
            };

            if (body.ExcludeClassIds != null)
            {
                foreach (var excludeClassid in body.ExcludeClassIds)
                {
                    var newClassDiaryTypeSettingExclude = new MsClassDiaryLessonExclude
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdClassDiaryTypeSetting = idClassDiaryTypeSetting,
                        IdLesson = excludeClassid
                    };

                    _dbContext.Entity<MsClassDiaryLessonExclude>().Add(newClassDiaryTypeSettingExclude);
                }
            }

            _dbContext.Entity<MsClassDiaryTypeSetting>().Add(newClassDiaryTypeSetting);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateClassDiaryTypeSettingRequest, UpdateClassDiaryTypeSettingValidator>();

            var existData = await _dbContext.Entity<MsClassDiaryTypeSetting>()
                .Where(x => x.Id == body.Id).FirstOrDefaultAsync(CancellationToken);

            if (existData is null)
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Class Diary Type Setting"], "Id", body.Id));
            }

            //var checkTypeName = await _dbContext.Entity<MsClassDiaryTypeSetting>()
            //                                    .Where(x => x.Id != body.Id && x.TypeName == body.TypeName && x.IdAcademicyear == body.IdAcademicYear)  
            //                                    .AnyAsync(CancellationToken);
            //if (checkTypeName)
            //    throw new BadRequestException($"Type name {body.TypeName} already exists");

            //update data in MsClassDiaryTypeSetting
            existData.TypeName = body.TypeName;

            if (existData.OccurrencePerDay != body.OccurrencePerDay.Value)
                existData.OccurrencePerDay = body.OccurrencePerDay.Value;

            if (existData.MinimumStartDay != body.MinimumStartDay.Value)
                existData.MinimumStartDay = body.MinimumStartDay.Value;

            if (existData.AllowStudentEntryClassDiary != body.AllowStudentEntryClassDiary)
                existData.AllowStudentEntryClassDiary = body.AllowStudentEntryClassDiary;

            //update data in MsClassDiaryExcludeLesson
            await UpdateExclude(body.Id, body.ExcludeClassIds);

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        private async Task UpdateExclude(string id, List<string> excludeClassId)
        {
            var tempClassId = _dbContext.Entity<MsClassDiaryLessonExclude>().Where(x => x.IdClassDiaryTypeSetting == id).ToList();

            //delete data
            var classIdDeleted = tempClassId.Where(x => excludeClassId.All(a => a != x.IdLesson)).ToList();
            if (classIdDeleted.Any())
            {
                classIdDeleted.ForEach(e => e.IsActive = false);
            }

            //insert new data
            var classIds = excludeClassId.Where(x => tempClassId.All(a => a.IdLesson != x)).ToList();
            if (classIds.Any())
            {
                List<MsClassDiaryLessonExclude> lessonExcludes = new List<MsClassDiaryLessonExclude>();
                foreach (var classId in classIds)
                {
                    lessonExcludes.Add(new MsClassDiaryLessonExclude
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdClassDiaryTypeSetting = id,
                        IdLesson = classId
                    });
                }

                await _dbContext.Entity<MsClassDiaryLessonExclude>().AddRangeAsync(lessonExcludes);
            }
        }
    }
}
