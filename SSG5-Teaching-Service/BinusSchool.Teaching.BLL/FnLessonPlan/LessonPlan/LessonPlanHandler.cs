using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Api.Scheduling.FnSchedule;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IRolePosition _serviceRolePosition;

        public LessonPlanHandler(ITeachingDbContext dbContext, IMachineDateTime dateTime, IRolePosition serviceRolePosition)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _serviceRolePosition = serviceRolePosition;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLessonPlanRequest>(nameof(GetLessonPlanRequest.IdUser));

            #region akses
            List<string> listCodePosition = new List<string>()
                    {
                        PositionConstant.SubjectTeacher,
                    };

            var listIdTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                    .Where(e => listCodePosition.Contains(e.Position.Code))
                    .Select(e => e.Id)
                    .ToListAsync(CancellationToken);

            var paramPositionByUser = new GetSubjectByUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdUser = param.IdUser,
                ListIdTeacherPositions = listIdTeacherPosition,
                IsLessonPlan = true
            };
            var getApiSubjectByUser = await _serviceRolePosition.GetSubjectByUser(paramPositionByUser);
            var getSubjectByUser = getApiSubjectByUser.IsSuccess ? getApiSubjectByUser.Payload : null;
            var getIdLessonByUser = new List<string>();
            if (getSubjectByUser != null)
            {
                getIdLessonByUser = getSubjectByUser.Select(e => e.Lesson.Id).Distinct().ToList(); ;
            }
            #endregion

            var _trLessonPlan = _dbContext.Entity<TrLessonPlan>()
                    .Where(x => x.LessonTeacher.Lesson.IdAcademicYear == param.IdAcademicYear)
                    .Include(e => e.LessonPlanDocuments)
                    .Include(x => x.LessonTeacher).ThenInclude(x => x.Lesson);

            var data = (
                from ms in _dbContext.Entity<MsSubject>()
                join ml in _dbContext.Entity<MsLesson>() on ms.Id equals ml.IdSubject
                join mlt in _dbContext.Entity<MsLessonTeacher>() on ml.Id equals mlt.IdLesson
                join tlp in _dbContext.Entity<TrLessonPlan>() on mlt.Id equals tlp.IdLessonTeacher
                join mg in _dbContext.Entity<MsGrade>() on ms.IdGrade equals mg.Id
                join ml2 in _dbContext.Entity<MsLevel>() on mg.IdLevel equals ml2.Id
                join may in _dbContext.Entity<MsAcademicYear>() on ml2.IdAcademicYear equals may.Id
                join ms2 in _dbContext.Entity<MsStaff>() on mlt.IdUser equals ms2.IdBinusian
                join mp in _dbContext.Entity<MsPeriod>() on mg.Id equals mp.IdGrade
                join mwsd in _dbContext.Entity<MsWeekSettingDetail>() on tlp.IdWeekSettingDetail equals mwsd.Id
                join msmsl in _dbContext.Entity<MsSubjectMappingSubjectLevel>() on tlp.IdSubjectMappingSubjectLevel equals msmsl.Id into leftMsml
                from subMsml in leftMsml.DefaultIfEmpty()
                join msl in _dbContext.Entity<MsSubjectLevel>() on subMsml.IdSubjectLevel equals msl.Id into leftMsl
                from subMsl in leftMsl.DefaultIfEmpty()
                join mws in _dbContext.Entity<MsWeekSetting>() on new { A = mp.Id, B = mwsd.IdWeekSetting } equals new { A = mws.IdPeriod, B = mws.Id }
                where mwsd.DeadlineDate != null && ms.IsNeedLessonPlan == true && getIdLessonByUser.Contains(ml.Id) //&& tlp.Id == "f58c0ab0-d930-49f4-bc1d-28d3e9807ed9"
                orderby may.Code descending, ml2.Code descending, mg.Description descending, mp.Code descending, ms.Description descending, mwsd.WeekNumber descending
                select new
                {
                    // IdLessonPlan = tlp.Id,
                    IdPeriod = mp.Id,
                    IdSubject = ms.Id,
                    IdSubjectLevel = subMsl.Id,
                    IdWeekSettingDetail = mwsd.Id,
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = may.Id,
                        Code = may.Code
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = ml2.Id,
                        Code = ml2.Code
                    },
                    Grade = new ItemValueVm
                    {
                        Id = mg.Id,
                        Description = mg.Description
                    },
                    Period = new CodeWithIdVm
                    {
                        Id = mp.Id,
                        Code = mp.Code
                    },
                    Subject = new ItemValueVm
                    {
                        Id = ms.Id,
                        Description = ms.Description
                    },
                    SubjectLevel = subMsl != null ? subMsl.Code : "-",
                    Periode = "Week " + mwsd.WeekNumber,
                    WeekNumber = mwsd.WeekNumber,
                    DeadlineDate = mwsd.DeadlineDate,
                    StatusWeekSetting = mwsd.Status,
                    IdLesson = mlt.IdLesson,
                    IdSubjectMappingSubjectLevel = tlp.IdSubjectMappingSubjectLevel
                }
            )
            .Distinct()
            .ToList();

            if (param.IdAcademicYear != null)
                data = data.Where(x => x.AcademicYear.Id == param.IdAcademicYear).ToList();

            if (param.IdLevel != null)
                data = data.Where(x => x.Level.Id == param.IdLevel).ToList();

            if (param.IdGrade != null)
                data = data.Where(x => x.Grade.Id == param.IdGrade).ToList();

            if (param.IdPeriod != null)
                data = data.Where(x => x.Period.Id == param.IdPeriod).ToList();

            if (param.IdSubject != null)
                data = data.Where(x => x.Subject.Id == param.IdSubject).ToList();

            //var count = param.CanCountWithoutFetchDb(data.ToList().Count) 
            //    ? data.ToList().Count
            //    : data.Select(x => x.AcademicYear).Count();

            var lessonPlanQuery = data.Select(x => new
            {
                IdLessonPlan = _trLessonPlan
                                .Where(e => e.IdWeekSettingDetail == x.IdWeekSettingDetail
                                        && e.WeekSettingDetail.WeekNumber == x.WeekNumber
                                        && e.LessonTeacher.IdLesson == x.IdLesson
                                        && e.LessonTeacher.Lesson.IdSubject == x.IdSubject
                                        && e.IdSubjectMappingSubjectLevel == x.IdSubjectMappingSubjectLevel)
                                .FirstOrDefault().Id,
                IdPeriod = x.IdPeriod,
                IdSubject = x.IdSubject,
                IdSubjectLevel = x.IdSubjectLevel,
                IdWeekSettingDetail = x.IdWeekSettingDetail,
                AcademicYear = x.AcademicYear.Code,
                Level = x.Level.Code,
                Grade = x.Grade.Description,
                Term = x.Period.Code,
                Subject = x.Subject.Description,
                SubjectLevel = x.SubjectLevel,
                Periode = x.Periode,
                DeadlineDate = x.DeadlineDate,
                //LastUpdate = _trLessonPlan.Where(e => e.IdWeekSettingDetail == x.IdWeekSettingDetail && e.WeekSettingDetail.WeekNumber == x.WeekNumber && e.LessonTeacher.IdLesson == x.IdLesson && e.LessonTeacher.Lesson.IdSubject == x.IdSubject).OrderBy(r=>r.DateUp).FirstOrDefault().DateUp,
                LastUpdate = _trLessonPlan
                                .Where(e => e.IdWeekSettingDetail == x.IdWeekSettingDetail
                                        && e.WeekSettingDetail.WeekNumber == x.WeekNumber
                                        && e.LessonTeacher.IdLesson == x.IdLesson
                                        && e.LessonTeacher.Lesson.IdSubject == x.IdSubject
                                        && e.IdSubjectMappingSubjectLevel == x.IdSubjectMappingSubjectLevel)
                                .SelectMany(r => r.LessonPlanDocuments.Select(d => d.DateIn))
                                .FirstOrDefault(),
                Status = x.StatusWeekSetting == true
                            ? _trLessonPlan
                                .Where(e => e.IdWeekSettingDetail == x.IdWeekSettingDetail
                                        && e.WeekSettingDetail.WeekNumber == x.WeekNumber
                                        && e.LessonTeacher.IdLesson == x.IdLesson
                                        && e.LessonTeacher.Lesson.IdSubject == x.IdSubject
                                        && e.IdSubjectMappingSubjectLevel == x.IdSubjectMappingSubjectLevel)
                                .FirstOrDefault().Status
                            : _trLessonPlan
                                .Where(e => e.IdWeekSettingDetail == x.IdWeekSettingDetail
                                        && e.WeekSettingDetail.WeekNumber == x.WeekNumber
                                        && e.LessonTeacher.IdLesson == x.IdLesson
                                        && e.IdSubjectMappingSubjectLevel == x.IdSubjectMappingSubjectLevel)
                                .FirstOrDefault().Status == "Need Approval"
                                    ? "Inactive"
                                    : _trLessonPlan
                                        .Where(e => e.IdWeekSettingDetail == x.IdWeekSettingDetail
                                            && e.WeekSettingDetail.WeekNumber == x.WeekNumber
                                            && e.LessonTeacher.IdLesson == x.IdLesson
                                            && e.IdSubjectMappingSubjectLevel == x.IdSubjectMappingSubjectLevel).FirstOrDefault().Status,

                CanUpload = x.StatusWeekSetting == true ? true : false,
                IdLesson = x.IdLesson
            }).ToList();

            if (param.Status != null)
                lessonPlanQuery = lessonPlanQuery.Where(x => x.Status == param.Status).ToList();

            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                lessonPlanQuery = lessonPlanQuery.Where(x
                    => x.AcademicYear.ToLower().Contains(param.Search.ToLower())
                    || x.Level.ToLower().Contains(param.Search.ToLower())
                    || x.Grade.ToLower().Contains(param.Search.ToLower())
                    || x.Term.ToLower().Contains(param.Search.ToLower())
                    || x.Subject.ToLower().Contains(param.Search.ToLower())
                    || x.SubjectLevel.ToLower().Contains(param.Search.ToLower())
                    || x.Periode.ToLower().Contains(param.Search.ToLower())
                    || x.Status.ToLower().Contains(param.Search.ToLower())
                ).ToList();
            }

            switch (param.OrderBy)
            {
                case "academicYear":
                    lessonPlanQuery = param.OrderType == OrderType.Desc
                        ? lessonPlanQuery.OrderByDescending(x => x.AcademicYear).ToList()
                        : lessonPlanQuery.OrderBy(x => x.AcademicYear).ToList();
                    break;
                case "level":
                    lessonPlanQuery = param.OrderType == OrderType.Desc
                        ? lessonPlanQuery.OrderByDescending(x => x.Level).ToList()
                        : lessonPlanQuery.OrderBy(x => x.Level).ToList();
                    break;
                case "grade":
                    lessonPlanQuery = param.OrderType == OrderType.Desc
                        ? lessonPlanQuery.OrderByDescending(x => x.Grade).ToList()
                        : lessonPlanQuery.OrderBy(x => x.Grade).ToList();
                    break;
                case "term":
                    lessonPlanQuery = param.OrderType == OrderType.Desc
                        ? lessonPlanQuery.OrderByDescending(x => x.Term).ToList()
                        : lessonPlanQuery.OrderBy(x => x.Term).ToList();
                    break;
                case "subject":
                    lessonPlanQuery = param.OrderType == OrderType.Desc
                        ? lessonPlanQuery.OrderByDescending(x => x.Subject).ToList()
                        : lessonPlanQuery.OrderBy(x => x.Subject).ToList();
                    break;
                case "subjectLevel":
                    lessonPlanQuery = param.OrderType == OrderType.Desc
                        ? lessonPlanQuery.OrderByDescending(x => x.SubjectLevel).ToList()
                        : lessonPlanQuery.OrderBy(x => x.SubjectLevel).ToList();
                    break;
                case "periode":
                    lessonPlanQuery = param.OrderType == OrderType.Desc
                        ? lessonPlanQuery.OrderByDescending(x => x.Periode).ToList()
                        : lessonPlanQuery.OrderBy(x => x.Periode).ToList();
                    break;
                case "deadlineDate":
                    lessonPlanQuery = param.OrderType == OrderType.Desc
                        ? lessonPlanQuery.OrderByDescending(x => x.DeadlineDate).ToList()
                        : lessonPlanQuery.OrderBy(x => x.DeadlineDate).ToList();
                    break;
                case "lastUpdate":
                    lessonPlanQuery = param.OrderType == OrderType.Desc
                        ? lessonPlanQuery.OrderByDescending(x => x.LastUpdate).ToList()
                        : lessonPlanQuery.OrderBy(x => x.LastUpdate).ToList();
                    break;
                case "status":
                    lessonPlanQuery = param.OrderType == OrderType.Desc
                        ? lessonPlanQuery.OrderByDescending(x => x.Status).ToList()
                        : lessonPlanQuery.OrderBy(x => x.Status).ToList();
                    break;
            };

            var lessonPlansData = lessonPlanQuery
            .SetPagination(param)
            .Select(x => new GetLessonPlanResult
            {
                IdLessonPlan = x.IdLessonPlan,
                IdPeriod = x.IdPeriod,
                IdSubject = x.IdSubject,
                IdSubjectLevel = x.IdSubjectLevel,
                IdWeekSettingDetail = x.IdWeekSettingDetail,
                AcademicYear = x.AcademicYear,
                Level = x.Level,
                Grade = x.Grade,
                Term = x.Term,
                Subject = x.Subject,
                SubjectLevel = x.SubjectLevel,
                Periode = x.Periode,
                DeadlineDate = x.DeadlineDate,
                LastUpdate = x.LastUpdate,
                Status = x.Status== "Approved" 
                            ? x.Status
                            : x.LastUpdate > x.DeadlineDate 
                                ? "Late" : x.Status,
                CanUpload = x.CanUpload,
                IdLesson = x.IdLesson
            });

            var lessonPlans = lessonPlansData.ToList();

            var count = param.CanCountWithoutFetchDb(lessonPlans.Count)
                            ? lessonPlans.Count
                            : data.Select(x => x.AcademicYear).Count();

            return Request.CreateApiResult2(lessonPlans as object, param.CreatePaginationProperty(count));
        }
    }
}
