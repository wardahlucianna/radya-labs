using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Constants;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanSummaryDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public LessonPlanSummaryDetailHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLessonPlanSummaryDetailRequest>(
                nameof(GetLessonPlanSummaryDetailRequest.IdWeekSettingDetail),
                nameof(GetLessonPlanSummaryDetailRequest.IdSubject)
            );

            var listLessonByUser = await LessonPlanListSummaryHandler.GetLessonByUser(_dbContext, CancellationToken, param.IdUser, param.IdAcademicYear, param.PositionCode);
            //var weekSettingDetail = _dbContext.Entity<MsWeekSettingDetail>()
            //    .Include(x => x.WeekSetting)
            //        .ThenInclude(x => x.Period)
            //            .ThenInclude(x => x.Grade)
            //                .ThenInclude(x => x.Level)
            //    .Include(x => x.LessonPlans)
            //        .ThenInclude(x => x.LessonTeacher)
            //            .ThenInclude(x => x.Staff)
            //    .Include(x => x.LessonPlans)
            //        .ThenInclude(x => x.SubjectMappingSubjectLevel)
            //            .ThenInclude(x => x.Subject)
            //    .Include(x => x.LessonPlans)
            //        .ThenInclude(x => x.LessonTeacher)
            //            .ThenInclude(x => x.Lesson)
            //    .FirstOrDefault(x => x.Id == param.IdWeekSettingDetail);
            //if (weekSettingDetail == null)
            //    throw new NotFoundException("Week setting detail not found");

            //var userNonST = _dbContext.Entity<TrNonTeachingLoad>()
            //.Include(x => x)
            //    .Include(x => x.MsNonTeachingLoad)
            //        .ThenInclude(x => x.TeacherPosition)
            //            .ThenInclude(x => x.Position)
            //    .Where(x => x.IdUser == AuthInfo.UserId && (x.MsNonTeachingLoad.TeacherPosition.Position.Code == "P" || x.MsNonTeachingLoad.TeacherPosition.Position.Code == "VP" || x.MsNonTeachingLoad.TeacherPosition.Position.Code == "SH" || x.MsNonTeachingLoad.TeacherPosition.Position.Code == "LH"))
            //    .ToList();
            //bool isApproval = false;
            //if (userNonST.Count() == 0)
            //{
            //    isApproval = await _dbContext.Entity<MsLessonApprovalState>().Where(x => x.IdUser == AuthInfo.UserId).AnyAsync(CancellationToken);
            //}
            //else
            //{
            //    isApproval = true;
            //}

            var listIdlesson = listLessonByUser.Select(e => e.IdLesson).Distinct().ToList();

            var queryLessonPlanByWeekSettingSummary =
                    from _lessonPlan in _dbContext.Entity<TrLessonPlan>()
                    join _lessonTeacher in _dbContext.Entity<MsLessonTeacher>() on _lessonPlan.IdLessonTeacher equals _lessonTeacher.Id
                    join _lesson in _dbContext.Entity<MsLesson>() on _lessonTeacher.IdLesson equals _lesson.Id
                    join _subject in _dbContext.Entity<MsSubject>() on _lesson.IdSubject equals _subject.Id
                    join _staff in _dbContext.Entity<MsStaff>() on _lessonTeacher.IdUser equals _staff.IdBinusian
                    join _weekSettingDetail in _dbContext.Entity<MsWeekSettingDetail>() on _lessonPlan.IdWeekSettingDetail equals _weekSettingDetail.Id
                    join _mappingSubjectLevel in _dbContext.Entity<MsSubjectMappingSubjectLevel>() on _lessonPlan.IdSubjectMappingSubjectLevel equals _mappingSubjectLevel.Id into joinedMappingSubjectLevel
                    from _mappingSubjectLevel in joinedMappingSubjectLevel.DefaultIfEmpty()
                    join _subjectLevel in _dbContext.Entity<MsSubjectLevel>() on _mappingSubjectLevel.IdSubjectLevel equals _subjectLevel.Id into joinedSubjectLevel
                    from _subjectLevel in joinedSubjectLevel.DefaultIfEmpty()
                    where
                    _subject.Id == param.IdSubject && _lessonPlan.IdWeekSettingDetail == param.IdWeekSettingDetail && listIdlesson.Contains(_lessonTeacher.IdLesson)
                    select new
                    {
                        Id = _lessonPlan.Id,
                        IdLesson = _lessonTeacher.IdLesson,
                        IdUser = _lessonTeacher.IdUser,
                        Fullname = $"{_staff.FirstName} {_staff.LastName}",
                        ClassId = $"{_lesson.ClassIdGenerated}",
                        Status = _lessonPlan.Status,
                        DeadlineDate = _weekSettingDetail.DeadlineDate,
                        UploadedDate = _lessonPlan.DateUp,
                        SubjectLevel = _subjectLevel.Description
                    };

            if (param.PositionCode == PositionConstant.SubjectTeacher)
                queryLessonPlanByWeekSettingSummary = queryLessonPlanByWeekSettingSummary.Where(e => e.IdUser == param.IdUser);

            var dataLessonPlanByWeekSettingSummary = await queryLessonPlanByWeekSettingSummary.ToListAsync(CancellationToken);

            var result = new GetLessonPlanSummaryDetailResult
            {
                Uploaded = dataLessonPlanByWeekSettingSummary.Where(x => (x.Status == "Approved" || x.Status == "Created" || x.Status == "Late")).Select(x => new LessonPlanSummaryDetailUploaded
                {
                    TeacherName = x.Fullname,
                    UploadDate = x.UploadedDate,
                    DeadlineDate = x.DeadlineDate,
                    Status = x.Status == "Approved"
                                ? x.Status
                                : x.UploadedDate>x.DeadlineDate?"Late": x.Status,
                    IdLessonPlan = x.Id,
                    IdLesson = x.IdLesson,
                    IdUser = x.IdUser,
                    IdClass = $"{x.ClassId}" + (x.SubjectLevel != null ? $" - {x.SubjectLevel}" : "")
                }).ToList(),
                NotUploaded = dataLessonPlanByWeekSettingSummary.Where(x => x.Status != "Approved" && x.Status != "Created" && x.Status != "Late").Select(x => new LessonPlanSummaryDetailNotUploaded
                {
                    TeacherName = x.Fullname,
                    DeadlineDate = x.DeadlineDate,
                    Status = x.Status,
                    IdLessonPlan = x.Id,
                    IdLesson = x.IdLesson,
                    IdUser = x.IdUser,
                    IdClass = $"{x.ClassId}" + (x.SubjectLevel != null ? $" - {x.SubjectLevel}" : "")
                }).ToList()
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
