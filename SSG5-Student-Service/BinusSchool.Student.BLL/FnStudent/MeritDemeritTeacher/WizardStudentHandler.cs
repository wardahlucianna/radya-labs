using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class WizardStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IRedisCache _redisCache;

        public WizardStudentHandler(IStudentDbContext EntryMeritDemetitDbContext, IMachineDateTime dateTime, IRedisCache redisCache)
        {
            _dbContext = EntryMeritDemetitDbContext;
            _dateTime = dateTime;
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<WizardStudentHandlerRequest>();

            var checkingComponent = await GetCheckingComponentRedis(param);

            var GetSemeterAy = await GetSemeterAyRedis(param);

            if (GetSemeterAy.Semester is null && GetSemeterAy.IdAcademicYear is null)
                throw new BadRequestException("Academic and semester are't found");

            var CountEntryMerit = await GetCountEntryMeritRedis(param, GetSemeterAy);

            var CountEntryDemerit = await GetCountEntryDemeritRedis(param, GetSemeterAy);

            WizardStudentHandlerResult items = new WizardStudentHandlerResult();
            items = await GetWizardStudentHandlerResultRedis(param, GetSemeterAy, CountEntryMerit, CountEntryDemerit);

            if(items != null)
            {
                items.IsPointSystem = checkingComponent != null 
                ? checkingComponent.Homeroom.Grade.MeritDemeritComponentSettings.FirstOrDefault()==null
                    ? false
                    : checkingComponent.Homeroom.Grade.MeritDemeritComponentSettings.FirstOrDefault().IsUsePointSystem
                : false;
                items.IsDemeritSystem = checkingComponent != null 
                ? checkingComponent.Homeroom.Grade.MeritDemeritComponentSettings.FirstOrDefault()==null 
                    ? false
                    : checkingComponent.Homeroom.Grade.MeritDemeritComponentSettings.FirstOrDefault().IsUseDemeritSystem
                : false;
            }

            return Request.CreateApiResult2(items as object);
        }

        private async Task<WizardStudentHandlerResult> GetWizardStudentHandlerResultRedis(WizardStudentHandlerRequest param, (int? Semester, string IdAcademicYear) getSemeterAy, int countEntryMerit, int countEntryDemerit)
        {
            var key = $"{GetType().FullName}.{nameof(GetWizardStudentHandlerResultRedis)}-{param.IdUser}-{getSemeterAy.Semester}-{getSemeterAy.IdAcademicYear}";

            var data = await _redisCache.GetAsync<WizardStudentHandlerResult>(key);

            if (data == null)
            {
                data = await
                (
                    from _homeroomStudent in _dbContext.Entity<MsHomeroomStudent>()
                    join _studentPoint in _dbContext.Entity<TrStudentPoint>() on _homeroomStudent.Id equals _studentPoint.IdHomeroomStudent into joinedStudentPoint
                    from _studentPoint in joinedStudentPoint.DefaultIfEmpty()
                    join _homeroom in _dbContext.Entity<MsHomeroom>() on _homeroomStudent.IdHomeroom equals _homeroom.Id
                    join _grade in _dbContext.Entity<MsGrade>() on _homeroom.IdGrade equals _grade.Id
                    join _level in _dbContext.Entity<MsLevel>() on _grade.IdLevel equals _level.Id
                    join _GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on _homeroom.IdGradePathwayClassRoom equals _GradePathwayClassroom.Id
                    join _Classroom in _dbContext.Entity<MsClassroom>() on _GradePathwayClassroom.IdClassroom equals _Classroom.Id
                    join _student in _dbContext.Entity<MsStudent>() on _homeroomStudent.IdStudent equals _student.Id
                    where _homeroomStudent.IdStudent == param.IdUser && _homeroom.Semester == getSemeterAy.Semester && _level.IdAcademicYear == getSemeterAy.IdAcademicYear
                    select new WizardStudentHandlerResult
                    {
                        IdHomeroomStudent = _homeroomStudent.Id,
                        IdLevel = _level.Id,
                        IdGrade = _grade.Id,
                        TotalMerit = _studentPoint != null ? _studentPoint.MeritPoint : 0,
                        TotalDemerit = _studentPoint != null ? _studentPoint.DemeritPoint : 0,
                        CountDemerit = countEntryDemerit,
                        CountMerit = countEntryMerit,
                        Semester = _homeroom.Semester,
                        Grade = _grade.Description,
                        Level = _level.Description,
                        StudentId = _homeroomStudent.IdStudent,
                        StudentName = NameUtil.GenerateFullName(_student.FirstName, _student.MiddleName, _student.LastName),
                        Homeroom = _grade.Code + _Classroom.Code
                    }

                ).FirstOrDefaultAsync(CancellationToken);

                await _redisCache.SetAsync(key, data, TimeSpan.FromMinutes(5));
            }

            return data;
        }

        private async Task<int> GetCountEntryDemeritRedis(WizardStudentHandlerRequest param, (int? Semester, string IdAcademicYear) getSemeterAy)
        {
            var data = await _dbContext.Entity<TrEntryDemeritStudent>()
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                    .Where(e => e.HomeroomStudent.Student.IdBinusian == param.IdUser
                        && e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == getSemeterAy.IdAcademicYear
                        && e.HomeroomStudent.Homeroom.Semester == getSemeterAy.Semester)
                    .CountAsync(CancellationToken);

            return data;
        }

        private async Task<int> GetCountEntryMeritRedis(WizardStudentHandlerRequest param, (int? Semester, string IdAcademicYear) getSemeterAy)
        {
            var data = await _dbContext.Entity<TrEntryMeritStudent>()
                        .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                        .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                        .Where(e => e.HomeroomStudent.Student.IdBinusian == param.IdUser
                            && e.HomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == getSemeterAy.IdAcademicYear
                            && e.HomeroomStudent.Homeroom.Semester == getSemeterAy.Semester)
                        .CountAsync(CancellationToken);

            return data;
        }

        private async Task<(int? Semester, string IdAcademicYear)> GetSemeterAyRedis(WizardStudentHandlerRequest param)
        {
            var query = await _dbContext.Entity<MsPeriod>()
                .Include(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                .Where(e => e.StartDate <= _dateTime.ServerTime.Date && e.EndDate >= _dateTime.ServerTime.Date && e.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
                .Select(e => new { Semester = e.Semester, IdAcademicYear = e.Grade.MsLevel.IdAcademicYear })
                .Distinct().FirstOrDefaultAsync(CancellationToken);

            return (query.Semester, query.IdAcademicYear);
        }

        private async Task<MsHomeroomStudent> GetCheckingComponentRedis(WizardStudentHandlerRequest param)
        {
            var key = $"{GetType().FullName}.{nameof(GetCheckingComponentRedis)}-{param.IdUser}";

            var data = await _redisCache.GetAsync<MsHomeroomStudent>(key);

            if (data == null)
            {
                data = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Student)
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.Grade)
                            .ThenInclude(x => x.MeritDemeritComponentSettings)
                    .Where(x => x.IdStudent == param.IdUser)
                    .OrderByDescending(x => x.DateIn)
                    .FirstOrDefaultAsync(CancellationToken);

                await _redisCache.SetAsync(key, data, TimeSpan.FromMinutes(5));
            }

            return data;
        }
    }
}
