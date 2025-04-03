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
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnExtracurricular.MasterGroup.Validator;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterGroup
{
    public class CreateMasterGroupHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public CreateMasterGroupHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CreateMasterGroupRequest, CreateMasterGroupValidator>();

            // Check for the same group name in DB
            bool isExistGroupName = _dbContext.Entity<MsExtracurricularGroup>()
                                    .Where(x => x.IdSchool == param.IdSchool)
                                    .Any(x => x.Name == param.GroupName.Trim());

            if (!string.IsNullOrEmpty(param.GroupName))
            {
                if (!isExistGroupName)
                {
                    var newIdExtracurricularGroup = Guid.NewGuid().ToString();

                    var newExtracurricularGroup = _dbContext.Entity<MsExtracurricularGroup>().Add(new MsExtracurricularGroup
                    {
                        Id = newIdExtracurricularGroup,
                        IdSchool = param.IdSchool,
                        Name = param.GroupName.Trim(),
                        Description = param.GroupDescription,
                        Status = (bool)param.Status
                    });

                    await _dbContext.SaveChangesAsync(CancellationToken);
                    return Request.CreateApiResult2();
                }
                throw new BadRequestException($"Group name {param.GroupName.Trim()} is already exists.");
            }

            throw new BadRequestException(null);
        }
    }
}
