using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Model.Scheduling.FnSchedule.TeacherHomeroomAndSubjectTeacher;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.TeacherHomeroomAndSubjectTeacher
{
    public class TeacherHomeroomAndSubjectTeacherHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public TeacherHomeroomAndSubjectTeacherHandler(ISchedulingDbContext dbContext,IApiService<IPeriod> periodeService, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<TeacherHomeroomAndSubjectTeacherRequest>(nameof(TeacherHomeroomAndSubjectTeacherRequest.IdUser),nameof(TeacherHomeroomAndSubjectTeacherRequest.IdSchool));
            
            var currentAcademicYear = await _dbContext.Entity<MsPeriod>()
                .Include(x => x.Grade)
                    .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
                .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
                .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
                .Select(x => new CurrentAcademicYearResult
                {
                    Id = x.Grade.Level.AcademicYear.Id,
                    Code = x.Grade.Level.AcademicYear.Code,
                    Description = x.Grade.Level.AcademicYear.Description
                }).FirstOrDefaultAsync(CancellationToken);
            List<TeacherHomeroomAndSubjectTeacherResult> res = new List<TeacherHomeroomAndSubjectTeacherResult>();
            if (currentAcademicYear == null)
                return Request.CreateApiResult2(res as object);
            var dataHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
            .Include(x=>x.Homeroom)
            .Where(x=>x.IdBinusian == param.IdUser)
            .Where(x=>x.Homeroom.IdAcademicYear == currentAcademicYear.Id)
            .Select(x=>new TeacherHomeroomAndSubjectTeacherResult {
                Id = x.IdTeacherPosition,
                Code = PositionConstant.ClassAdvisor,
                Description = PositionConstant.ClassAdvisor
            }).FirstOrDefaultAsync(CancellationToken);

            var dataLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
            .Include(x=>x.Lesson)
            .Where(x=>x.IdUser == param.IdUser)
            .Where(x=>x.Lesson.IdAcademicYear == currentAcademicYear.Id)
            .Select(x=>new TeacherHomeroomAndSubjectTeacherResult {
                Id = PositionConstant.SubjectTeacher,
                Code = PositionConstant.SubjectTeacher,
                Description = PositionConstant.SubjectTeacher
            }).FirstOrDefaultAsync(CancellationToken);

           

            if (dataHomeroomTeacher != null)
                res.Add(dataHomeroomTeacher);
            if (dataLessonTeacher != null)
                res.Add(dataLessonTeacher);

            return Request.CreateApiResult2(res as object);
        }
    }
}
