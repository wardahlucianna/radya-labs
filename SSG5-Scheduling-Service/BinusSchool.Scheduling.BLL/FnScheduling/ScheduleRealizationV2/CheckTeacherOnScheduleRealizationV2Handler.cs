using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class CheckTeacherOnScheduleRealizationV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public CheckTeacherOnScheduleRealizationV2Handler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        private static readonly string[] _columns = { "Date", "Session" };

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<CheckTeacherOnScheduleRealizationV2Request>(nameof(CheckTeacherOnScheduleRealizationV2Request.IdAcademicYear),
                                                                                           nameof(CheckTeacherOnScheduleRealizationV2Request.IdUser));
            var IdUserLessonTeacher = param.IdUser;

            var dataAY = await _dbContext.Entity<MsAcademicYear>().Where(x => x.Id == param.IdAcademicYear).FirstOrDefaultAsync(CancellationToken);

            var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization2>(x => x.IdBinusianSubtitute == param.IdUser && x.IsCancel == false && (x.Status == "Subtituted" || x.Status == "Venue Change") && x.IdAcademicYear == param.IdAcademicYear);

            var getDataSubtituteTeacher = await _dbContext.Entity<TrScheduleRealization2>()
                                 .Where(predicateSubtitute).FirstOrDefaultAsync(CancellationToken);

            if (getDataSubtituteTeacher != null)
            {
                IdUserLessonTeacher = getDataSubtituteTeacher.IdBinusian;
            }

            
            var dataHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                        .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                        .Where(x => x.IdBinusian == param.IdUser  && x.IsAttendance
                                                    && x.Homeroom.Grade.Level.IdAcademicYear==param.IdAcademicYear)
                                        .FirstOrDefaultAsync(CancellationToken);

            var listIdLevelHomeroomTeacher = "";
            var absentTermHomeroomTeacher = false;

            if (dataHomeroomTeacher != null)
            {
                listIdLevelHomeroomTeacher = dataHomeroomTeacher.Homeroom.Grade.IdLevel;

                var getMappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                                .Where(x => listIdLevelHomeroomTeacher.Contains(x.IdLevel))
                                                .FirstOrDefaultAsync(CancellationToken);

                if (getMappingAttendance.AbsentTerms == AbsentTerm.Session)
                    absentTermHomeroomTeacher = getMappingAttendance.IsNeedValidation;
                else
                    absentTermHomeroomTeacher = true;
            }

            var MappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                               .Where(x => x.Level.IdAcademicYear == param.IdAcademicYear && x.AbsentTerms == AbsentTerm.Session)
                               .ToListAsync(CancellationToken);

            var listIdLevel = MappingAttendance.Select(e => e.IdLevel).ToList();

            var dataLessonTeacher = _dbContext.Entity<MsLessonTeacher>()
                                     .Include(x => x.Lesson).ThenInclude(x => x.Grade)
                                    .Where(x => x.IdUser == IdUserLessonTeacher && listIdLevel.Contains(x.Lesson.Grade.IdLevel))
                                    .FirstOrDefault();

            var listIdLevelLessonTeacher = "";
            var absentTermLessonTeacher = false;
            if (dataLessonTeacher != null)
            {
                listIdLevelLessonTeacher = dataLessonTeacher.Lesson.Grade.IdLevel;
                absentTermLessonTeacher = MappingAttendance
                                .Where(x => listIdLevelLessonTeacher.Contains(x.IdLevel) && x.AbsentTerms == AbsentTerm.Session)
                               .Any();
            }

            var data = new CheckTeacherOnScheduleRealizationV2Result
            {
                AcademicYear = new CodeWithIdVm(dataAY.Id, dataAY.Code, dataAY.Description),
                HaveDailyAttendance = absentTermHomeroomTeacher,
                HaveSessionAttendance = absentTermLessonTeacher
            };

            return Request.CreateApiResult2(data as object);
        }
    }
}
