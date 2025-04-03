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
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetEventParentAndStudentHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetEventParentAndStudentRequest.IdAcademicYear),
        };
        private static readonly string[] _columns = { "EventName", "EventType", "StartDate", "EndDate" };
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;


        public GetEventParentAndStudentHandler(ISchedulingDbContext AppointmentBookingDbContext, IMachineDateTime dateTime)
        {
            _dbContext = AppointmentBookingDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEventParentAndStudentRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<TrEvent>(x => x.IsActive==true);
            
            predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear && x.StatusEvent == "Approved");

            predicate = predicate.And(x => x.EventIntendedFor.Any(y => y.IntendedFor == "Parent" || y.IntendedFor == "Student"));
            
            predicate = predicate.And(x => x.EventDetails.Any(y => y.StartDate > _dateTime.ServerTime.Date));

            if(!string.IsNullOrEmpty(param.StartDate) || !string.IsNullOrEmpty(param.EndDate))
            {
                predicate = predicate.And(x => x.EventDetails.Any(y
                    => y.StartDate == Convert.ToDateTime(param.StartDate) || y.EndDate == Convert.ToDateTime(param.EndDate) 
                    || (y.StartDate < Convert.ToDateTime(param.StartDate)
                        ? (y.EndDate > Convert.ToDateTime(param.StartDate) && y.EndDate < Convert.ToDateTime(param.EndDate)) || y.EndDate > Convert.ToDateTime(param.EndDate)
                        : (Convert.ToDateTime(param.EndDate) > y.StartDate && Convert.ToDateTime(param.EndDate) < y.EndDate) || Convert.ToDateTime(param.EndDate) > y.EndDate)));
            }

            if (!string.IsNullOrEmpty(param.IdEventType))
                predicate = predicate.And(x => x.IdEventType== param.IdEventType);
            
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, $"%{param.Search}%"));

            predicate = predicate.And(x => x.IsStudentInvolvement == false);

            var dataQuery = _dbContext.Entity<TrEvent>()
                .Include(e => e.EventType)
                .Include(e => e.EventIntendedFor)
                .Include(x => x.EventDetails)
                .Where(predicate);

            var query = dataQuery
               .Select(x => new
               {
                   Id = x.Id,
                   EventId = x.Id,
                   EventName = x.Name,
                   EventType = x.EventType.Description,
                   StartDate = x.EventDetails.Min(e=>e.StartDate),
                   EndDate = x.EventDetails.Max(e => e.EndDate)
               })
               .OrderBy(x => x.StartDate);

            //ordering
            switch (param.OrderBy)
            {
                case "EventName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EventName)
                        : query.OrderBy(x => x.EventName);
                    break;
                case "EventType":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EventType)
                        : query.OrderBy(x => x.EventType);
                    break;
                case "StartDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StartDate)
                        : query.OrderBy(x => x.StartDate);
                    break;
                case "EndDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EndDate)
                        : query.OrderBy(x => x.EndDate);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetEventParentAndStudentResult
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    EventName = x.EventName,
                    EventType = x.EventType,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
                items = result.Select(x => new GetEventParentAndStudentResult
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    EventName = x.EventName,
                    EventType = x.EventType,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.EventId).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
