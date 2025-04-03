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
    public class GetListSchedulePreviewHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetSchedulePreviewRequest.IdInvitationBookingSetting),
            nameof(GetSchedulePreviewRequest.InvitationDate),
        };
        private static readonly string[] _columns = { "StartTime", "EndTime", "Description", "QuotaSlot", "Duration" };
        private readonly ISchedulingDbContext _dbContext;

        public GetListSchedulePreviewHandler(ISchedulingDbContext SchedulingDbContext)
        {
            _dbContext = SchedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSchedulePreviewRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<TrInvitationBookingSettingSchedule>(x => x.IsActive==true);
            
            predicate = predicate.And(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting && x.DateInvitation == param.InvitationDate);
            
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, $"%{param.Search}%"));

            var dataQuery = _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                .Where(predicate);

            var queryDistinct = dataQuery
               .Select(x => new
               {
                   IdInvitationBookingSetting = x.IdInvitationBookingSetting,
                   Description = x.Description,
                   InvitationDate = x.DateInvitation,
                   StartTime = x.StartTime,
                   EndTime = x.EndTime,
                   QuotaSlot = x.QuotaSlot,
                   Duration = x.Duration,
                   BreakName = x.BreakName,
                   IsAvailable = x.IsAvailable,
                   IsFixedBreak = x.IsFixedBreak,
               }).Distinct();

            var query = queryDistinct.ToList();

            switch (param.OrderBy)
            {
                case "StartTime":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StartTime).ToList()
                        : query.OrderBy(x => x.StartTime).ToList();
                    break;
                case "EndTime":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EndTime).ToList()
                        : query.OrderBy(x => x.EndTime).ToList();
                    break;
                case "Description":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Description).ToList()
                        : query.OrderBy(x => x.Description).ToList();
                    break;
                case "QuotaSlot":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.QuotaSlot).ToList()
                        : query.OrderBy(x => x.QuotaSlot).ToList();
                    break;
                case "Duration":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Duration).ToList()
                        : query.OrderBy(x => x.Duration).ToList();
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();

                items = result.Select(x => new GetSchedulePreviewResult
                {
                    IdInvitationBookingSetting = x.IdInvitationBookingSetting,
                    Description = x.Description,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    QuotaSlot = x.QuotaSlot,
                    Duration = x.Duration,
                    IsAvailable = x.IsAvailable,
                    BreakName = x.BreakName,
                    AvailableDescription = x.BreakName == null ? null : "Available to set as " + x.BreakName
                })
                .OrderBy(x => x.StartTime)
                .ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();
                items = result.Select(x => new GetSchedulePreviewResult
                {
                    IdInvitationBookingSetting = x.IdInvitationBookingSetting,
                    Description = x.Description,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    QuotaSlot = x.QuotaSlot,
                    Duration = x.Duration,
                    IsAvailable = x.IsAvailable, 
                    BreakName = x.BreakName,
                    AvailableDescription = x.BreakName == null ? null : "Available to set as " + x.BreakName
                })
                .OrderBy(x => x.StartTime)
                .ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.IdInvitationBookingSetting).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
