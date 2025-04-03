using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.ProjectInformation.SubmissionFlow;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.ProjectInformation.SubmissionFlow
{
    public class GetSPCListHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _context;

        public GetSPCListHandler(ISchoolDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetSPCListRequest>();

            var response = await GetSPCList(param);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<List<GetSPCListResponse>> GetSPCList(GetSPCListRequest param)
        {
            var response = new List<GetSPCListResponse>();

            var GetSPCList = await _context.Entity<MsSchoolProjectCoordinator>()
                .Include(x => x.Staff)
                .Include(x => x.School)
                .Where(x => x.IdSchool == (param.IdSchool ?? x.IdSchool) && x.IsActive == true)
                .OrderBy(x => x.School.Description)
                    .ThenBy(x => x.Staff.FirstName.Trim() + " " + x.Staff.LastName.Trim())
                .Select(x => new GetSPCListResponse
                {
                    IdSchoolProjectCoordinator = x.Id,
                    FullName = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName),
                    BinusianEmail = x.Staff.BinusianEmailAddress,
                    Remarks = x.Remarks,
                    School = new ItemValueVm
                    { 
                        Id = x.School.Id,
                        Description = x.School.Description
                    },
                    PhotoUrl = x.PhotoLink,
                    IdBinusian = x.IdBinusian
                })
                .ToListAsync(CancellationToken);

            response.AddRange(GetSPCList);

            return response;
        }
    }
}

