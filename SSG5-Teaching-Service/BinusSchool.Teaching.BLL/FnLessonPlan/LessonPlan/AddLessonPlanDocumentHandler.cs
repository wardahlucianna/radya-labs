using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class AddLessonPlanDocumentHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public AddLessonPlanDocumentHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddLessonPlanDocumentRequest, AddLessonPlanDocumentValidator>();

            var queryLessonSubjectByUser = _dbContext.Entity<MsLessonTeacher>()
                                        .Include(e => e.Lesson)
                                        .Where(e => e.IdUser == body.IdUser && e.IsLessonPlan);

            var predicate = PredicateBuilder.Create<TrLessonPlan>(x => x.LessonTeacher.Lesson.IdGrade == body.IdGrade
                                                                    && x.WeekSettingDetail.WeekSetting.IdPeriod == body.IdPeriod
                                                                    && x.WeekSettingDetail.WeekNumber == body.WeekNumber
                                                                    && x.LessonTeacher.Lesson.Subject.IsNeedLessonPlan == true
                                                                  );

            var listSubject = body.Subject.Select(e => e.IdSubject).ToList();

            if (body.IsAllSubject == false)
            {

                var checkSubject = _dbContext.Entity<MsSubject>()
                    .Include(x => x.Lessons)
                    .Where(x => listSubject.Contains(x.Id)).Select(e => e.Id).ToList();

                if (checkSubject == null && checkSubject.Count == 0)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SubjectId"], "Id", listSubject));

                queryLessonSubjectByUser = queryLessonSubjectByUser.Where(e => listSubject.Contains(e.Lesson.IdSubject));
            }

            var listIdLessonByUser = await queryLessonSubjectByUser
                                                .Select(e => e.IdLesson)
                                                .ToListAsync(CancellationToken);

            predicate = predicate.And(e => listIdLessonByUser.Contains(e.LessonTeacher.IdLesson));

            var lessonPlan = _dbContext.Entity<TrLessonPlan>()
                .Include(x => x.SubjectMappingSubjectLevel)
                .Include(x => x.LessonTeacher)
                    .ThenInclude(x => x.Lesson)
                        .ThenInclude(x => x.Subject)
                            .ThenInclude(x => x.Grade)
                                .ThenInclude(x => x.Level)
                .Where(predicate)
                .ToList();

            if (lessonPlan == null)
                throw new NotFoundException("Lesson plan not found");

            foreach (var listLessonPlan in lessonPlan)
            {
                var subjectMappingSubjectLevel = listLessonPlan.SubjectMappingSubjectLevel;

                if (body.IsAllSubject == false)
                {
                    var querySubjectExsis = body.Subject
                                    .Where(e => e.IdSubject == listLessonPlan.LessonTeacher.Lesson.IdSubject);

                    if (subjectMappingSubjectLevel != null)
                    {
                        querySubjectExsis = body.Subject
                                        .Where(e => e.IdSubjectMappingSubjectLevel == subjectMappingSubjectLevel.Id);
                    }

                    var bodySubjectExsis = querySubjectExsis.Any();

                    if (!bodySubjectExsis)
                        continue;
                }

                var levelApproval = _dbContext.Entity<MsLevelApproval>()
                .Include(x => x.LessonApproval)
                    .ThenInclude(x => x.LessonApprovalStates)
                .FirstOrDefault(x => x.IdLevel == listLessonPlan.LessonTeacher.Lesson.Subject.Grade.IdLevel && x.IsApproval);

                //if (levelApproval == null)
                //    throw new NotFoundException("Level approval not found");

                string status;
                var idUserApproval = "";
                var now = DateTimeUtil.ServerTime;
                if (levelApproval != null)
                {
                    if (!levelApproval.IsApproval)
                        continue;

                    status = "Need Approval";
                    listLessonPlan.Status = "Need Approval";
                    idUserApproval = levelApproval.LessonApproval.LessonApprovalStates.FirstOrDefault().IdUser;
                    _dbContext.Entity<TrLessonPlanApproval>().Add(new TrLessonPlanApproval
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdUser = idUserApproval,
                        IdLessonPlan = listLessonPlan.Id,
                        StateNumber = 1,
                        LessonPlanApprovalDate = now
                    });
                }
                else
                {
                    status = "Created";
                    listLessonPlan.Status = "Created";
                }

                var checkLessonPlanDocumentOld = await _dbContext.Entity<TrLessonPlanDocument>()
                                                .Where(x => x.IdLessonPlan == listLessonPlan.Id)
                                                .ToListAsync(CancellationToken);

                checkLessonPlanDocumentOld.ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrLessonPlanDocument>().UpdateRange(checkLessonPlanDocumentOld);

                var IdLessonPlanDocument = Guid.NewGuid().ToString();
                _dbContext.Entity<TrLessonPlanDocument>().Add(new TrLessonPlanDocument
                {
                    Id = IdLessonPlanDocument,
                    PathFile = body.PathFile,
                    Filename = body.Filename,
                    IdLessonPlan = listLessonPlan.Id,
                    Status = status,
                    LessonPlanDocumentDate = now
                });
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            // await ProcessEmailAndNotif(idUserApproval, body.IdLessonPlan);

            return Request.CreateApiResult2();
        }

        #region emailandnotif
        private async Task ProcessEmailAndNotif(string IdUserApproval, string IdLessonPlan)
        {
            if (!string.IsNullOrEmpty(IdUserApproval))
            {
                var lessonPlansParamNotif = await (
                    from lpa in _dbContext.Entity<TrLessonPlanApproval>()
                    join lp in _dbContext.Entity<TrLessonPlan>() on lpa.IdLessonPlan equals lp.Id
                    join lpd in _dbContext.Entity<TrLessonPlanDocument>() on lp.Id equals lpd.IdLessonPlan
                    join lt in _dbContext.Entity<MsLessonTeacher>() on lp.IdLessonTeacher equals lt.Id
                    join s in _dbContext.Entity<MsStaff>() on lt.IdUser equals s.IdBinusian
                    join l in _dbContext.Entity<MsLesson>() on lt.IdLesson equals l.Id
                    join subj in _dbContext.Entity<MsSubject>() on l.IdSubject equals subj.Id
                    join smsl in _dbContext.Entity<MsSubjectMappingSubjectLevel>() on lp.IdSubjectMappingSubjectLevel equals smsl.Id into lsmsl
                    from xsmsl in lsmsl.DefaultIfEmpty()
                    join sl in _dbContext.Entity<MsSubjectLevel>() on xsmsl.IdSubjectLevel equals sl.Id into lsl
                    from xsl in lsl.DefaultIfEmpty()
                    join g in _dbContext.Entity<MsGrade>() on subj.IdGrade equals g.Id
                    join lv in _dbContext.Entity<MsLevel>() on g.IdLevel equals lv.Id
                    join ay in _dbContext.Entity<MsAcademicYear>() on lv.IdAcademicYear equals ay.Id
                    join sch in _dbContext.Entity<MsSchool>() on ay.IdSchool equals sch.Id
                    join wsd in _dbContext.Entity<MsWeekSettingDetail>() on lp.IdWeekSettingDetail equals wsd.Id
                    join ws in _dbContext.Entity<MsWeekSetting>() on wsd.IdWeekSetting equals ws.Id
                    join mp in _dbContext.Entity<MsPeriod>() on ws.IdPeriod equals mp.Id
                    join usr in _dbContext.Entity<MsUser>() on lpa.IdUser equals usr.Id
                    where lpa.IdUser == IdUserApproval && lpa.IsApproved == false && lpa.Reason == null && lp.Id == IdLessonPlan

                    select new GetLessonPlanNotificationResult
                    {
                        IdLessonPlanApproval = lpa.Id,
                        IdLessonPlan = lp.Id,
                        IdLessonPlanDocument = lpd.Id,
                        IdSubject = subj.Id,
                        IdSubjectLevel = xsl.Id,
                        IdWeekSettingDetail = wsd.Id,
                        ApprovalName = usr.DisplayName,
                        AcademicYear = ay.Description,
                        Level = lv.Code,
                        Grade = g.Description,
                        Term = mp.Description,
                        Subject = subj.Description,
                        SubjectLevel = xsl.Code ?? "-",
                        Periode = "Week " + wsd.WeekNumber,
                        DeadlineDate = wsd.DeadlineDate.Value.Date.ToString("dd MMM yyyy"),
                        IdTeacher = s.IdBinusian,
                        TeacherName = s.FirstName + (s.LastName != null ? (" " + s.LastName) : " "),
                        SchoolName = sch.Name
                    }
                ).FirstOrDefaultAsync();

                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();
                paramTemplateNotification.Add("ApproverName", lessonPlansParamNotif.ApprovalName.TrimStart().TrimEnd());
                paramTemplateNotification.Add("SchoolName", lessonPlansParamNotif.SchoolName.ToUpper());
                paramTemplateNotification.Add("AcademicYear", lessonPlansParamNotif.AcademicYear);
                paramTemplateNotification.Add("Level", lessonPlansParamNotif.Level);
                paramTemplateNotification.Add("Grade", lessonPlansParamNotif.Grade);
                paramTemplateNotification.Add("Term", lessonPlansParamNotif.Term);
                paramTemplateNotification.Add("Subject", lessonPlansParamNotif.Subject);
                paramTemplateNotification.Add("SubjLevel", lessonPlansParamNotif.SubjectLevel);
                paramTemplateNotification.Add("TeachersName", lessonPlansParamNotif.TeacherName.TrimStart().TrimEnd());
                paramTemplateNotification.Add("Periode", lessonPlansParamNotif.Periode);
                paramTemplateNotification.Add("Deadline", lessonPlansParamNotif.DeadlineDate);
                paramTemplateNotification.Add("IdLessonPlan", lessonPlansParamNotif.IdLessonPlan);
                paramTemplateNotification.Add("IdLessonPlanDocument", lessonPlansParamNotif.IdLessonPlanDocument);

                if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "LP2")
                    {
                        IdRecipients = new[] { IdUserApproval },
                        KeyValues = paramTemplateNotification
                    });
                    collector.Add(message);
                }
            }
        }
        #endregion

    }
}
