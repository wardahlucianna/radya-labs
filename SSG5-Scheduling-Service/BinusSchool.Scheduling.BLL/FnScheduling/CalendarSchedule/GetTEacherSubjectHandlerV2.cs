using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetTeacherSubjectsHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "code", "description", "grade", "acadyear", "pathway", "department", "curriculumType", "subjectType", "subjectId", "subjectLevel", "maxSession" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[3], "grade.level.academicYear.code" },
            { _columns[5], "department.description" },
            { _columns[6], "curriculum.description" },
            { _columns[7], "subjectType.description" },
        };

        private readonly ISchedulingDbContext _dbContext;
        public GetTeacherSubjectsHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserSubjectsRequest>(nameof(GetUserSubjectsRequest.IdUser));
            var columns = new[] { "description" };

            if (param.Position != PositionConstant.VicePrincipal && param.Position != PositionConstant.SubjectHead && param.Position != PositionConstant.SubjectHeadAssitant)
            {
                var predicate = PredicateBuilder.True<MsLessonTeacher>();

                if (!string.IsNullOrEmpty(param.IdUser))
                    predicate = predicate.And(x => x.IdUser == param.IdUser);

                if (!string.IsNullOrEmpty(param.IdGrade))
                    predicate = predicate.And(x => x.Lesson.Subject.IdGrade == param.IdGrade);

                if (!string.IsNullOrEmpty(param.IdAcadyear))
                    predicate = predicate.And(x => x.Lesson.IdAcademicYear == param.IdAcadyear);

                if (!string.IsNullOrWhiteSpace(param.IdHomeroom))
                    predicate = predicate.And(x => x.Lesson.Subject.Grade.Homerooms.Any(y => y.Id == param.IdHomeroom));

                if (!string.IsNullOrWhiteSpace(param.Search))
                    predicate = predicate.And(x => EF.Functions.Like(x.Lesson.Subject.Description, $"%{param.Search}%"));

                if (param.Ids != null && param.Ids.Any())
                    predicate = predicate.And(x => param.Ids.Contains(x.Lesson.IdSubject));

                var query = _dbContext.Entity<MsLessonTeacher>()
                .Include(x => x.Lesson).ThenInclude(x => x.Subject)
                .ThenInclude(x => x.Grade)
                          .Where(predicate)
                          .Select(x => new { x.Lesson.Subject.Id, x.Lesson.Subject.Description })
                          .Distinct();

                if (!string.IsNullOrEmpty(param.OrderBy))
                {
                    if (param.OrderType == OrderType.Asc)
                        query = query.OrderBy(x => x.Description);
                    else
                        query = query.OrderByDescending(x => x.Description);
                }
                else
                    query = query.OrderBy(x => x.Description);

                IReadOnlyList<IItemValueVm> items;
                if (param.Return == CollectionType.Lov)
                    items = await query
                        .Select(x => new ItemValueVm(x.Id, x.Description))
                        .ToListAsync(CancellationToken);
                else
                    items = await query
                        .SetPagination(param)
                        .Select(x => new ItemValueVm(x.Id, x.Description))
                        .ToListAsync(CancellationToken);
                var count = param.CanCountWithoutFetchDb(items.Count)
                    ? items.Count
                    : await query.CountAsync(CancellationToken);

                return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
            }
            else
            {
                var predicate = PredicateBuilder.True<MsSubject>();

                if (!string.IsNullOrEmpty(param.IdAcadyear))
                    predicate = predicate.And(x => param.IdAcadyear.Contains(x.Grade.Level.IdAcademicYear));
                if (!string.IsNullOrEmpty(param.IdGrade))
                    predicate = predicate.And(x => param.IdGrade.Contains(x.IdGrade));
                if (!string.IsNullOrWhiteSpace(param.Search))
                    predicate = predicate.And(x => EF.Functions.Like(x.Description, $"%{param.Search}%"));

                if (param.Position == PositionConstant.SubjectHead || param.Position == PositionConstant.SubjectHeadAssitant)
                {
                    var listIdSubject = new List<string>();
                    var listTeacherAssignment = await _dbContext.Entity<TrNonTeachingLoad>()
                    .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                     .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdAcadyear
                                && x.IdUser == param.IdUser
                                && x.MsNonTeachingLoad.TeacherPosition.Position.Code == param.Position).ToListAsync(CancellationToken);
                    foreach (var item in listTeacherAssignment)
                    {
                        var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);
                        _dataNewPosition.TryGetValue("Grade", out var _GradePosition);

                        if (_GradePosition != null)
                        {
                            if (_GradePosition.Id == param.IdGrade)
                            {
                                if (_SubjectPosition != null)
                                {
                                    listIdSubject.Add(_SubjectPosition.Id);
                                }
                            }
                        }
                    }
                    if (listIdSubject.Any())
                        predicate = predicate.And(x => listIdSubject.Contains(x.Id));
                }

                var query = _dbContext.Entity<MsSubject>().Where(predicate);

                query = param.OrderBy switch
                {
                    // sort like number
                    "grade" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Grade.Description.Length).ThenBy(x => x.Grade.Description)
                        : query.OrderByDescending(x => x.Grade.Description.Length).ThenByDescending(x => x.Grade.Description),
                    // sort one-to-many relation
                    "subjectLevel" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.SubjectMappingSubjectLevels.Count != 0
                            ? x.SubjectMappingSubjectLevels.Min(y => y.SubjectLevel.Code)
                            : Localizer["General"])
                        : query.OrderByDescending(x => x.SubjectMappingSubjectLevels.Min(y => y.SubjectLevel.Code)),
                    // sort like number
                    "subjectId" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Grade.Code.Length).ThenBy(x => x.Grade.Code).ThenBy(x => x.SubjectID)
                        : query.OrderByDescending(x => x.Grade.Code.Length).ThenByDescending(x => x.Grade.Code).ThenByDescending(x => x.SubjectID),
                    // default
                    _ => query.OrderByDynamic(param, _aliasColumns)
                };

                IReadOnlyList<IItemValueVm> items = default;
                if (param.Return == CollectionType.Lov)
                {
                    items = await query
                        .Select(x => new CodeWithIdVm(x.Id, x.Code, $"{x.Description} ({x.SubjectID})"))
                        .ToListAsync(CancellationToken);
                }
                else
                {
                    var results = await query
                        .Include(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                        .Include(x => x.SubjectMappingSubjectLevels).ThenInclude(x => x.SubjectLevel)
                        .Include(x => x.Department)
                        .Include(x => x.Curriculum)
                        // .Include(x => x.SubjectGroup) // can be null, can't use inner join
                        .SetPagination(param)
                        .ToListAsync(CancellationToken);

                    // collect subject group here instead in query above
                    var idSubjectGroups = results.Where(x => x.IdSubjectGroup != null).Select(x => x.IdSubjectGroup).Distinct().ToArray();
                    var existSubjectGroups = new List<MsSubjectGroup>();
                    if (idSubjectGroups.Length != 0)
                    {
                        existSubjectGroups = await _dbContext
                            .Entity<MsSubjectGroup>().Where(x => idSubjectGroups.Contains(x.Id))
                            .ToListAsync(CancellationToken);
                    }

                    items = results
                        .Select(x => new GetSubjectResult
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description,
                            Grade = x.Grade.Description,
                            Acadyear = x.Grade.Level.AcademicYear.Code,
                            //Pathway = string.Join(", ", x.SubjectPathways // force order asc one-to-many relation
                            //    .OrderBy(y => y.GradePathwayDetail.Pathway.Code).Select(y => y.GradePathwayDetail.Pathway.Code)),
                            IdDepartment = x.IdDepartment,
                            Department = x.Department.Description,
                            CurriculumType = x.Curriculum.Description,
                            //SubjectType = x.SubjectType.Description,
                            SubjectId = x.SubjectID,
                            SubjectLevel = x.SubjectMappingSubjectLevels.Count != 0 // force order asc one-to-many relation
                                ? string.Join(", ", x.SubjectMappingSubjectLevels
                                    .OrderBy(y => y.SubjectLevel.Code).Select(y => y.SubjectLevel.Code))
                                : Localizer["General"],
                            SubjectLevels = x.SubjectMappingSubjectLevels.Count != 0 // force order asc one-to-many relation
                                ? x.SubjectMappingSubjectLevels
                                    .OrderBy(y => y.SubjectLevel.Code)
                                    .Select(y => new SubjectLevelResult
                                    {
                                        Id = y.SubjectLevel.Id,
                                        Code = y.SubjectLevel.Code,
                                        Description = y.SubjectLevel.Description,
                                        IsDefault = y.IsDefault
                                    })
                                : null,
                            SubjectGroup = x.IdSubjectGroup != null && existSubjectGroups.Any(y => y.Id == x.IdSubjectGroup)
                                ? new CodeWithIdVm
                                {
                                    Id = x.IdSubjectGroup,
                                    Code = existSubjectGroups.Find(y => y.Id == x.IdSubjectGroup)?.Code,
                                    Description = existSubjectGroups.Find(y => y.Id == x.IdSubjectGroup)?.Description
                                }
                                : null,
                            MaxSession = x.MaxSession
                        })
                        .ToList();
                }
                var count = param.CanCountWithoutFetchDb(items.Count)
                    ? items.Count
                    : await query.Select(x => x.Id).CountAsync(CancellationToken);

                return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
            }
        }
    }
}
