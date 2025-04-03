using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.SubjectGroupSettings;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.SubjectSelection.SubjectGroupSettings
{
    public class GetSubjectGroupSettingsCurriculumMappingHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;

        public GetSubjectGroupSettingsCurriculumMappingHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetSubjectGroupSettingsCurriculumMappingRequest>(
                nameof(GetSubjectGroupSettingsCurriculumMappingRequest.IdSchool));

            var response = new List<GetSubjectGroupSettingsCurriculumMappingResponse>();

            var subjectSelectionGroups = await _context.Entity<MsMappingCurriculumSubjectGroup>()
                .Include(a => a.SubjectSelectionGroup)
                .Include(a => a.MappingCurriculumGrade.Grade.MsLevel.MsAcademicYear)
                .Include(a => a.MappingCurriculumGrade.SubjectSelectionCurriculum)
                .Where(a => a.SubjectSelectionGroup.IdSchool == request.IdSchool
                    && a.MappingCurriculumGrade.SubjectSelectionCurriculum.IdSchool == request.IdSchool
                    && (string.IsNullOrEmpty(request.IdAcademicYear) ? true : a.MappingCurriculumGrade.Grade.MsLevel.IdAcademicYear == request.IdAcademicYear)
                    && (string.IsNullOrEmpty(request.IdGrade) ? true : a.MappingCurriculumGrade.IdGrade == request.IdGrade)
                    && (string.IsNullOrEmpty(request.IdCurriculum) ? true : a.MappingCurriculumGrade.IdSubjectSelectionCurriculum == request.IdCurriculum))
                .ToListAsync(CancellationToken);

            if (!subjectSelectionGroups.Any())
                return Request.CreateApiResult2(response as object);

            var data = subjectSelectionGroups
                .GroupBy(g => new
                {
                    IdAcademicYear = g.MappingCurriculumGrade.Grade.MsLevel.IdAcademicYear,
                    AcademicYearDesc = g.MappingCurriculumGrade.Grade.MsLevel.MsAcademicYear.Description,
                    IdGrade = g.MappingCurriculumGrade.IdGrade,
                    GradeDesc = g.MappingCurriculumGrade.Grade.Description,
                    IdCurriculum = g.MappingCurriculumGrade.IdSubjectSelectionCurriculum,
                    CurriculumDesc = g.MappingCurriculumGrade.SubjectSelectionCurriculum.CurriculumName,
                    ActiveStatus = g.ActiveStatus
                })
                .Select(a => new GetSubjectGroupSettingsCurriculumMappingResponse
                {
                    AcademicYear = new ItemValueVm
                    {
                        Id = a.Key.IdAcademicYear,
                        Description = a.Key.AcademicYearDesc,
                    },
                    Grade = new ItemValueVm
                    {
                        Id = a.Key.IdGrade,
                        Description = a.Key.GradeDesc,
                    },
                    Curriculum = new ItemValueVm
                    {
                        Id = a.Key.IdCurriculum,
                        Description = a.Key.CurriculumDesc,
                    },
                    MappingSubjectSelectionGroups = a.Select(b => new GetSubjectGroupSettingsCurriculumMappingResponse_MappingSubjectSelectionGroup
                    {
                        IdMappingSubjectSelectionGroup = b.Id,
                        Id = b.IdSubjectSelectionGroup,
                        Description = b.SubjectSelectionGroup.SubjectSelectionGroupName
                    }).ToList(),
                    ActiveStatus = a.Key.ActiveStatus
                })
                .ToList();

            if (!string.IsNullOrEmpty(request.Search))
                data = data
                    .Where(a => a.AcademicYear.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.Grade.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.Curriculum.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                        || a.MappingSubjectSelectionGroups.Any(sg => sg.Description.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

            response.AddRange(data);

            response = request.OrderBy switch
            {
                "AcademicYear" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.AcademicYear.Description).ToList()
                    : response.OrderByDescending(a => a.AcademicYear.Description).ToList(),
                "Grade" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Grade.Description).ToList()
                    : response.OrderByDescending(a => a.Grade.Description).ToList(),
                "Curriculum" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.Curriculum.Description).ToList()
                    : response.OrderByDescending(a => a.Curriculum.Description).ToList(),
                "Status" => request.OrderType == OrderType.Asc
                    ? response.OrderBy(a => a.ActiveStatus).ToList()
                    : response.OrderByDescending(a => a.ActiveStatus).ToList(),
                _ => response.OrderByDescending(a => a.AcademicYear.Description).ToList(),
            };

            var count = request.CanCountWithoutFetchDb(response.Count)
                ? response.Count : response.Select(a => a.AcademicYear.Description).Count();

            response = response
                .SetPagination(request)
                .ToList();

            return Request.CreateApiResult2(response as object, request.CreatePaginationProperty(count));
        }
    }
}
