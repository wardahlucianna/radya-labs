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
    public class GetDetailVenueTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetDetailVenueTypeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailVenueTypeRequest>(nameof(GetDetailVenueTypeRequest.IdVenueType));

            var data = await _dbContext.Entity<LtVenueType>()
                .Where(x => x.Id == param.IdVenueType)
                .Select(x => new GetDetailVenueTypeResult
                {
                    IdVenueType = x.Id,
                    VenueTypeName = x.VenueTypeName,
                    Description = x.Description
                }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(data as object);
        }
    }
}
