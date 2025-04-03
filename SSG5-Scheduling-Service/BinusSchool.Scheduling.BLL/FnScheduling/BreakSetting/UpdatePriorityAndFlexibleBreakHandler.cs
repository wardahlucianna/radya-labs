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
    public class UpdatePriorityAndFlexibleBreakHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public UpdatePriorityAndFlexibleBreakHandler(ISchedulingDbContext BreakSettingDbContext)
        {
            _dbContext = BreakSettingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdatePriorityAndFlexibleBreakRequest, UpdatePriorityAndFlexibleBreakValidator>();

            var DataSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                                .Where(e => e.Id == body.IdInvitationBookingSettingSchedule)
                                .FirstOrDefaultAsync(CancellationToken);

            var DataScheduleBeririsan = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                                .Where(e => e.IdUserTeacher == DataSchedule.IdUserTeacher
                                             && e.IdInvitationBookingSetting != DataSchedule.IdInvitationBookingSetting
                                             && e.DateInvitation.Date == DataSchedule.DateInvitation.Date
                                             && (
                                                    (e.StartTime >= DataSchedule.StartTime && e.StartTime <= DataSchedule.EndTime)
                                                    ||
                                                    (e.EndTime >= DataSchedule.StartTime && e.EndTime <= DataSchedule.EndTime)
                                                )
                                        )
                                .ToListAsync(CancellationToken);

            var CountInvitationBooking = await _dbContext.Entity<TrInvitationBookingDetail>()
                                                         .Include(e => e.InvitationBooking)
                                                         .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                                         .Where(e => e.InvitationBooking.IdUserTeacher == DataSchedule.IdUserTeacher && e.InvitationBooking.StartDateInvitation == DataSchedule.DateInvitation.Date + DataSchedule.StartTime)
                                                         .CountAsync(CancellationToken);


            List<TrInvitationBookingSettingSchedule> UpdateBeririsan = new List<TrInvitationBookingSettingSchedule>();

            if (DataSchedule == null)
                throw new BadRequestException($"Schedule with Id : {string.Join(",", body.IdInvitationBookingSettingSchedule)} not exists");

            if (body.IsPrority && body.IsChecked)
            {
                DataSchedule.IsPriority = true;
                DataSchedule.IsFlexibleBreak = false;
                DataSchedule.IsDisabledPriority = false;
                DataSchedule.IsDisabledFlexible = true;
                DataSchedule.IsDisabledAvailable = true;

                foreach (var item in DataScheduleBeririsan)
                {
                    item.IsPriority = false;
                    item.IsFlexibleBreak = false;
                    item.IsDisabledPriority = true;
                    item.IsDisabledFlexible = true;
                    item.IsDisabledAvailable = true;

                    UpdateBeririsan.Add(item);
                }

                
            }
            else if (!body.IsPrority && body.IsChecked)
            {
                DataSchedule.IsPriority = false;
                DataSchedule.IsFlexibleBreak = true;
                DataSchedule.IsDisabledPriority = true;
                DataSchedule.IsDisabledFlexible = true;
                DataSchedule.IsDisabledAvailable = true;

                if(CountInvitationBooking>0)
                    throw new BadRequestException($"Schedule can't setting {body.BreakName} because parent already booking");


                foreach (var item in DataScheduleBeririsan)
                {
                    item.IsPriority = false;
                    item.IsFlexibleBreak = false;
                    item.IsDisabledPriority = true;
                    item.IsDisabledFlexible = true;
                    item.IsDisabledAvailable = true;

                    UpdateBeririsan.Add(item);
                }
            }
            else
            {
                
                DataSchedule.IsPriority = false;
                DataSchedule.IsFlexibleBreak = null;
                DataSchedule.IsDisabledPriority = false;
                DataSchedule.IsDisabledFlexible = false;
                DataSchedule.IsDisabledAvailable = CountInvitationBooking > 0 ? true:false ;

                foreach (var item in DataScheduleBeririsan)
                {
                    item.IsPriority = null;
                    item.IsFlexibleBreak = null;
                    item.IsDisabledPriority = false;
                    item.IsDisabledFlexible = false;
                    item.IsDisabledAvailable = false;

                    UpdateBeririsan.Add(item);
                }
            }

            _dbContext.Entity<TrInvitationBookingSettingSchedule>().Update(DataSchedule);
            _dbContext.Entity<TrInvitationBookingSettingSchedule>().UpdateRange(UpdateBeririsan);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
