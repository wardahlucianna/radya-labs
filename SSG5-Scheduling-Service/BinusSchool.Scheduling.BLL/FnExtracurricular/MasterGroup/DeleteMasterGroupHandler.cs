using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.MasterGroup.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterGroup
{
    public class DeleteMasterGroupHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DeleteMasterGroupHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteMasterGroupRequest, DeleteMasterGroupValidator>();

            var getExistsExtracurricularFromGroup = await _dbContext.Entity<MsExtracurricular>()
                                                    .Include(eg => eg.ExtracurricularGroup)
                                                    .Where(x => x.ExtracurricularGroup.Id == param.GroupId)
                                                    .Select(x => x.ExtracurricularGroup.Name)
                                                    .FirstOrDefaultAsync(CancellationToken);

            if (!string.IsNullOrEmpty(param.GroupId))
            {
                // Cannot delete if the group is already used
                if (getExistsExtracurricularFromGroup != null)
                {
                    throw new BadRequestException($"Cannot delete extracurricular group ({getExistsExtracurricularFromGroup.Trim()}) because it is already used by extracurricular");
                }
                else
                {
                    var getExtracurricularGroup = await _dbContext.Entity<MsExtracurricularGroup>()
                                                .Where(x => x.Id == param.GroupId)
                                                .FirstOrDefaultAsync(CancellationToken);

                    getExtracurricularGroup.IsActive = false;
                    _dbContext.Entity<MsExtracurricularGroup>().Update(getExtracurricularGroup);
                    await _dbContext.SaveChangesAsync(CancellationToken);
                    return Request.CreateApiResult2();
                }
            }

            throw new BadRequestException(null);
        }
    }
}
