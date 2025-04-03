using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.BreakSetting;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.BreakSetting.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.BreakSetting
{
    public class UpdateAvailabilityHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public UpdateAvailabilityHandler(ISchedulingDbContext BreakSettingDbContext)
        {
            _dbContext = BreakSettingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateAvailabilityRequest, UpdateAvailabilityValidator>();

            var DataSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                                .Where(e => e.Id == body.IdInvitationBookingSettingSchedule)
                                .FirstOrDefaultAsync(CancellationToken);

            if (DataSchedule == null)
                throw new BadRequestException($"Schedule with Id : {string.Join(",", body.IdInvitationBookingSettingSchedule)} not exists");

            if (!body.IsAvailability)
            {
                DataSchedule.IsPriority = false;
                DataSchedule.IsAvailable = false;
                DataSchedule.IsFlexibleBreak = false;
                DataSchedule.IsDisabledPriority = true;
                DataSchedule.IsDisabledFlexible = true;
                DataSchedule.IsDisabledAvailable = false;
                DataSchedule.IdInvitationBookingSettingBreak = null;
                DataSchedule.BreakName = null;
            }
            else
            {
                var CountInvitationBooking = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                                                         .Where(e => e.IdUserTeacher == DataSchedule.IdUserTeacher
                                                                && e.DateInvitation.Date == DataSchedule.DateInvitation.Date
                                                                && (e.IsPriority == true || e.IsFlexibleBreak == true)
                                                                && ((e.StartTime>= DataSchedule.StartTime && e.StartTime<= DataSchedule.EndTime) || (e.EndTime >= DataSchedule.StartTime && e.EndTime <= DataSchedule.EndTime))
                                                                && e.Id != DataSchedule.Id)
                                                         .CountAsync(CancellationToken);

                if (CountInvitationBooking > 0)
                {
                    DataSchedule.IsPriority = false;
                    DataSchedule.IsAvailable = true;
                    DataSchedule.IsFlexibleBreak = false;
                    DataSchedule.IsDisabledPriority = true;
                    DataSchedule.IsDisabledFlexible = false;
                    DataSchedule.IsDisabledAvailable = false;
                    DataSchedule.IdInvitationBookingSettingBreak = null;
                    DataSchedule.BreakName = null;
                }
                else
                {
                    DataSchedule.IsPriority = false;
                    DataSchedule.IsAvailable = true;
                    DataSchedule.IsFlexibleBreak = false;
                    DataSchedule.IsDisabledPriority = false;
                    DataSchedule.IsDisabledFlexible = false;
                    DataSchedule.IsDisabledAvailable = false;
                    DataSchedule.IdInvitationBookingSettingBreak = null;
                    DataSchedule.BreakName = null;
                }
            }

            _dbContext.Entity<TrInvitationBookingSettingSchedule>().Update(DataSchedule);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
