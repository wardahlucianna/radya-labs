﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSubject.Subject
{
    public class GetSubjectHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "code", "description", "grade", "acadyear", "pathway", "department", "curriculumType", "subjectType", "subjectId", "subjectLevel", "maxSession" };
        private static readonly IDictionary<string, string> _aliasColumns = new Dictionary<string, string>
        {
            { _columns[3], "grade.level.academicYear.code" },
            { _columns[5], "department.description" },
            { _columns[6], "curriculum.description" },
            { _columns[7], "subjectType.description" },
        };

        private readonly ISchoolDbContext _dbContext;

        public GetSubjectHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSubjectRequest>();
            var predicate = PredicateBuilder.True<MsSubject>();

            if (param.IdSchool?.Any() ?? false)
                predicate = predicate.And(x => param.IdSchool.Contains(x.Grade.Level.AcademicYear.IdSchool));
            if (param.IdAcadyear?.Any() ?? false)
                predicate = predicate.And(x => param.IdAcadyear.Contains(x.Grade.Level.IdAcademicYear));
            if (param.IdLevel?.Any() ?? false)
                predicate = predicate.And(x => param.IdLevel.Contains(x.Grade.IdLevel));
            if (param.IdGrade?.Any() ?? false)
                predicate = predicate.And(x => param.IdGrade.Contains(x.IdGrade));
            if (!string.IsNullOrEmpty(param.IdPathway))
                predicate = predicate.And(x => x.SubjectPathways.Any(y => y.GradePathwayDetail.IdPathway == param.IdPathway));
            if (!string.IsNullOrEmpty(param.IdCurriculumType))
                predicate = predicate.And(x => x.IdCurriculum == param.IdCurriculumType);
            if (!string.IsNullOrEmpty(param.IdSubjectGroup))
                predicate = predicate.And(x => x.IdSubjectGroup == param.IdSubjectGroup);
            if (param.IdDepartment?.Any() ?? false)
                predicate = predicate.And(x => param.IdDepartment.Contains(x.IdDepartment));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Grade.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Grade.Level.AcademicYear.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Department.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Curriculum.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Curriculum.Code, param.SearchPattern())
                    || EF.Functions.Like(x.SubjectType.Description, param.SearchPattern())
                    || EF.Functions.Like(x.SubjectID, param.SearchPattern())
                    || EF.Functions.Like(Convert.ToString(x.MaxSession), param.SearchPattern())
                    || x.SubjectPathways.Any(y => EF.Functions.Like(y.GradePathwayDetail.Pathway.Code, param.SearchPattern())
                    || x.SubjectMappingSubjectLevels.Any(z => EF.Functions.Like(z.SubjectLevel.Code, param.SearchPattern()))));

            var query = _dbContext.Entity<MsSubject>().SearchByIds(param).Where(predicate);
            query = param.OrderBy switch
            {
                // sort like number
                "grade" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Grade.Description.Length).ThenBy(x => x.Grade.Description)
                    : query.OrderByDescending(x => x.Grade.Description.Length).ThenByDescending(x => x.Grade.Description),
                // sort one-to-many relation
                "pathway" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.SubjectPathways.Min(y => y.GradePathwayDetail.Pathway.Code))
                    : query.OrderByDescending(x => x.SubjectPathways.Min(y => y.GradePathwayDetail.Pathway.Code)),
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
                    .Include(x => x.SubjectPathways).ThenInclude(x => x.GradePathwayDetail).ThenInclude(x => x.Pathway)
                    .Include(x => x.SubjectMappingSubjectLevels).ThenInclude(x => x.SubjectLevel)
                    .Include(x => x.Department)
                    .Include(x => x.Curriculum)
                    .Include(x => x.SubjectType)
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
                        Level = x.Grade.Level.Description,
                        Acadyear = x.Grade.Level.AcademicYear.Code,
                        Pathway = string.Join(", ", x.SubjectPathways // force order asc one-to-many relation
                            .OrderBy(y => y.GradePathwayDetail.Pathway.Code).Select(y => y.GradePathwayDetail.Pathway.Code)),
                        IdDepartment = x.IdDepartment,
                        Department = x.Department.Description,
                        CurriculumType = x.Curriculum.Description,
                        SubjectType = x.SubjectType.Description,
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

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
