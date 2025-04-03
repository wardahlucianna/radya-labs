using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.ImmersionPeriod;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnActivities.Immersion.ImmersionPeriod.Validator;

namespace BinusSchool.Scheduling.FnActivities.Immersion.ImmersionPeriod
{
    public class UpdateImmersionPeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public UpdateImmersionPeriodHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<UpdateImmersionPeriodRequest, UpdateImmersionPeriodValidator>();

            // create new immersion period
            if (string.IsNullOrEmpty(param.IdImmersionPeriod))
            {
                var newImmersionPeriod = new MsImmersionPeriod
                {
                    Id = Guid.NewGuid().ToString(),
                    IdAcademicYear = param.IdAcademicYear,
                    Semester = param.Semester,
                    Name = param.Name,
                    RegistrationStartDate = param.RegistrationStartDate <= param.RegistrationEndDate ? param.RegistrationStartDate : throw new BadRequestException("Registration start date must be less than or equal to registration end date"),
                    RegistrationEndDate = param.RegistrationEndDate >= param.RegistrationStartDate ? param.RegistrationEndDate : throw new BadRequestException("Registration end date must be greater than or equal to registration start date")
                };

                _dbContext.Entity<MsImmersionPeriod>().Add(newImmersionPeriod);
                await _dbContext.SaveChangesAsync(CancellationToken);
            }

            // update existing immersion period
            else
            {
                var updateImmersionPeriod = _dbContext.Entity<MsImmersionPeriod>()
                                                .Find(param.IdImmersionPeriod);

                updateImmersionPeriod.IdAcademicYear = param.IdAcademicYear;
                updateImmersionPeriod.Semester = param.Semester;
                updateImmersionPeriod.Name = param.Name;
                updateImmersionPeriod.RegistrationStartDate = param.RegistrationStartDate <= param.RegistrationEndDate ? param.RegistrationStartDate : throw new BadRequestException("Registration start date must be less than or equal to registration end date");
                updateImmersionPeriod.RegistrationEndDate = param.RegistrationEndDate >= param.RegistrationStartDate ? param.RegistrationEndDate : throw new BadRequestException("Registration end date must be greater than or equal to registration start date");

                _dbContext.Entity<MsImmersionPeriod>().Update(updateImmersionPeriod);
                await _dbContext.SaveChangesAsync(CancellationToken);
            }

            return Request.CreateApiResult2();
        }
    }
}
