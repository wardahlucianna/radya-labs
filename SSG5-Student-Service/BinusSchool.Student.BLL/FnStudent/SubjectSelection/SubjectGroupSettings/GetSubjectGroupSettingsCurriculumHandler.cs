using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
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
    public class GetSubjectGroupSettingsCurriculumHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;

        public GetSubjectGroupSettingsCurriculumHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetSubjectGroupSettingsCurriculumRequest>(
                nameof(GetSubjectGroupSettingsCurriculumRequest.IdSchool),
                nameof(GetSubjectGroupSettingsCurriculumRequest.IdGrade));

            var response = await GetSubjectGroupSettingsCurriculums(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetSubjectGroupSettingsCurriculumResponse>> GetSubjectGroupSettingsCurriculums(GetSubjectGroupSettingsCurriculumRequest request)
        {
            var response = new List<GetSubjectGroupSettingsCurriculumResponse>();

            var curriculums = _context.Entity<MsMappingCurriculumGrade>()
                .Include(a => a.SubjectSelectionCurriculum)
                .Where(a => a.SubjectSelectionCurriculum.IdSchool == request.IdSchool
                    && a.IdGrade == request.IdGrade)
                .ToList();

            if (!curriculums.Any())
                return response;

            if (!string.IsNullOrEmpty(request.Search))
                curriculums = curriculums
                    .Where(a => a.SubjectSelectionCurriculum.CurriculumName.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            response = curriculums
                .Select(a => new GetSubjectGroupSettingsCurriculumResponse
                {
                    Id = a.Id,
                    Description = a.SubjectSelectionCurriculum.CurriculumName
                })
                .OrderBy(a => a.Description)
                .ToList();

            return response;
        }
    }
}
