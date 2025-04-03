using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministration.validator;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.ApprovalAttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration
{
    public class SetStatusApprovalAttendanceAdministrationHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public SetStatusApprovalAttendanceAdministrationHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetStatusApprovalAttendanceAdministrationRequest, SetStatusApprovalAttendanceAdministrationValidator>();
            List<TrAttendanceEntry> trAttendanceEntries = new List<TrAttendanceEntry>();
            var data = await _dbContext.Entity<TrAttendanceAdministration>()
                        .Include(x => x.Attendance)
                        .Include(x => x.StudentGrade)
                            .ThenInclude(x => x.Student)
                        .Include(x => x.StudentGrade)
                            .ThenInclude(x => x.Grade)
                                .ThenInclude(x => x.Level)
                        .Where(x => x.Id == body.Id)
                        .FirstOrDefaultAsync(CancellationToken);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AttendanceAdministration"], "Id", body.Id));

            data.StatusApproval = body.IsApproved ? 1 : 2;

            if (body.IsApproved == true)
                {
                    #region Find all lesson by student , date and periode
                    var scheduleLessons = _dbContext.Entity<TrGeneratedScheduleLesson>()
                        .Include(x => x.GeneratedScheduleStudent)
                        .Where(x => x.GeneratedScheduleStudent.IdStudent == data.StudentGrade.Student.Id)
                        .Where(x => x.ScheduleDate.Date >= data.StartDate.Date)
                        .Where(x => x.ScheduleDate.Date <= data.EndDate.Date)
                        .AsQueryable();
                    
                    scheduleLessons = scheduleLessons.Where(x => (data.StartTime >= x.StartTime && data.StartTime <= x.EndTime)
                                                                     || (data.EndTime > x.StartTime && data.EndTime <= x.EndTime)
                                                                     || (data.StartTime <= x.StartTime && data.EndTime >= x.EndTime));
                    var _scheduleLesson = await scheduleLessons.Select(x => x.Id).ToListAsync();
                    #endregion

                    #region Find IdMappingAttendance For Each Student, case each student different level
                    MsAttendanceMappingAttendance mappingAttendance = new MsAttendanceMappingAttendance();
                    if (data.Attendance.AttendanceCategory == AttendanceCategory.Absent)
                    {
                        mappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                       .Include(x => x.MappingAttendance)
                           .ThenInclude(x => x.Level)
                       .Include(x => x.Attendance)
                       .Where(x => x.Attendance.AbsenceCategory.Value == AbsenceCategory.Unexcused)
                       .Where(x => x.MappingAttendance.IdLevel == data.StudentGrade.Grade.IdLevel)
                       .FirstOrDefaultAsync(CancellationToken);
                    }
                    else
                    {
                        mappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                        .Include(x => x.MappingAttendance)
                            .ThenInclude(x => x.Level)
                        .Include(x => x.Attendance)
                        .Where(x => x.IdAttendance == data.IdAttendance)
                        .Where(x => x.MappingAttendance.IdLevel == data.StudentGrade.Grade.IdLevel)
                        .FirstOrDefaultAsync(CancellationToken);
                    }


                    if (mappingAttendance is null)
                    {
                        var selectedMappingAttendance = await _dbContext.Entity<MsAttendance>()
                            .Where(x => x.Id == data.IdAttendance)
                            .Select(x => x.Description)
                            .FirstOrDefaultAsync(CancellationToken);

                        throw new BadRequestException($"Attendance name {selectedMappingAttendance} is not exist in level {data.StudentGrade.Grade.Level}.");
                    }
                    #endregion


                    var attendanceEntryByGeneratedScheduleLesson = await _dbContext.Entity<TrAttendanceEntry>()
                        .Where(x => _scheduleLesson.Any(y => y == x.IdGeneratedScheduleLesson)).ToListAsync();
                    List<TrAttendanceEntry> attendanceEntries = new List<TrAttendanceEntry>();
                    // update any existing entries
                    if (attendanceEntryByGeneratedScheduleLesson.Any())
                    {
                        foreach (var newEntryScheduleId in _scheduleLesson.Where(scheduleId => !attendanceEntryByGeneratedScheduleLesson.Any(y => y.IdGeneratedScheduleLesson == scheduleId)))
                        {
                            TrAttendanceEntry trAttendanceEntry = new TrAttendanceEntry
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdGeneratedScheduleLesson = newEntryScheduleId,
                                IdAttendanceMappingAttendance = mappingAttendance.Id,
                                FileEvidence = data.AbsencesFile,
                                Notes = data.Reason,
                                Status = AttendanceEntryStatus.Submitted,
                                IsFromAttendanceAdministration = true
                            };
                            trAttendanceEntries.Add(trAttendanceEntry);
                        }

                        _dbContext.Entity<TrAttendanceEntry>().UpdateRange(attendanceEntryByGeneratedScheduleLesson);
                    }
                    else
                    {
                        foreach (var newEntryScheduleId in _scheduleLesson)
                        {
                            TrAttendanceEntry trAttendanceEntry = new TrAttendanceEntry
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdGeneratedScheduleLesson = newEntryScheduleId,
                                IdAttendanceMappingAttendance = mappingAttendance.Id,
                                FileEvidence = data.AbsencesFile,
                                Notes = data.Reason,
                                Status = AttendanceEntryStatus.Submitted,
                                IsFromAttendanceAdministration = true
                            };
                            trAttendanceEntries.Add(trAttendanceEntry);
                        }
                    }
                }

            _dbContext.Entity<TrAttendanceAdministration>().Update(data);

            _dbContext.Entity<TrAttendanceEntry>().AddRange(trAttendanceEntries);

            await _dbContext.SaveChangesAsync(CancellationToken);
                
            return Request.CreateApiResult2();
        }
    }
}
