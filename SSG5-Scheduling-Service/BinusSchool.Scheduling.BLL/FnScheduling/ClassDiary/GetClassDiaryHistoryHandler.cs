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
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator;
using Microsoft.EntityFrameworkCore;


namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class GetClassDiaryHistoryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetClassDiaryHistoryHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetClassDiaryHistoryRequest>();
            string[] _columns = { "AcademicYear", "ClassDiaryGrade", "Subject", "Semester", "Homeroom", "ClassId", "ClassDiaryDate", "ClassDiaryTypeSetting", "ClassDiaryTopic", "Status", "RequesrDate" };


            IReadOnlyList<IItemValueVm> items = default;
            var query = (from ClassDiary in _dbContext.Entity<HTrClassDiary>()
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on ClassDiary.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                         join Grade in _dbContext.Entity<MsGrade>() on Homeroom.IdGrade equals Grade.Id
                         join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Homeroom.IdAcademicYear equals AcademicYear.Id
                         join ClassDiaryTypeSetting in _dbContext.Entity<MsClassDiaryTypeSetting>() on ClassDiary.IdClassDiaryTypeSetting equals ClassDiaryTypeSetting.Id
                         join Lesson in _dbContext.Entity<MsLesson>() on ClassDiary.IdLesson equals Lesson.Id
                         join Subject in _dbContext.Entity<MsSubject>() on Lesson.IdSubject equals Subject.Id
                         where AcademicYear.Id == param.AcademicYearId && ClassDiary.UserIn == param.UserId
                         select new
                         {
                             ClassDiaryId = ClassDiary.IdHTrClassDiary,
                             AcademicYear = AcademicYear.Description,
                             ClassDiaryTypeSetting = ClassDiaryTypeSetting.TypeName,
                             ClassDiaryTypeSettingId = ClassDiaryTypeSetting.Id,
                             SubjectId = Subject.Id,
                             Subject = Subject.Description,
                             ClassId = Lesson.ClassIdGenerated,
                             LessonId = Lesson.Id,
                             ClassDiaryDate = ClassDiary.ClassDiaryDate,
                             ClassDiaryTopic = ClassDiary.ClassDiaryTopic,
                             Status = ClassDiary.Status,
                             GradeId = Homeroom.IdGrade,
                             ClassDiaryGrade = Grade.Description,
                             Semester = Homeroom.Semester,
                             HomeroomId = Homeroom.Id,
                             Homeroom = Grade.Code + Classroom.Code,
                             RequestDate = ClassDiary.DateIn,
                             RequestBy = ClassDiary.UserIn,
                         });

            //filter
            if (!string.IsNullOrEmpty(param.GradeId))
                query = query.Where(x => x.GradeId == param.GradeId);
            if (!string.IsNullOrEmpty(param.SubjectId))
                query = query.Where(x => x.SubjectId == param.SubjectId);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                query = query.Where(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.HomeroomId))
                query = query.Where(x => x.HomeroomId == param.HomeroomId);
            if (!string.IsNullOrEmpty(param.LessonId))
                query = query.Where(x => x.LessonId == param.LessonId);
            if (!string.IsNullOrEmpty(param.ClassDiaryDate.ToString()))
                query = query.Where(x => x.ClassDiaryDate >= Convert.ToDateTime(param.ClassDiaryDate).Date && x.ClassDiaryDate <= Convert.ToDateTime(param.ClassDiaryDate).Date.AddDays(1).AddMilliseconds(-1));
            if (!string.IsNullOrEmpty(param.ClassDiaryTypeSettingId))
                query = query.Where(x => x.ClassDiaryTypeSettingId == param.ClassDiaryTypeSettingId);
            if (!string.IsNullOrEmpty(param.ClassDiaryStatus))
                query = query.Where(x => x.Status == param.ClassDiaryStatus);
            if (!string.IsNullOrEmpty(param.RequestDate.ToString()))
                query = query.Where(x => x.RequestDate >= Convert.ToDateTime(param.RequestDate).Date && x.RequestDate <= Convert.ToDateTime(param.RequestDate).Date.AddDays(1).AddMilliseconds(-1));
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.ClassDiaryTopic.Contains(param.Search));


            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "ClassDiaryGrade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ClassDiaryGrade)
                        : query.OrderBy(x => x.ClassDiaryGrade);
                    break;
                case "Subject":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Subject)
                        : query.OrderBy(x => x.Subject);
                    break;
                case "Semester":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Semester)
                        : query.OrderBy(x => x.Semester);
                    break;
                case "Homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom)
                        : query.OrderBy(x => x.Homeroom);
                    break;
                case "ClassId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ClassId)
                        : query.OrderBy(x => x.ClassId);
                    break;
                case "ClassDiaryDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ClassDiaryDate)
                        : query.OrderBy(x => x.ClassDiaryDate);
                    break;
                case "ClassDiaryTypeSetting":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ClassDiaryTypeSetting)
                        : query.OrderBy(x => x.ClassDiaryTypeSetting);
                    break;
                case "ClassDiaryTopic":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ClassDiaryTopic)
                        : query.OrderBy(x => x.ClassDiaryTopic);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
                case "RequestDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.RequestDate)
                        : query.OrderBy(x => x.RequestDate);
                    break;
            };

            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetClassDiaryHistoryResult
                {
                    Id = x.ClassDiaryId,
                    AcademicYear = x.AcademicYear,
                    Grade = x.ClassDiaryGrade,
                    Subject = x.Subject,
                    Semester = x.Semester.ToString(),
                    Homeroom = x.Homeroom,
                    ClassId = x.ClassId,
                    ClassDiaryDate = x.ClassDiaryDate,
                    ClassDiaryTypeSetting = x.ClassDiaryTypeSetting,
                    ClassDiaryTopic = x.ClassDiaryTopic,
                    RequestDate = x.RequestDate,
                    Status = x.Status,
                }).ToList();

            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetClassDiaryHistoryResult
                {
                    Id = x.ClassDiaryId,
                    AcademicYear = x.AcademicYear,
                    Grade = x.ClassDiaryGrade,
                    Subject = x.Subject,
                    Semester = x.Semester.ToString(),
                    Homeroom = x.Homeroom,
                    ClassId = x.ClassId,
                    ClassDiaryDate = x.ClassDiaryDate,
                    ClassDiaryTypeSetting = x.ClassDiaryTypeSetting,
                    ClassDiaryTopic = x.ClassDiaryTopic,
                    RequestDate = x.RequestDate,
                    Status = x.Status,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.ClassDiaryId).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));

        }
    }
}
