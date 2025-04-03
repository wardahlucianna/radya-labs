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
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class GetGradesByEventHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = new[]
        {
            nameof(GetGradesByEventRequest.IdEvent),
            nameof(GetGradesByEventRequest.IdLevel)
        };

        private readonly IAttendanceDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;

        public GetGradesByEventHandler(IAttendanceDbContext dbContext, IFeatureManagerSnapshot featureManager)
        {
            _dbContext = dbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradesByEventRequest>(_requiredParams);

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                var eventFor = await _dbContext.Entity<TrEventIntendedFor>()
                    .Where(x => x.IdEvent == param.IdEvent && x.IntendedFor == RoleConstant.Student)
                    .Select(x => new { IdIntendedFor = x.Id, x.Option })
                    .FirstOrDefaultAsync(CancellationToken);
                
                var idGrades = Enumerable.Empty<string>();
                if (eventFor is {})
                {
                    idGrades = eventFor.Option switch
                    {
                        nameof(EventOptionType.Level) => await _dbContext.Entity<TrEventIntendedForLevelStudent>()
                            .Where(x => x.IdEventIntendedFor == eventFor.IdIntendedFor)
                            .SelectMany(x => x.Level.Grades.Select(y => y.Id))
                            .Distinct()
                            .ToListAsync(CancellationToken),
                        nameof(EventOptionType.Grade) => await _dbContext.Entity<TrEventIntendedForGradeStudent>()
                            .Where(x => x.IdEventIntendedFor == eventFor.IdIntendedFor)
                            .Select(x => x.Homeroom.GradePathwayClassroom.GradePathway.IdGrade)
                            .Distinct()
                            .ToListAsync(CancellationToken),
                        nameof(EventOptionType.Personal) => await _dbContext.Entity<TrEventIntendedForPersonalStudent>()
                            .Where(x => x.IdEventIntendedFor == eventFor.IdIntendedFor)
                            .SelectMany(x => x.Student.StudentGrades.Select(y => y.IdGrade))
                            .Distinct()
                            .ToListAsync(CancellationToken),
                        _ => idGrades
                    };
                }

                var predicateGrade = PredicateBuilder.Create<MsGrade>(x => idGrades.Contains(x.Id));
                if (!string.IsNullOrWhiteSpace(param.Search))
                    predicateGrade = predicateGrade.And(x
                        => EF.Functions.Like(x.Code, $"%{param.Search}%")
                        || EF.Functions.Like(x.Description, $"%{param.Search}%"));

                var levels = await _dbContext.Entity<MsGrade>()
                    .Where(predicateGrade)
                    .Select(x => new CodeWithIdVm(x.Id, x.Code, x.Description))
                    .ToListAsync(CancellationToken);
                
                return Request.CreateApiResult2(levels as object);
            }

            return Request.CreateApiResult2();
        }
    }
}
