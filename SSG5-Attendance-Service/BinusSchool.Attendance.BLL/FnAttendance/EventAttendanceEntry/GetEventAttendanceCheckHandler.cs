using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class GetEventAttendanceCheckHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = new[]
        {
            nameof(GetEventAttendanceCheckRequest.IdEvent),
            nameof(GetEventAttendanceCheckRequest.Date),
        };

        private readonly IAttendanceDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;

        public GetEventAttendanceCheckHandler(IAttendanceDbContext dbContext, IFeatureManagerSnapshot featureManager)
        {
            _dbContext = dbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEventAttendanceCheckRequest>(_requiredParams);

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                var @event = await _dbContext.Entity<TrEvent>()
                    .Include(x => x.EventIntendedFor)
                        .ThenInclude(x => x.EventIntendedForAttendanceStudents)
                        .ThenInclude(x => x.EventIntendedForAtdCheckStudents)
                    .Where(x => x.Id == param.IdEvent && x.EventIntendedFor.Any(y => y.IntendedFor == RoleConstant.Student))
                    .FirstOrDefaultAsync(CancellationToken);

                if (@event is null)
                    throw new NotFoundException("Event is not found");
                
                var results = @event.EventIntendedFor
                    .Where(x => x.IntendedFor == RoleConstant.Student)
                    .SelectMany(x => x.EventIntendedForAttendanceStudents
                        .Where(y => y.IsSetAttendance)
                        .SelectMany(y => y.EventIntendedForAtdCheckStudents
                            .Where(z => z.StartDate.Date == param.Date).OrderBy(e => e.Time).ThenBy(e => e.CheckName)
                            .Select(z => new EventCheck
                            {
                                Id = z.Id,
                                Description = z.CheckName,
                                Date = z.StartDate,
                                AttendanceTime = z.Time
                            })));
                
                return Request.CreateApiResult2(results as object);
            }

            return Request.CreateApiResult2();
        }
    }
}
