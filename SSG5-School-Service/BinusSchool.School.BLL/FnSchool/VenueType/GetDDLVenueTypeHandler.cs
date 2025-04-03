using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueType;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.VenueType
{
    public class GetDDLVenueTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetDDLVenueTypeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDDLVenueTypeRequest>(nameof(GetDDLVenueTypeRequest.IdSchool));

            var data = await _dbContext.Entity<LtVenueType>()
                .Where(x => x.IdSchool == param.IdSchool)
                .Select(x => new ItemValueVm
                {
                    Id = x.Id,
                    Description = x.Description
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
