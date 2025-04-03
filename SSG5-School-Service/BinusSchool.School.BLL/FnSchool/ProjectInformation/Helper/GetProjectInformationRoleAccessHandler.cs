using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.Helper;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.ProjectInformation.Helper
{
    public class GetProjectInformationRoleAccessHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;

        public GetProjectInformationRoleAccessHandler(ISchoolDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var response = await GetProjectInformationRoleAccess();

            return Request.CreateApiResult2(response as object);
        }

        public async Task<GetProjectInformationRoleAccessResponse> GetProjectInformationRoleAccess()
        {
            var response = new GetProjectInformationRoleAccessResponse();

            var userRole = AuthInfo.Roles.Select(a => a.Id).ToList();

            var getProjectInformationRoleAccess = await _context.Entity<MsProjectInformationRoleAccess>()
                .Where(a => userRole.Contains(a.IdRole))
                .ToListAsync(CancellationToken);

            response.AllowCreateUpdateDelete = getProjectInformationRoleAccess.Any();

            return response;
        }
    }
}
