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
using System.Security.Policy;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetListInvitationBookingSettingHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetListInvitationBookingSettingRequest.IdAcademicYear),
            nameof(GetListInvitationBookingSettingRequest.Status),
        };
        private static readonly string[] _columns = { "InvitationName" };
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetListInvitationBookingSettingHandler(ISchedulingDbContext SchedulingDbContext, IMachineDateTime dateTime)
        {
            _dbContext = SchedulingDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListInvitationBookingSettingRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<TrInvitationBookingSetting>(x => x.IsActive==true);
            
            predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear && x.Status == param.Status);
            
            if(!string.IsNullOrEmpty(param.InvitationStartDate) || !string.IsNullOrEmpty(param.InvitationEndDate))
            {
                predicate = predicate.And(y => y.InvitationStartDate == Convert.ToDateTime(param.InvitationStartDate) || y.InvitationEndDate == Convert.ToDateTime(param.InvitationEndDate) 
                    || (y.InvitationStartDate < Convert.ToDateTime(param.InvitationStartDate)
                        ? (y.InvitationEndDate > Convert.ToDateTime(param.InvitationStartDate) && y.InvitationEndDate < Convert.ToDateTime(param.InvitationEndDate)) || y.InvitationEndDate > Convert.ToDateTime(param.InvitationEndDate)
                        : (Convert.ToDateTime(param.InvitationEndDate) > y.InvitationStartDate && Convert.ToDateTime(param.InvitationEndDate) < y.InvitationEndDate) || Convert.ToDateTime(param.InvitationEndDate) > y.InvitationEndDate));
            }

            if(param.IsMyInvitationBooking == true)
            {
                predicate = predicate.And(x => x.InvitationBookingSettingVenueDates.Any(y => y.InvitationBookingSettingVenueDtl.Any(z => z.IdUserTeacher == param.IdUser)));
            }

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.InvitationName, $"%{param.Search}%"));

            var dataQuery = _dbContext.Entity<TrInvitationBookingSetting>()
                .Include(x => x.InvitationBookingSettingVenueDates).ThenInclude(x => x.InvitationBookingSettingVenueDtl)
                .Include(x => x.AcademicYears)
                .Include(x => x.InvitationBookings)
                .Where(predicate);

            var query = dataQuery
               .Select(x => new
               {
                   Id = x.Id,
                   DateIn = x.DateIn,
                   AcademicYear = x.AcademicYears.Description,
                   InvitationName = x.InvitationName,
                   InvitationStartDate = x.InvitationStartDate,
                   InvitationEndDate = x.InvitationEndDate,
                   CanEdit = x.InvitationStartDate > _dateTime.ServerTime.Date ? x.InvitationBookings.Count() == 0 ? true : false : false,
                   CanDelete = x.InvitationStartDate > _dateTime.ServerTime.Date ? x.InvitationBookings.Count() == 0 ? true : false : false,
                   ParentBookingStartDate =  x.ParentBookingStartDate,
                   ParentBookingEndDate =  x.ParentBookingEndDate,
                   StaffBookingStartDate = x.StaffBookingStartDate,
                   StaffBookingEndDate = x.StaffBookingEndDate,
               });

            //ordering
            switch (param.OrderBy)
            {
                case "InvitationName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.InvitationName)
                        : query.OrderBy(x => x.InvitationName);
                    break;
                case "InvitationDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.InvitationStartDate)
                        : query.OrderBy(x => x.InvitationStartDate);
                    break;
                default:
                    query = query.OrderByDescending(x => x.DateIn);
                    break;
            };

            List<GetListInvitationBookingSettingResult> items =  new List<GetListInvitationBookingSettingResult>();
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                foreach(var item in result)
                {
                    var CanEditVenueOnly = false;
                    if (item.ParentBookingStartDate == null)
                    {
                        if (item.StaffBookingStartDate >= _dateTime.ServerTime && item.StaffBookingEndDate <= _dateTime.ServerTime)
                            CanEditVenueOnly = true;
                    }
                    else if (item.StaffBookingStartDate == null)
                    {
                        if (item.ParentBookingStartDate >= _dateTime.ServerTime && item.ParentBookingEndDate <= _dateTime.ServerTime)
                            CanEditVenueOnly = true;
                    }
                    else
                    {
                        if ((item.StaffBookingStartDate >= _dateTime.ServerTime && item.StaffBookingEndDate <= _dateTime.ServerTime) ||
                            (item.ParentBookingStartDate >= _dateTime.ServerTime && item.ParentBookingEndDate <= _dateTime.ServerTime))
                            CanEditVenueOnly = true;
                    }

                    items.Add(new GetListInvitationBookingSettingResult
                    {
                        Id = item.Id,
                        AcademicYear = item.AcademicYear,
                        InvitationName = item.InvitationName,
                        InvitationStartDate = item.InvitationStartDate,
                        InvitationEndDate = item.InvitationEndDate,
                        CanEdit = item.CanEdit,
                        CanDelete = item.CanDelete,
                        CanEditVenueOnly = CanEditVenueOnly
                    });
                }
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                foreach (var item in result)
                {
                    if(item.Id== "1bf6aad0-5521-41cc-b920-07ed70e30186")
                    {

                    }
                    var CanEditVenueOnly = false;
                    if (item.ParentBookingStartDate == null)
                    {
                        if (item.StaffBookingStartDate <= _dateTime.ServerTime && item.StaffBookingEndDate >= _dateTime.ServerTime)
                            CanEditVenueOnly = true;
                    }
                    else if (item.StaffBookingStartDate == null)
                    {
                        if (item.ParentBookingStartDate <= _dateTime.ServerTime && item.ParentBookingEndDate >= _dateTime.ServerTime)
                            CanEditVenueOnly = true;
                    }
                    else
                    {
                        if ((item.StaffBookingStartDate <= _dateTime.ServerTime && item.StaffBookingEndDate >= _dateTime.ServerTime) ||
                            (item.ParentBookingStartDate <= _dateTime.ServerTime && item.ParentBookingEndDate >= _dateTime.ServerTime))
                            CanEditVenueOnly = true;
                    }

                    items.Add(new GetListInvitationBookingSettingResult
                    {
                        Id = item.Id,
                        AcademicYear = item.AcademicYear,
                        InvitationName = item.InvitationName,
                        InvitationStartDate = item.InvitationStartDate,
                        InvitationEndDate = item.InvitationEndDate,
                        CanEdit = item.CanEdit,
                        CanDelete = item.CanDelete,
                        CanEditVenueOnly = CanEditVenueOnly
                    });
                }
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
