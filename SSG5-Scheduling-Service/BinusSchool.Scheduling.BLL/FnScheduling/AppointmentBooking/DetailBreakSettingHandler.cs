using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchedulingDb.Entities.School;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class DetailBreakSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DetailBreakSettingHandler(ISchedulingDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DetailBreakSettingRequest>(nameof(DetailBreakSettingRequest.IdInvitationBookingSettingBreak));
            var invitationBookingSetting = await DetailBreakSetting(param);

            return Request.CreateApiResult2(invitationBookingSetting as object);
        }

        public async Task<DetailBreakSettingResult> DetailBreakSetting(DetailBreakSettingRequest param)
        {
            var predicate = PredicateBuilder.Create<TrInvitationBookingSettingBreak>(x => true);

            predicate = predicate.And(x => x.Id == param.IdInvitationBookingSettingBreak);

            var data = await _dbContext.Entity<TrInvitationBookingSettingBreak>()
                .Include(e => e.Grade)
                .Where(predicate)
                .FirstOrDefaultAsync(CancellationToken);

            var dataGrade = _dbContext.Entity<MsGrade>();

            if(data == null)
                throw new BadRequestException($"Invitation booking setting not found");
            
            var invitationBookingSettingBreaks = new DetailBreakSettingResult
                {
                    IdInvitationBookingSettingBreak = data.Id,
                    AppointmentDate = data.DateInvitation,
                    BreakName = data.BreakName,
                    StartTime = data.StartTime,
                    EndTime = data.EndTime,
                    BreakType = data.BreakType,
                    Grade = data.IdGrade == null ? null : new ItemValueVm(data.Grade.Id, data.Grade.Description)
                };

            return invitationBookingSettingBreaks;
        }
    }
}
