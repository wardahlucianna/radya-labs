using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach
{
    public class DeleteElectiveExternalCoachHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DeleteElectiveExternalCoachHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteElectiveExternalCoachRequest, DeleteElectiveExternalCoachValidator>();

            var getExternalCoach = await _dbContext.Entity<MsExtracurricularExternalCoach>()
                                    .Include(x => x.ExtracurricularExternalCoachAtts)
                                    .Include(x => x.ExtracurricularExtCoachMappings)
                                    .Where(x => x.Id == param.IdExtracurricularExternalCoach)
                                    .FirstOrDefaultAsync(CancellationToken);

            if (getExternalCoach == null)
                throw new BadRequestException("Cannot find external coach data");

            if(getExternalCoach.ExtracurricularExtCoachMappings.Any() || getExternalCoach.ExtracurricularExternalCoachAtts.Any())
                throw new BadRequestException("Cannot delete external coach");

            _dbContext.Entity<MsExtracurricularExternalCoach>().Remove(getExternalCoach);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
