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

    public class UpdateBreakSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public UpdateBreakSettingHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateBreakSettingRequest, UpdateBreakSettingValidator>();

            var data = await _dbContext.Entity<TrInvitationBookingSettingBreak>().FindAsync(body.IdInvitationBookingSettingBreak);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["InvitationBookingSetting"], "Id", body.IdInvitationBookingSettingBreak));

            var predicate = PredicateBuilder.Create<TrInvitationBookingSettingBreak>(x => x.IsActive==true);

            if(!String.IsNullOrEmpty(body.IdGrade))
                predicate = predicate.And(x => x.IdGrade == body.IdGrade);

            var isNameExist = await _dbContext.Entity<TrInvitationBookingSettingBreak>()
                .Where(x => x.Id != body.IdInvitationBookingSettingBreak && x.IdInvitationBookingSetting == data.IdInvitationBookingSetting && x.DateInvitation == body.AppointmentDate && x.BreakName.ToLower() == body.BreakName.ToLower())
                .Where(predicate)
                .FirstOrDefaultAsync(CancellationToken);
            if (isNameExist != null)
                throw new BadRequestException($"{body.BreakName} already exists in this invitation date");

            if(body.StartTime == body.EndTime)
                throw new BadRequestException($"Start Time : {body.StartTime} same with End Time : {body.EndTime}");

            if(body.StartTime > body.EndTime)
                throw new BadRequestException($"Start Time : {body.StartTime} is more than End Time : {body.EndTime}");
            
            predicate = predicate.And(x => x.Id != body.IdInvitationBookingSettingBreak && x.IdInvitationBookingSetting == data.IdInvitationBookingSetting && x.DateInvitation == body.AppointmentDate);

            predicate = predicate.And(x => x.StartTime == body.StartTime || x.EndTime == body.EndTime 
                    || (x.StartTime < body.StartTime
                        ? (x.EndTime > body.StartTime && x.EndTime < body.EndTime) || x.EndTime > body.EndTime
                        : (body.EndTime > x.StartTime && body.EndTime < x.EndTime) || body.EndTime > x.EndTime));

            var isTimeExist = await _dbContext.Entity<TrInvitationBookingSettingBreak>()
                .Where(predicate)
                .FirstOrDefaultAsync(CancellationToken);
                
            if (isTimeExist != null)
                throw new BadRequestException($"Time Range : {body.StartTime} - {body.EndTime} for break has been used");

            data.BreakName = body.BreakName;
            data.StartTime = body.StartTime;
            data.EndTime = body.EndTime;
            data.BreakType = body.BreakType;
            data.DateInvitation = body.AppointmentDate;
            data.IdGrade = body.IdGrade;

            _dbContext.Entity<TrInvitationBookingSettingBreak>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

    }
}
