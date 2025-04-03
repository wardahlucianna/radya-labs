using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.TeacherHomeroomAndSubjectTeacher;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Attendance
{
    public class TabbingAttendanceEntryHandler : FunctionsHttpSingleHandler
    {
        private readonly IMachineDateTime _dateTime;
        private readonly IAttendanceDbContext _dbContext;

        public TabbingAttendanceEntryHandler(
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<TeacherHomeroomAndSubjectTeacherRequest>(nameof(TeacherHomeroomAndSubjectTeacherRequest.IdUser), nameof(TeacherHomeroomAndSubjectTeacherRequest.IdSchool));

            var currentAcademicYear = await _dbContext.Entity<MsPeriod>()
                .Include(x => x.Grade)
                    .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
                .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
                .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
                .Select(x => new
                {
                    Id = x.Grade.Level.AcademicYear.Id,
                    Code = x.Grade.Level.AcademicYear.Code,
                    Description = x.Grade.Level.AcademicYear.Description
                }).FirstOrDefaultAsync(CancellationToken);
            TeacherHomeroomAndSubjectTeacherResult res = new TeacherHomeroomAndSubjectTeacherResult();
            if (currentAcademicYear == null)
                return Request.CreateApiResult2(res as object);
            res.HomeroomTerms = await
                (
                    from _homeroomTeacher in _dbContext.Entity<MsHomeroomTeacher>()
                    join _homeroom in _dbContext.Entity<MsHomeroom>() on _homeroomTeacher.IdHomeroom equals _homeroom.Id
                    join _grade in _dbContext.Entity<MsGrade>() on _homeroom.IdGrade equals _grade.Id
                    join _level in _dbContext.Entity<MsLevel>() on _grade.IdLevel equals _level.Id
                    join _mappingAttendance in _dbContext.Entity<MsMappingAttendance>() on _level.Id equals _mappingAttendance.IdLevel
                    where
                    _homeroomTeacher.IdBinusian == param.IdUser
                    && _homeroomTeacher.IsAttendance == true
                    && _level.IdAcademicYear == currentAcademicYear.Id
                    group _mappingAttendance by _mappingAttendance.AbsentTerms into g

                    select g.Key

                ).ToListAsync(CancellationToken);

            res.SubjectTeacherTerms = await
                (
                    from _lessonTeacher in _dbContext.Entity<MsLessonTeacher>()
                    join _lesson in _dbContext.Entity<MsLesson>() on _lessonTeacher.IdLesson equals _lesson.Id
                    join _grade in _dbContext.Entity<MsGrade>() on _lesson.IdGrade equals _grade.Id
                    join _level in _dbContext.Entity<MsLevel>() on _grade.IdLevel equals _level.Id
                    join _mappingAttendance in _dbContext.Entity<MsMappingAttendance>() on _level.Id equals _mappingAttendance.IdLevel
                    where
                    _lessonTeacher.IdUser == param.IdUser
                    && _lessonTeacher.IsAttendance == true
                    && _level.IdAcademicYear == currentAcademicYear.Id
                    group _mappingAttendance by _mappingAttendance.AbsentTerms into g

                    select g.Key

                ).ToListAsync(CancellationToken);

            res.IsNeedValidation = await
                (
                    from _homeroomTeacher in _dbContext.Entity<MsHomeroomTeacher>()
                    join _homeroom in _dbContext.Entity<MsHomeroom>() on _homeroomTeacher.IdHomeroom equals _homeroom.Id
                    join _grade in _dbContext.Entity<MsGrade>() on _homeroom.IdGrade equals _grade.Id
                    join _level in _dbContext.Entity<MsLevel>() on _grade.IdLevel equals _level.Id
                    join _mappingAttendance in _dbContext.Entity<MsMappingAttendance>() on _level.Id equals _mappingAttendance.IdLevel
                    where
                    _homeroomTeacher.IdBinusian == param.IdUser
                    && _homeroomTeacher.IsAttendance == true
                    && _level.IdAcademicYear == currentAcademicYear.Id

                    select _mappingAttendance.IsNeedValidation

                ).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(res as object);

        }
    }
}
