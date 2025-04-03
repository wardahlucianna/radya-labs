using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation;
using BinusSchool.Scheduling.FnSchedule.PersonalInvitation.Validator;

namespace BinusSchool.Scheduling.FnSchedule.PersonalInvitation
{

    public class CancelPersonalInviationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public CancelPersonalInviationHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CancelPersonalInvitationRequest, CancelPersonalInvitationValidator>();

            var data = await _dbContext.Entity<TrPersonalInvitation>().FindAsync(body.Id);
            
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["IdPersonalInvitation"], "Id", body.Id));

            //if(data.Status == PersonalInvitationStatus.Approved)
            //    throw new BadRequestException($"Data has been {data.Status}");

            data.Status = PersonalInvitationStatus.Cancelled;
            data.DeclineReason = body.Reason;

            _dbContext.Entity<TrPersonalInvitation>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

    }
}
