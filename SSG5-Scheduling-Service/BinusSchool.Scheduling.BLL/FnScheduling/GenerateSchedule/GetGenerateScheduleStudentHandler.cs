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
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.GenerateSchedule
{
    public class GetGenerateScheduleStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetGenerateScheduleStudentHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGenerateScheduleStudentRequest>(nameof(GetGenerateScheduleStudentRequest.IdGrade));
            var columns = new[] { "name", "description" };

            var predicate = PredicateBuilder.Create<TrGeneratedScheduleStudent>(p => p.GeneratedScheduleLessons.Any(x => x.IsGenerated));
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.GeneratedScheduleGrade.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.GeneratedScheduleLessons.Any(y => y.IdHomeroom == param.IdHomeroom));
            if (param.Semester.HasValue)
                predicate = predicate.And(x => x.GeneratedScheduleLessons.Any(y => y.Homeroom.Semester == param.Semester.Value));
            // if (!string.IsNullOrWhiteSpace(param.Search))
            //     predicate = predicate.And(x
            //         => EF.Functions.Like(x.Student.FirstName, param.SearchPattern())
            //         || EF.Functions.Like(x.Student.MiddleName, param.SearchPattern())
            //         || EF.Functions.Like(x.Student.LastName, param.SearchPattern())
            //         || EF.Functions.Like(x.GeneratedScheduleLessons.First().HomeroomName, param.SearchPattern()));

            var query = _dbContext.Entity<TrGeneratedScheduleStudent>()
                .Include(x => x.GeneratedScheduleLessons)
                .Where(predicate)
                .SearchByIds(param)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items = default;
            var count = 0;
            if (param.Return == CollectionType.Lov)
            {
                var results = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = NameUtil.GenerateFullNameWithId(x.Student.Id, x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
                    })
                    .ToListAsync(CancellationToken);

                if (!string.IsNullOrWhiteSpace(param.Search))
                {
                    results = results
                        .Where(x => x.Description.Contains(param.Search, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                items = results;
                count = items.Count();
            }
            else
            {
                var results = await query
                    .GroupBy(x => new
                    {
                        x.IdStudent,
                        x.Student.FirstName,
                        x.Student.MiddleName,
                        x.Student.LastName
                    })
                    .If(string.IsNullOrWhiteSpace(param.Search), x => x.SetPagination(param))
                    .Select(x => new GetGeneratedScheduleStudentResult
                    {
                        Id = x.Key.IdStudent,
                        Student = new NameValueVm(x.Key.IdStudent, NameUtil.GenerateFullName(x.Key.FirstName, x.Key.MiddleName, x.Key.LastName)),
                        //Homeroom = new CodeWithIdVm
                        //{
                        //    Id = x.Key.IdHomeroom,
                        //    Code = x.Key.Code,
                        //    Description = x.Key.Description
                        //},
                        //Grade = new CodeWithIdVm
                        //{
                        //    Id = x.Key.IdGrade, 
                        //    Code = x.Key.Code,
                        //    Description = x.Key.Description
                        //},
                        //ClassIds = x.Key.GeneratedScheduleLessons.Distinct().Select(y => y.ClassID)
                    })
                    .ToListAsync(CancellationToken);

                var datas = new List<GetGeneratedScheduleStudentResult>();

                if (!string.IsNullOrWhiteSpace(param.Search))
                    datas = results
                        .Where(x => x.Student.Name.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                                    || x.Student.Id.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                                    || $"{x.Student.Id} {x.Student.Name}".Contains(param.Search, StringComparison.OrdinalIgnoreCase))
                        .SetPagination(param)
                        .ToList();
                else
                    datas = results;


                var studentId = datas.Select(x => x.Student.Id).ToList();
                var generatedScheduleLesson = await
                    (
                        from _generatedScheduleLesson in _dbContext.Entity<TrGeneratedScheduleLesson>()
                        join _generatedScheduleStudent in _dbContext.Entity<TrGeneratedScheduleStudent>() on _generatedScheduleLesson.IdGeneratedScheduleStudent equals _generatedScheduleStudent.Id
                        join _lesson in _dbContext.Entity<MsLesson>() on _generatedScheduleLesson.IdLesson equals _lesson.Id
                        join _lessonTeacher in _dbContext.Entity<MsLessonTeacher>() on new { p1 = _lesson.Id, p2 = true } equals new { p1 = _lessonTeacher.IdLesson, p2 = _lessonTeacher.IsPrimary }
                        join _staff in _dbContext.Entity<MsStaff>() on _lessonTeacher.IdUser equals _staff.IdBinusian
                        where
                            1 == 1
                            && studentId.Any(y => y == _generatedScheduleStudent.IdStudent)
                        select new
                        {
                            IdStudent = _generatedScheduleStudent.IdStudent,
                            ClassId = _generatedScheduleLesson.ClassID,
                            TeacherName = _lessonTeacher != null ? _lessonTeacher.Staff.FirstName + " " + _lessonTeacher.Staff.LastName : null,
                            IdHomeroom = _generatedScheduleLesson.IdHomeroom,
                            HomeroomName = _generatedScheduleLesson.HomeroomName
                        }
                   ).ToListAsync(CancellationToken);
                foreach (var item in datas)
                {
                    item.ClassIds = generatedScheduleLesson
                        .Where(x => x.IdStudent == item.Student.Id)
                        .GroupBy(x => new
                        {
                            x.ClassId,
                            x.TeacherName
                        })
                        .Select(x => new LessonGeneratedScheduleVm
                        {
                            ClassId = x.Key.ClassId,
                            TeacherName = x.Key.TeacherName != null ? string.Join(",", x.Key.TeacherName) : null
                        })
                        .ToList();
                    item.Homeroom = generatedScheduleLesson
                        .Where(x => x.IdStudent == item.Student.Id)
                        .GroupBy(x => new
                        {
                            x.IdHomeroom,
                            x.HomeroomName
                        })
                        .Select(x => new CodeWithIdVm
                        {
                            Id = x.Key.IdHomeroom,
                            Code = x.Key.HomeroomName,
                            Description = x.Key.HomeroomName
                        })
                        .FirstOrDefault();
                }

                items = datas;

                count = param.CanCountWithoutFetchDb(items.Count)
               ? items.Count
               : string.IsNullOrWhiteSpace(param.Search) ? await query.GroupBy(x => new
               {
                   x.IdStudent,
                   x.Student.FirstName,
                   x.Student.MiddleName,
                   x.Student.LastName
               }).Select(x => x.Key).CountAsync(CancellationToken) : results.Where(x => x.Student.Name.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                                  || x.Student.Id.Contains(param.Search, StringComparison.OrdinalIgnoreCase)
                                  || $"{x.Student.Id} {x.Student.Name}".Contains(param.Search, StringComparison.OrdinalIgnoreCase))
               .Count();
            }

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
