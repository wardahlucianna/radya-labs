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
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{

    public class DeleteBreakSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DeleteBreakSettingHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteBreakSettingRequest, DeleteBreakSettingValidator>();

            var data = await _dbContext.Entity<TrInvitationBookingSettingBreak>().FindAsync(body.IdInvitationBookingSettingBreak);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["InvitationBookingSetting"], "Id", body.IdInvitationBookingSettingBreak));

            data.IsActive = false;

            _dbContext.Entity<TrInvitationBookingSettingBreak>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

    }
}
