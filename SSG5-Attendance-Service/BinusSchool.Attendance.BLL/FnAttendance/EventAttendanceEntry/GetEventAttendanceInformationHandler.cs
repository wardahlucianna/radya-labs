using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class GetEventAttendanceInformationHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = new[]
        {
            nameof(GetEventAttendanceInformationRequest.IdEvent)
        };

        private readonly IAttendanceDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;

        public GetEventAttendanceInformationHandler(IAttendanceDbContext dbContext, IFeatureManagerSnapshot featureManager)
        {
            _dbContext = dbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEventAttendanceInformationRequest>(_requiredParams);

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                var result2 = await _dbContext.Entity<TrEventIntendedFor>()
                        .Include(e => e.Event).ThenInclude(e => e.EventCoordinators).ThenInclude(e => e.User)
                    // .Where(x => x.Id == param.IdEvent && x.EventIntendedFor.Any(y => y.IntendedFor == RoleConstant.Student))
                    .Where(x => x.IdEvent == param.IdEvent && x.IntendedFor == RoleConstant.Student)
                    .Select(x => new EventAttendanceInformationResult
                    {
                        Id = x.IdEvent,
                        Description = x.Event.Name,
                        EventOptionType = Enum.Parse<EventOptionType>(x.Option),
                        PIC = x.Event.EventCoordinators
                                .Select(z => new NameValueVm
                                {
                                    Id = z.User.Id,
                                    Name = z.User.DisplayName
                                }).FirstOrDefault(),
                        EventDates = x.Event.EventDetails
                            .Select(y => new EventDate
                            {
                                StartDate = y.StartDate,
                                StartTime = y.StartDate.TimeOfDay,
                                EndDate = y.EndDate,
                                EndTime = y.EndDate.TimeOfDay
                            })
                            .ToList(),
                        AvailableCheckDates = x.EventIntendedForAttendanceStudents
                            .SelectMany(y => y.EventIntendedForAtdCheckStudents.Select(z => z.StartDate))
                            .ToList()
                    })
                    .FirstOrDefaultAsync(CancellationToken);

                if (result2 is null)
                    throw new NotFoundException("Event is not found");

                result2.AvailableCheckDates = result2.AvailableCheckDates.Distinct().ToList();

                return Request.CreateApiResult2(result2 as object);
            }

            return Request.CreateApiResult2();
        }
    }
}
