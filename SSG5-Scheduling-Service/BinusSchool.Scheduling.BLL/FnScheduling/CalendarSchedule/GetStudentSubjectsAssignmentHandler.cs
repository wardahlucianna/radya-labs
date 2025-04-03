using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetStudentSubjectsAssignmentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetStudentSubjectsAssignmentHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserSubjectsRequest>(nameof(GetUserSubjectsRequest.IdUser));
            var columns = new[] { "description" };

            if (param.Position == "ST")
            {
                var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.IdUser == param.IdUser);

                if (!string.IsNullOrWhiteSpace(param.IdHomeroom))
                    predicate = predicate.And(x => x.IdHomeroom == param.IdHomeroom);
                
                if(!string.IsNullOrWhiteSpace(param.IdGrade))
                    predicate = predicate.And(x => x.Homeroom.IdGrade == param.IdGrade);

                if (!string.IsNullOrWhiteSpace(param.Search))
                    predicate = predicate.And(x => EF.Functions.Like(x.SubjectName, $"%{param.Search}%"));

                if (param.Ids != null && param.Ids.Any())
                    predicate = predicate.And(x => param.Ids.Contains(x.IdSubject));

                var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
                                      .Include(x => x.Homeroom)
                                      .Include(x => x.GeneratedScheduleStudent)
                                      .Where(predicate)
                                      .Select(x => new { x.IdSubject, x.SubjectName })
                                      .Distinct();

                if (!string.IsNullOrEmpty(param.OrderBy))
                {
                    if (param.OrderType == OrderType.Asc)
                        query = query.OrderBy(x => x.SubjectName);
                    else
                        query = query.OrderByDescending(x => x.SubjectName);
                }
                else
                    query = query.OrderBy(x => x.SubjectName);

                IReadOnlyList<IItemValueVm> items;
                if (param.Return == CollectionType.Lov)
                    items = await query
                        .Select(x => new ItemValueVm(x.IdSubject, x.SubjectName))
                        .ToListAsync(CancellationToken);
                else
                    items = await query
                        .SetPagination(param)
                        .Select(x => new ItemValueVm(x.IdSubject, x.SubjectName))
                        .ToListAsync(CancellationToken);
                var count = param.CanCountWithoutFetchDb(items.Count)
                    ? items.Count
                    : await query.CountAsync(CancellationToken);

                return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
            }
            else
            {
                var query = (
                    from ms in _dbContext.Entity<MsSubject>() 
                    join mg in _dbContext.Entity<MsGrade>() on ms.IdGrade equals mg.Id
                    join ml2 in _dbContext.Entity<MsLevel>() on mg.IdLevel equals ml2.Id
                    join may in _dbContext.Entity<MsAcademicYear>() on ml2.IdAcademicYear equals may.Id
                    where
                    may.Id == param.IdAcadyear
                    && mg.Id == param.IdGrade
                    group new { ms, mg, ml2, may } by new
                    {
                        IdSubject = ms.Id,
                        IdAcademicYear = may.Id,
                        AcademicYear = may.Code,
                        IdLevel = ml2.Id,
                        Level = ml2.Description,
                        IdGrade = mg.Id,
                        Grade = mg.Description,
                        SubjectName = ms.Code,
                    } into gb

                    select new
                    {
                        gb.Key.IdAcademicYear,
                        gb.Key.AcademicYear,
                        gb.Key.IdLevel,
                        gb.Key.Level,
                        gb.Key.IdGrade,
                        gb.Key.Grade,
                        gb.Key.SubjectName,
                        gb.Key.IdSubject,
                    }
                )
                .AsQueryable();


                // Check teacher position
                var userVP = _dbContext.Entity<TrNonTeachingLoad>()
                .Include(x => x)
                    .Include(x => x.MsNonTeachingLoad)
                        .ThenInclude(x => x.TeacherPosition)
                            .ThenInclude(x => x.Position)
                    .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdAcadyear && x.IdUser == param.IdUser && (x.MsNonTeachingLoad.TeacherPosition.Position.Code == "P" || x.MsNonTeachingLoad.TeacherPosition.Position.Code == "VP"))
                    .ToList();
                if (userVP.Count() == 0)
                {
                    var userLH = _dbContext.Entity<TrNonTeachingLoad>()
                          .Include(x => x.MsNonTeachingLoad)
                              .ThenInclude(x => x.TeacherPosition)
                                  .ThenInclude(x => x.Position)
                          .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdAcadyear && x.IdUser == param.IdUser && x.MsNonTeachingLoad.TeacherPosition.Position.Code == "LH")
                          .AsQueryable();

                    var UserLHList = userLH.ToList();

                    if (UserLHList.Count() == 0)
                    {
                        var userSH = _dbContext.Entity<TrNonTeachingLoad>()
                        .Include(x => x.MsNonTeachingLoad)
                            .ThenInclude(x => x.TeacherPosition)
                                .ThenInclude(x => x.Position)
                        .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdAcadyear && x.IdUser == param.IdUser && x.MsNonTeachingLoad.TeacherPosition.Position.Code == "SH")
                        .AsQueryable();

                        var UserSHList = userSH.ToList();
                        if (UserSHList.Count() != 0)
                        {
                            List<string> SubjectId = new List<string>();
                            List<string> LevelId = new List<string>();

                            foreach (var datasubject in userSH)
                            {
                                var subject = datasubject.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Subject", false);
                                var level = datasubject.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Level", false);
                                SubjectId.Add(subject.Id);
                                LevelId.Add(level.Id);
                            }
                            query = query.Where(x => (SubjectId.Any(y => x.IdSubject == y) && LevelId.Any(y => x.IdLevel == y)));
                        }
                    }
                    else
                    {
                        List<string> GradeId = new List<string>();

                        foreach (var datasubject in UserLHList)
                        {
                            var Grade = datasubject.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Grade", false);
                            GradeId.Add(Grade.Id);
                        }
                        query = query.Where(x => GradeId.Any(y => x.IdGrade == y));
                    }
                }
                else
                {
                    List<string> LevelId = new List<string>();

                    foreach (var datasubject in userVP)
                    {
                        var level = datasubject.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Level", false);
                        LevelId.Add(level.Id);
                    }
                    query = query.Where(x => LevelId.Any(y => x.IdLevel == y));
                }

                if (!string.IsNullOrEmpty(param.Search))
                    query = query.Where(x => EF.Functions.Like(x.SubjectName, $"%{param.Search}%"));


                IReadOnlyList<IItemValueVm> items;
                if (param.Return == CollectionType.Lov)
                    items = await query
                        .Select(x => new ItemValueVm(x.IdSubject, x.SubjectName))
                        .ToListAsync(CancellationToken);
                else
                    items = await query
                        .SetPagination(param)
                        .Select(x => new ItemValueVm(x.IdSubject, x.SubjectName))
                        .ToListAsync(CancellationToken);
                var count = param.CanCountWithoutFetchDb(items.Count)
                    ? items.Count
                    : await query.CountAsync(CancellationToken);

                return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
            }
        }
    }
}
