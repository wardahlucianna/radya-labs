using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetListTimeFromQuotaDurationHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetListTimeFromQuotaDurationRequest.IdInvitationBookingSetting),
            nameof(GetListTimeFromQuotaDurationRequest.DateInvitation)
        };
        private static readonly string[] _columns = { "StartTime", "EndTime" };
        private readonly ISchedulingDbContext _dbContext;

        public GetListTimeFromQuotaDurationHandler(ISchedulingDbContext SchedulingDbContext)
        {
            _dbContext = SchedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListTimeFromQuotaDurationRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<TrInvitationBookingSettingQuota>(x => x.IsActive==true);
            
            predicate = predicate.And(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting && (x.SettingType != 2 ? x.DateInvitation == param.DateInvitation : (x.InvitationBookingSetting.InvitationStartDate.Date == param.DateInvitation.Date || x.InvitationBookingSetting.InvitationEndDate.Date == param.DateInvitation.Date
                    || (x.InvitationBookingSetting.InvitationStartDate.Date < param.DateInvitation.Date
                        ? (x.InvitationBookingSetting.InvitationEndDate.Date > param.DateInvitation.Date && x.InvitationBookingSetting.InvitationEndDate.Date < param.DateInvitation.Date) || x.InvitationBookingSetting.InvitationEndDate.Date > param.DateInvitation.Date
                        : (param.DateInvitation.Date > x.InvitationBookingSetting.InvitationStartDate.Date && param.DateInvitation.Date < x.InvitationBookingSetting.InvitationEndDate.Date) || param.DateInvitation.Date > x.InvitationBookingSetting.InvitationEndDate.Date))));
            
            if(!string.IsNullOrWhiteSpace(param.IdGrade))
            {
                predicate = predicate.And(x => x.IdGrade == param.IdGrade);
            }

            var dataQuery = _dbContext.Entity<TrInvitationBookingSettingQuota>()
                .Where(predicate);

            var query = dataQuery
               .Select(x => new
               {
                   Id = x.Id,
                   StartTime = x.StartTime,
                   EndTime = x.EndTime,
                   BreakBetweenSession = x.BreakBetweenSession != null ? x.BreakBetweenSession : 0,
                   Duration = x.Duration,
                   SettingType = x.SettingType,
                   AppointmentDate = x.DateInvitation,
                   IdInvitationBookingSetting = x.IdInvitationBookingSetting
               })
               .OrderBy(x => x.StartTime);

            IReadOnlyList<IItemValueVm> items;

            var result = await query
                    .ToListAsync(CancellationToken);

            List<TimeSpan> dataTimeGenerated = new List<TimeSpan>();

            List<GetListTimeFromQuotaDurationResult> ListTimeFromQuotaDurationResult= new List<GetListTimeFromQuotaDurationResult>();

            foreach(var dataTime in result)
            {

                var StartTime = dataTime.StartTime;
                var EndTime = dataTime.StartTime;

                TimeSpan Interval = TimeSpan.FromMinutes(dataTime.Duration);
                while (EndTime < dataTime.EndTime)
                {
                    StartTime = EndTime;
                    EndTime = EndTime.Add(Interval);

                    ListTimeFromQuotaDurationResult.Add(new GetListTimeFromQuotaDurationResult
                    {
                        StartTime = StartTime,
                        EndTime = EndTime
                    });

                    if(dataTime.BreakBetweenSession != 0)
                    {
                        TimeSpan BreakBetweenSession = TimeSpan.FromMinutes(dataTime.BreakBetweenSession.Value);
                        EndTime = EndTime.Add(BreakBetweenSession);
                    }
                };
            }

                items = ListTimeFromQuotaDurationResult.Select(x => new GetListTimeFromQuotaDurationResult
                {
                   Id = x.Id,
                   StartTime = x.StartTime,
                   EndTime = x.EndTime
                }).ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
