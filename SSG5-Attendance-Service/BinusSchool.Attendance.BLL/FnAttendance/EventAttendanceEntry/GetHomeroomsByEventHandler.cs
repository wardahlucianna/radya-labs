using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.EventAttendanceEntry;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class GetHomeroomsByEventHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = new[]
        {
            nameof(GetHomeroomsByEventRequest.IdEvent),
            nameof(GetHomeroomsByEventRequest.IdGrade)
        };

        private readonly IAttendanceDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;

        public GetHomeroomsByEventHandler(IAttendanceDbContext dbContext, IFeatureManagerSnapshot featureManager)
        {
            _dbContext = dbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetHomeroomsByEventRequest>(_requiredParams);

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                var eventFor = await _dbContext.Entity<TrEventIntendedFor>()
                    .Where(x => x.IdEvent == param.IdEvent && x.IntendedFor == RoleConstant.Student)
                    .Select(x => new { IdIntendedFor = x.Id, x.Option })
                    .FirstOrDefaultAsync(CancellationToken);
                
                var idHomerooms = Enumerable.Empty<string>();
                if (eventFor is {})
                {
                    idHomerooms = eventFor.Option switch
                    {
                        nameof(EventOptionType.Grade) => await _dbContext.Entity<TrEventIntendedForGradeStudent>()
                            .Where(x => x.IdEventIntendedFor == eventFor.IdIntendedFor)
                            .Select(x => x.IdHomeroom)
                            .Distinct()
                            .ToListAsync(CancellationToken),
                        _ => idHomerooms
                    };
                }

                var predicateHomeroom = PredicateBuilder.Create<MsHomeroom>(x => idHomerooms.Contains(x.Id));
                var levels = await _dbContext.Entity<MsHomeroom>()
                    .Where(predicateHomeroom)
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = $"{x.GradePathwayClassroom.GradePathway.Grade.Code}{x.GradePathwayClassroom.Classroom.Code}"
                    })
                    .Where(x => string.IsNullOrEmpty(param.Search) || EF.Functions.Like(x.Description, $"%{param.Search}%"))
                    .ToListAsync(CancellationToken);
                
                return Request.CreateApiResult2(levels as object);
            }

            return Request.CreateApiResult2();
        }
    }
}
