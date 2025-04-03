using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPOI.Util;

namespace BinusSchool.Attendance.FnLongRun.TimeTriggers
{
    public class MoveStudentAttendanceHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IAttendanceV2 _apiAttendanceV2;
        private readonly ILogger<MoveStudentAttendanceHandler> _logger;

        public MoveStudentAttendanceHandler(
            IServiceProvider serviceProvider,
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime,
            IAttendanceV2 apiAttendanceV2,
            ILogger<MoveStudentAttendanceHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _dateTime = dateTime;
            _logger = logger;
            _apiAttendanceV2 = apiAttendanceV2;
        }

        [FunctionName(nameof(MoveStudentAttendance))]
        public async Task MoveStudentAttendance([TimerTrigger(AttendanceLongRunTimeConstant.MoveStudentAttendanceConstantTime
#if DEBUG
                //, RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
            CancellationToken cancellationToken)
        {
            var schools = await _dbContext.Entity<MsSchool>()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var item in schools)
            {
                _logger.LogInformation("Moving student for school {Name} started", item.Name);

                var sw = Stopwatch.StartNew();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var date = _dateTime.ServerTime.Date.AddMonths(-1);
                    var listTrHomeroomStudentEnroll = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                                                        .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.Level)
                                                        .Where(e => e.DateIn >= date && e.HomeroomStudent.Homeroom.Grade.Level.AcademicYear.IdSchool==item.Id)
                                                        .ToListAsync(cancellationToken);

                    if (listTrHomeroomStudentEnroll.Any())
                    {
                        var listHomeroomStudentGroup = listTrHomeroomStudentEnroll
                                                        .GroupBy(e => new
                                                        {
                                                            e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear,
                                                            e.IdHomeroomStudent,
                                                            e.StartDate
                                                        });

                        foreach(var itemHomeroomStudentEnroll in listHomeroomStudentGroup)
                        {
                            var listLessonMoveStudentEnroll = itemHomeroomStudentEnroll
                                                                .Where(e=>e.IsShowHistory)
                                                                .GroupBy(e => new 
                                                                {
                                                                    IdLessonNew = e.IdLessonNew,
                                                                    IdLessonOld = e.IdLessonOld
                                                                })
                                                                .Select(e => new LessonMoveStudentEnroll
                                                                {
                                                                    IdLessonNew = e.Key.IdLessonNew,
                                                                    IdLessonOld = e.Key.IdLessonOld
                                                                })
                                                                .ToList();

                            UpdateAttendanceByMoveStudentEnrollRequest newUpdateAttendance = new UpdateAttendanceByMoveStudentEnrollRequest
                            {
                                IdAcademicYear = itemHomeroomStudentEnroll.Key.IdAcademicYear,
                                IdHomeroomStudent = itemHomeroomStudentEnroll.Key.IdHomeroomStudent,
                                StartDate = itemHomeroomStudentEnroll.Key.StartDate,
                                ListLessonMoveStudentEnroll = listLessonMoveStudentEnroll
                            };

                            var json_String = JsonSerializer.Serialize(newUpdateAttendance);
                            await _apiAttendanceV2.UpdateAttendanceByMoveStudentEnroll(newUpdateAttendance);
                        }
                    }
                }

                sw.Stop();

                _logger.LogInformation("Attendance summary for school {Name} ended for {TotalSeconds}s", item.Name,
                    Math.Round(sw.Elapsed.TotalSeconds));

                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
