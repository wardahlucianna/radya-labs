using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences
{
    public class TimeTablePreferencesDashboardHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetDashboardTimeTableRequest.IdAcademicyear)
        });
        
        private readonly ITeachingDbContext _dbContext;
        
        public TimeTablePreferencesDashboardHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDashboardTimeTableRequest>(_requiredParams.Value);

            var timeTable = await _dbContext.Entity<MsSubjectCombination>()
                .Include(x => x.Subject).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.GradePathwayDetails)
                .Include(x => x.TimeTablePrefHeader)
                .Where(x 
                    => x.Subject.Grade.Level.IdAcademicYear == param.IdAcademicyear
                    && x.TimeTablePrefHeader != null
                    && x.TimeTablePrefHeader.IdParent == null)
                .OrderBy(x => x.Subject.SubjectID)
                .ToListAsync(CancellationToken);

            var mapped = (from _timeTable in timeTable
                          where
                              (string.IsNullOrEmpty(param.IdLevel) || _timeTable.Subject.Grade.IdLevel == param.IdLevel)
                              && (string.IsNullOrEmpty(param.IdGrade) || _timeTable.Subject.IdGrade == param.IdGrade)
                              && (string.IsNullOrEmpty(param.IdDepartment) || _timeTable.Subject.IdDepartment == param.IdDepartment)
                              && (string.IsNullOrEmpty(param.IdStreaming) || _timeTable.GradePathwayClassroom.GradePathway.GradePathwayDetails.Any(y => y.IdPathway == param.IdStreaming))
                              && (string.IsNullOrEmpty(param.IdSubjetc) || _timeTable.IdSubject == param.IdSubjetc)
                              && _timeTable.TimeTablePrefHeader.Status
                          select _timeTable.TimeTablePrefHeader.Id
                        ).ToList();
            var notMapped = (from _timeTable in timeTable
                             where
                            (string.IsNullOrEmpty(param.IdLevel) || _timeTable.Subject.Grade.IdLevel == param.IdLevel)
                            && (string.IsNullOrEmpty(param.IdGrade) || _timeTable.Subject.IdGrade == param.IdGrade)
                            && (string.IsNullOrEmpty(param.IdDepartment) || _timeTable.Subject.IdDepartment == param.IdDepartment)
                            && (string.IsNullOrEmpty(param.IdStreaming) || _timeTable.GradePathwayClassroom.GradePathway.GradePathwayDetails.Any(y => y.IdPathway == param.IdStreaming))
                            && (string.IsNullOrEmpty(param.IdSubjetc) || _timeTable.IdSubject == param.IdSubjetc)
                        && !_timeTable.TimeTablePrefHeader.Status
                             select _timeTable.TimeTablePrefHeader.Id
                    ).ToList();
            var sessionMapped = await _dbContext.Entity<TrTimetablePrefDetail>()
            .Where(x => mapped.Contains(x.IdTimetablePrefHeader))
            .Select(x => x.Count * x.Length).SumAsync();

            var sessionNotMapped = await _dbContext.Entity<TrTimetablePrefDetail>()
            .Where(x => notMapped.Contains(x.IdTimetablePrefHeader))
            .Select(x => x.Count * x.Length).SumAsync();

            var result = new GetDashboardTimeTableResult
            {
                Mapped = mapped.Count,
                NotMapped = notMapped.Count,
                TotalSessionMapped = sessionMapped,
                TotalSessionNotMapped = sessionNotMapped
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
