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
    public class UpdateMasterGroupHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public UpdateMasterGroupHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateMasterGroupRequest, UpdateMasterGroupValidator>();

            if (!string.IsNullOrEmpty(param.Group.Id))
            {
                var getExtracurricularGroup = await _dbContext.Entity<MsExtracurricularGroup>()
                                                .Where(x => x.Id == param.Group.Id)
                                                .FirstOrDefaultAsync(CancellationToken);

                if(getExtracurricularGroup != null)
                {
                    // Hanya update status
                    if (string.IsNullOrEmpty(param.Group.Name))
                    {
                        getExtracurricularGroup.Status = (bool)param.Status;
                    }

                    // update seluruh
                    else
                    {
                        getExtracurricularGroup.Name = param.Group.Name.Trim();
                        getExtracurricularGroup.Description = param.Description?.Trim();

                        // Check for the same group name in DB
                        bool isExistGroupName = _dbContext.Entity<MsExtracurricularGroup>()
                                                .Where(x => x.Id != getExtracurricularGroup.Id)
                                                .Where(x => x.IdSchool == param.IdSchool)
                                                .Any(x => x.Name == param.Group.Name.Trim());

                        if (isExistGroupName)
                        {
                            throw new BadRequestException($"Group name {param.Group.Name.Trim()} is already exists.");
                        }
                    }

                    _dbContext.Entity<MsExtracurricularGroup>().Update(getExtracurricularGroup);
                    await _dbContext.SaveChangesAsync(CancellationToken);
                    return Request.CreateApiResult2();
                }
            }
            throw new BadRequestException(null);
        }
    }
}
