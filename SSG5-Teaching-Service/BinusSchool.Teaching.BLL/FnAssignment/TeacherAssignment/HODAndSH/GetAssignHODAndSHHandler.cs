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
using BinusSchool.Data.Model.School.FnSubject.Department;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.HODAndSH;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.HODAndSH
{
    public class GetAssignHODAndSHHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetDepartmentRequest.IdSchool)
        });
        private static readonly Lazy<string[]> _columns = new Lazy<string[]>(new[]
        {
            "acadyear", "level", "description"
        });
        private static readonly Lazy<IDictionary<string, string>> _aliasColumns = new Lazy<IDictionary<string, string>>(new Dictionary<string, string>
        {
            { _columns.Value[0], "academicYear.code" }
        });
        private static readonly Lazy<string[]> _positionToFilters = new Lazy<string[]>(new[]
        {
            PositionConstant.HeadOfDepartment, PositionConstant.SubjectHead, PositionConstant.SubjectHeadAssitant
        });

        private IEnumerable<TrNonTeachingLoad> _trNonTchLoadsHoD, _trNonTchLoadsSH, _trNonTchLoadsSHA;
        private List<string> _deletedIds;

        private readonly ITeachingDbContext _dbContext;

        public GetAssignHODAndSHHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDepartmentRequest>(_requiredParams.Value);
            var predicate = PredicateBuilder.Create<MsDepartment>(x => param.IdSchool.Contains(x.AcademicYear.IdSchool));

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => param.IdLevel == "general"
                    ? x.Type == DepartmentType.General
                    : x.DepartmentLevels.Any(y => y.IdLevel == param.IdLevel));
            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcadyear);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.AcademicYear.Description, param.SearchPattern())
                    || x.DepartmentLevels.Any(y => EF.Functions.Like(y.Level.Code, param.SearchPattern()))
                    || EF.Functions.Like(x.Description, param.SearchPattern()));
            
            var query = _dbContext.Entity<MsDepartment>()
                .SearchByIds(param)
                .Where(predicate);
            
            query = param.OrderBy switch
            {
                "level" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.DepartmentLevels.Count != 0
                        ? x.DepartmentLevels.Min(y => y.Level.Code)
                        : Localizer["General"])
                    : query.OrderByDescending(x => x.DepartmentLevels.Count != 0
                        ? x.DepartmentLevels.Min(y => y.Level.Code)
                        : Localizer["General"]),
                _ => query.OrderByDynamic(param, _aliasColumns.Value)
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            }
            else
            {
                var results = await query
                    .Include(x => x.AcademicYear)
                    .Include(x => x.Subjects)
                    .Include(x => x.DepartmentLevels).ThenInclude(x => x.Level)
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
                
                if (results.Count != 0)
                {
                    var idAcadyears = string.IsNullOrEmpty(param.IdAcadyear)
                        ? results.Select(x => x.IdAcademicYear).Distinct()
                        : new[] { param.IdAcadyear };
                    var trNonTeachingLoads = await _dbContext.Entity<TrNonTeachingLoad>()
                        .Include(x => x.MsNonTeachingLoad).ThenInclude(x => x.TeacherPosition).ThenInclude(x => x.Position)
                        .Where(x 
                            => _positionToFilters.Value.Contains(x.MsNonTeachingLoad.TeacherPosition.Position.Code)
                            && idAcadyears.Contains(x.MsNonTeachingLoad.IdAcademicYear))
                        .ToListAsync(CancellationToken);

                    _trNonTchLoadsHoD = trNonTeachingLoads.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.HeadOfDepartment);
                    _trNonTchLoadsSH = trNonTeachingLoads.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.SubjectHead);
                    _trNonTchLoadsSHA = trNonTeachingLoads.Where(x => x.MsNonTeachingLoad.TeacherPosition.Position.Code == PositionConstant.SubjectHeadAssitant);
                }
                
                items = results
                    .Select(x => new GetAssignHODAndSHResult
                    {
                        Id = x.Id,
                        AcademicYear = new ItemValueVm
                        {
                            Id = x.IdAcademicYear,
                            Description = x.AcademicYear.Description
                        },
                        Department = new ItemValueVm
                        {
                            Id = x.Id,
                            Description = x.Description,

                        },
                        Description = x.Description,
                        Level = x.DepartmentLevels.Count != 0
                            ? string.Join(", ", x.DepartmentLevels.Select(y => y.Level.Code))
                            : Localizer["General"],
                        Status = GetStatus(x.IdAcademicYear, x.Id, x.Subjects),
                        DeletedIds = _deletedIds
                    })
                    .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns.Value));
        }

        private string GetStatus(string idAcadyear, string idDepartment, IEnumerable<MsSubject> subjects)
        {
            var (isHoDAssigned, isSHAssigned) = (false, false);
            _deletedIds = new List<string>();

            foreach (var item in _trNonTchLoadsHoD.Where(x => x.MsNonTeachingLoad.IdAcademicYear == idAcadyear))
            {
                var data = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);

                if (data.TryGetValue("Department", out var department) && department.Id == idDepartment)
                {
                    isHoDAssigned = true;
                    _deletedIds.Add(item.Id);
                    break;
                }
            }

            var statusOfSHs = new List<bool>();
            foreach (var item in subjects)
            {
                var teachingLoadSH = _trNonTchLoadsSH
                    .Where(x => x.MsNonTeachingLoad.IdAcademicYear == idAcadyear)
                    .Where(x =>
                    {
                        bool matchDepartment = true, matchSubject = true;
                        var department = x.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Department", false);
                        var subject = x.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Subject", false);
                        matchDepartment = department.Id == idDepartment;
                        matchSubject = subject.Id == item.Id;
                        return matchDepartment && matchSubject;
                    })
                    .FirstOrDefault();

                if (teachingLoadSH != null)
                {
                    statusOfSHs.Add(true);
                    _deletedIds.Add(teachingLoadSH.Id);

                    var teachingLoadSHA = _trNonTchLoadsSHA
                        .Where(x => x.MsNonTeachingLoad.IdAcademicYear == idAcadyear)
                        .Where(x => 
                        {
                            bool matchDepartment = true, matchSubject = true;
                            var department = x.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Department", false);
                            var subject = x.Data.DeserializeToDictionaryAndReturn<ItemValueVm>("Subject", false);

                            matchDepartment = department.Id == idDepartment;
                            matchSubject = subject.Id == item.Id;
                            return matchDepartment && matchSubject;
                        })
                        .FirstOrDefault();

                    if (teachingLoadSHA != null)
                        _deletedIds.Add(teachingLoadSHA.Id);
                }
                else
                {
                    statusOfSHs.Add(false);
                }
            }

            isSHAssigned = statusOfSHs.Count > 0 ? statusOfSHs.All(x => x) : false;
            var status = (isHoDAssigned, isSHAssigned) switch
			{
				(true, true) => "All Assigned",
				(false, true) => "SH Assigned",
				(true, false) => "HOD Assigned",
				_ => "HOD and SH Not Assigned"
			};

			return status;
        }
    }
}
