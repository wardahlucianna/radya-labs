using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Comparers;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.ClassSession;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.ClassSession
{
    public class GetClassSessionHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetClassSessionHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetClassSessionRequest>(nameof(GetClassSessionRequest.IdUser),
                                                                       nameof(GetClassSessionRequest.IdSchool),
                                                                       nameof(GetClassSessionRequest.Date));

            var result = await GetClassAndSessions(param);

            return Request.CreateApiResult2(result as object);
        }

        public async Task<IEnumerable<GetClassSessionResult>> GetClassAndSessions(GetClassSessionRequest param)
        {
            var uniqueComparer = new UniqueIdComparer<SessionOfClass>();

            #region Check user Homeroom teacher or not for this homeroom
            var isHomeroomTeacher = !string.IsNullOrEmpty(param.IdHomeroom) ? await _dbContext.Entity<MsHomeroomTeacher>()
                .AnyAsync(x => x.IdBinusian == param.IdUser
                          && x.IdHomeroom == param.IdHomeroom, CancellationToken) : false;

            #endregion

            if (isHomeroomTeacher)
            {
                var query = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Where(x
                    => x.IdHomeroom == param.IdHomeroom
                    && EF.Functions.DateDiffDay(x.ScheduleDate, param.Date) == 0
                    && x.IsGenerated)
                .ToListAsync(CancellationToken);
                return query
                .GroupBy(x => x.ClassID)
                .Select(x => new GetClassSessionResult
                {
                    Id = x.First().Id,
                    ClassId = x.Key,
                    Description = x.First().SubjectName,
                    Sessions = x
                        .OrderBy(y => y.StartTime)
                        .Select(y => new SessionOfClass
                        {
                            Id = y.IdSession,
                            SessionId = y.SessionID,
                            StartTime = y.StartTime,
                            EndTime = y.EndTime
                        })
                        .Distinct(uniqueComparer)
                        .OrderBy(x => int.TryParse(x.SessionId, out var sessionNumber) ? sessionNumber : 0)
                });
            }
            else
            {

                var AcademicyearByDate = await _dbContext.Entity<MsPeriod>()
                   .Include(x => x.Grade)
                       .ThenInclude(x => x.Level)
                           .ThenInclude(x => x.AcademicYear)
               .Where(x => param.Date.Date >= x.AttendanceStartDate.Date)
               .Where(x => param.Date.Date <= x.AttendanceEndDate.Date)
               .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
               .GroupBy(x => new
               {
                   x.Grade.Level.IdAcademicYear
               })
               .Select(x => x.Key)
               .FirstOrDefaultAsync(CancellationToken);

                var allowIsAttendanceEntryByClassId = AcademicyearByDate != null ? await _dbContext.Entity<MsLessonTeacher>()
                 .Include(x => x.Lesson)
                 .Where(x => x.IdUser == param.IdUser)
                 .Where(x => x.Lesson.IdAcademicYear == AcademicyearByDate.IdAcademicYear)
                 .Where(x => x.IsAttendance)
                 .Select(x => x.Lesson.Id)
                 .ToListAsync(CancellationToken) : new List<string>();

                var query = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                    .Include(x => x.Homeroom.Grade.Level.MappingAttendances)
                .Where(x
                    => (!string.IsNullOrEmpty(param.IdHomeroom) ? x.IdHomeroom == param.IdHomeroom : true)
                    && (x.IdUser == param.IdUser || allowIsAttendanceEntryByClassId.Contains(x.IdLesson))
                    && EF.Functions.DateDiffDay(x.ScheduleDate, param.Date) == 0
                    && x.IsGenerated
                    && x.Homeroom.Grade.Level.MappingAttendances.Any(y => y.AbsentTerms == Common.Model.Enums.AbsentTerm.Session))
                .ToListAsync(CancellationToken);
                return query
                .GroupBy(x => x.ClassID)
                .Select(x => new GetClassSessionResult
                {
                    Id = x.First().Id,
                    ClassId = x.Key,
                    Description = x.First().SubjectName,
                    Sessions = x
                        .OrderBy(y => y.StartTime)
                        .Select(y => new SessionOfClass
                        {
                            Id = y.IdSession,
                            SessionId = y.SessionID,
                            StartTime = y.StartTime,
                            EndTime = y.EndTime
                        })
                        .Distinct(uniqueComparer)
                        .OrderBy(x => int.TryParse(x.SessionId, out var sessionNumber) ? sessionNumber : 0)
                });
            }
        }
    }
}
