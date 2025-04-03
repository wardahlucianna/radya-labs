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
    public class GetSubjectGroupSettingsSubjectSelectionGroupHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;

        public GetSubjectGroupSettingsSubjectSelectionGroupHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetSubjectGroupSettingsSubjectSelectionGroupRequest>
                (nameof(GetSubjectGroupSettingsSubjectSelectionGroupRequest.IdSchool));

            var response = new List<GetSubjectGroupSettingsSubjectSelectionGroupResponse>();

            var subjectSelectionGroups = await _context.Entity<LtSubjectSelectionGroup>()
                .Where(a => a.IdSchool == request.IdSchool)
                .ToListAsync(CancellationToken);

            if (!subjectSelectionGroups.Any())
                return Request.CreateApiResult2(response as object);

            var data = subjectSelectionGroups
                .Select(a => new GetSubjectGroupSettingsSubjectSelectionGroupResponse
                {
                    IdSubjectSelectionGroup = a.Id,
                    Name = a.SubjectSelectionGroupName,
                    ActiveStatus = a.ActiveStatus,
                    CanDelete = !_context.Entity<MsMappingCurriculumSubjectGroup>().Where(b => b.Id == a.Id).Any(),
                })
                .ToList();

            if (!string.IsNullOrEmpty(request.Search))
                data = data
                    .Where(a => a.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            response.AddRange(data);

            response = request.OrderBy switch
            {
                "Name" => request.OrderType == Common.Model.Enums.OrderType.Asc
                    ? response.OrderBy(a => a.Name).ToList()
                    : response.OrderByDescending(a => a.Name).ToList(),
                _ => response.OrderBy(a => a.Name).ToList(),
            };

            var count = request.CanCountWithoutFetchDb(response.Count)
                ? response.Count
                : response.Select(a => a.IdSubjectSelectionGroup).Count();

            response = response.SetPagination(request).ToList();

            return Request.CreateApiResult2(response as object, request.CreatePaginationProperty(count));
        }
    }
}
