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

    public class DeleteInvitationBookingSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DeleteInvitationBookingSettingHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteInvitationBookingSettingRequest, DeleteInvitationBookingSettingValidator>();

            if (body.IdInvitationBookingSetting.Any())
            {
                var deleteds = await _dbContext.Entity<TrInvitationBookingSetting>()
                                                 .Where(x => body.IdInvitationBookingSetting.Contains(x.Id))
                                                 .ToListAsync(CancellationToken);

                foreach (var deleted in deleteds)
                {
                    deleted.IsActive = false;
                    _dbContext.Entity<TrInvitationBookingSetting>().Update(deleted);
                }
            }
            
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

    }
}
