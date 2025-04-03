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
    public class TimeTablePreferencesGetByUserHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetTimeTableByUserRequest.IdSchoolUser), 
            nameof(GetTimeTableByUserRequest.IdSchoolAcademicYear)
        });

        private readonly ITeachingDbContext _dbContext;

        public TimeTablePreferencesGetByUserHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTimeTableByUserRequest>(_requiredParams.Value);

            var data = await _dbContext.Entity<MsSubjectCombination>()
                .Include(x => x.Subject).ThenInclude(x => x.Grade)
                .Include(x => x.Subject).ThenInclude(x => x.Department)
                .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                .Include(x => x.TimeTablePrefHeader)
                .Where(x 
                    => x.Subject.Grade.Level.IdAcademicYear == param.IdSchoolAcademicYear
                    && x.TimeTablePrefHeader != null
                    && x.TimeTablePrefHeader.IdParent == null
                    && x.TimeTablePrefHeader.TimetablePrefDetails.Any(y => y.TeachingLoads.Any(z => z.IdUser == param.IdSchoolUser)))
                .OrderBy(x => x.Subject.SubjectID)
                .ToListAsync(CancellationToken);

            var data2 = await _dbContext.Entity<TrTeachingLoad>()
            .Include(x=>x.TimetablePrefDetail)
            .Where(x=>x.IdUser == param.IdSchoolUser)
            .ToListAsync(CancellationToken);
            
            var result =
            (
                from _dataTeachingLoad in data2
                join _timeTablePrefheader in data on _dataTeachingLoad.TimetablePrefDetail?.IdTimetablePrefHeader equals _timeTablePrefheader.TimeTablePrefHeader.Id
                select new GetTimeTableByUserResult
                {
                    Id = _dataTeachingLoad.Id,
                    Class = _timeTablePrefheader.GradePathwayClassroom?.Classroom.Description,
                    Department = _timeTablePrefheader.Subject?.Department.Description,
                    Grade = _timeTablePrefheader.Subject?.Grade.Description,
                    Subject = _timeTablePrefheader.Subject?.Description,
                    TotalSession = _dataTeachingLoad.Load
                }
            ).ToList();

            return Request.CreateApiResult2(result as object);
        }
    }
}
