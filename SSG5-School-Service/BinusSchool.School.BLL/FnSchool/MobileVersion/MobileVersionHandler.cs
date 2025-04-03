using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MobileVersion;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MobileVersion
{
    public class MobileVersionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public MobileVersionHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<MobileVersionRequest>();

            var query = await _dbContext.Entity<MsMobileVersion>().Where(x => x.OperatingSystem.ToLower().Contains(param.OperatingSystem.ToLower())).OrderByDescending(x => x.DateIn).Select(x => new MobileVersionResult
            {
                IdMobileVersion = x.Id,
                MobileVersion = x.MobileVersion,
                OperatingSystem = x.OperatingSystem                
            }).FirstOrDefaultAsync(CancellationToken);

            if (query == null)
            {
                throw new NotFoundException($"Version of {param.OperatingSystem} Not Found !");
            }

            return Request.CreateApiResult2(query as object);
        }
    }
}
