using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class GetMasterExtracurricularTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetMasterExtracurricularTypeHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMasterExtracurricularTypeRequest>
                (nameof(GetMasterExtracurricularTypeRequest.IdSchool));

            var getExtracurricularType = await _dbContext.Entity<MsExtracurricularType>()
                .Where(a => param.IdSchool.Any(b => b == a.IdSchool)
                    && (string.IsNullOrWhiteSpace(param.Search) ? true : EF.Functions.Like(a.Description, param.SearchPattern())))
                .OrderByDescending(a => a.IsDefault)
                    .ThenBy(a => a.Description)
                .ToListAsync(CancellationToken);

            var items = getExtracurricularType.Select(a => new GetMasterExtracurricularTypeResult
            {
                Id = a.Id,
                Description = a.Description,
                IsDefault = a.IsDefault
            })
                .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
