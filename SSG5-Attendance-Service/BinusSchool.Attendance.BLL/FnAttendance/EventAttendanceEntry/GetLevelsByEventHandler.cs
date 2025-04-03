using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
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
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace BinusSchool.Attendance.FnAttendance.EventAttendanceEntry
{
    public class GetLevelsByEventHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = new[]
        {
            nameof(GetLevelsByEventRequest.IdEvent)
        };

        private readonly IAttendanceDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;

        public GetLevelsByEventHandler(IAttendanceDbContext dbContext, IFeatureManagerSnapshot featureManager)
        {
            _dbContext = dbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLevelsByEventRequest>(_requiredParams);

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                var eventFor = await _dbContext.Entity<TrEventIntendedFor>()
                    .Include(x => x.Event)
                    .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdPICStudents)
                    .Where(x => x.IdEvent == param.IdEvent && x.IntendedFor == RoleConstant.Student
                            && x.EventIntendedForAttendanceStudents.Any(y => y.EventIntendedForAtdPICStudents.Any(z => z.IdUser == AuthInfo.UserId)))
                    .Select(x => new { IdIntendedFor = x.Id, x.Option, x.Event.IdAcademicYear, 
                            PICType = x.EventIntendedForAttendanceStudents.FirstOrDefault().EventIntendedForAtdPICStudents.FirstOrDefault().Type})
                    .FirstOrDefaultAsync(CancellationToken);

                var idLevels = Enumerable.Empty<string>();
                if (eventFor is {})
                {
                    idLevels = eventFor.Option switch
                    {
                        nameof(EventOptionType.All) => await _dbContext.Entity<MsLevel>()
                            .Where(x => x.IdAcademicYear == eventFor.IdAcademicYear)
                            .Select(x => x.Id)
                            .ToListAsync(CancellationToken),
                        nameof(EventOptionType.Level) => await _dbContext.Entity<TrEventIntendedForLevelStudent>()
                            .Where(x => x.IdEventIntendedFor == eventFor.IdIntendedFor)
                            .Select(x => x.IdLevel)
                            .Distinct()
                            .ToListAsync(CancellationToken),
                        nameof(EventOptionType.Grade) => await _dbContext.Entity<TrEventIntendedForGradeStudent>()
                            .Where(x => x.IdEventIntendedFor == eventFor.IdIntendedFor)
                            .Select(x => x.Homeroom.GradePathwayClassroom.GradePathway.Grade.IdLevel)
                            .Distinct()
                            .ToListAsync(CancellationToken),
                        nameof(EventOptionType.Personal) => await _dbContext.Entity<TrEventIntendedForPersonalStudent>()
                            .Where(x => x.IdEventIntendedFor == eventFor.IdIntendedFor)
                            .SelectMany(x => x.Student.StudentGrades.Select(y => y.Grade.IdLevel))
                            .Distinct()
                            .ToListAsync(CancellationToken),
                        _ => idLevels
                    };
                }

                List<string> idLevelHomeRoom = new List<string>();
                if (eventFor.PICType == EventIntendedForAttendancePICStudent.Homeroom)
                {
                    idLevelHomeRoom = await _dbContext.Entity<MsHomeroomTeacher>()
                    .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level)
                    .Where(x => x.IdBinusian == AuthInfo.UserId && x.Homeroom.AcademicYear.Id == eventFor.IdAcademicYear)
                    .Select(x => x.Homeroom.Grade.Level.Id)
                    .Distinct()
                    .ToListAsync(CancellationToken);
                }

                var predicateLevel = PredicateBuilder.Create<MsLevel>(x => idLevelHomeRoom.Count == 0 ? idLevels.Contains(x.Id) : idLevelHomeRoom.Contains(x.Id));

                if (!string.IsNullOrWhiteSpace(param.Search))
                    predicateLevel = predicateLevel.And(x
                        => EF.Functions.Like(x.Code, $"%{param.Search}%")
                        || EF.Functions.Like(x.Description, $"%{param.Search}%"));

                var levels = await _dbContext.Entity<MsLevel>()
                    .Where(predicateLevel)
                    .Select(x => new CodeWithIdVm(x.Id, x.Code, x.Description))
                    .ToListAsync(CancellationToken);
                
                return Request.CreateApiResult2(levels as object);
            }

            return Request.CreateApiResult2();
        }
    }
}
