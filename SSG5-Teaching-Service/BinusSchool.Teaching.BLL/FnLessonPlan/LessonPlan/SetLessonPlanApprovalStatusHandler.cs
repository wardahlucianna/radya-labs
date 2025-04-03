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
using BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class SetLessonPlanApprovalStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public SetLessonPlanApprovalStatusHandler(ITeachingDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetLessonPlanApprovalStatusRequest, SetLessonPlanApprovalStatusValidator>();

            var lessonPlanApproval = _dbContext.Entity<TrLessonPlanApproval>()
                .Include(x => x.LessonPlan)
                    .ThenInclude(x => x.WeekSettingDetail)
                        .ThenInclude(x => x.WeekSetting)
                            .ThenInclude(x => x.Period)
                                .ThenInclude(x => x.Grade)
                                    .ThenInclude(x => x.Level)
                .FirstOrDefault(x => x.Id == body.IdLessonPlanApproval);
            if (lessonPlanApproval == null)
                throw new NotFoundException("Lesson plan approval not found");

            var levelApproval = _dbContext.Entity<MsLevelApproval>()
                .Include(x => x.LessonApproval)
                    .ThenInclude(x => x.LessonApprovalStates)
                .FirstOrDefault(x => x.IdLevel == lessonPlanApproval.LessonPlan.WeekSettingDetail.WeekSetting.Period.Grade.IdLevel);
            if (levelApproval == null)
                throw new NotFoundException("Level approval not found");

            if (!levelApproval.LessonApproval.LessonApprovalStates.Any(x => x.IdUser == body.IdUser && x.StateNumber == lessonPlanApproval.StateNumber) || !levelApproval.IsApproval)
                throw new UnauthorizedAccessException("You're not authorized to approve this lesson plan");

            lessonPlanApproval.IsApproved = body.IsApproved;
            lessonPlanApproval.Reason = body.Reason;

            var lessonPlan = _dbContext.Entity<TrLessonPlan>()
                .Include(x => x.LessonPlanDocuments)
                .FirstOrDefault(x => x.Id == lessonPlanApproval.IdLessonPlan);
            var lessonPlanDocument = lessonPlan.LessonPlanDocuments.Where(x => x.Id == body.IdLessonPlanDocument).FirstOrDefault();
            if (lessonPlanDocument == null)
                throw new NotFoundException("Lesson plan document not found");

            var nextLessonApprovalState = levelApproval.LessonApproval.LessonApprovalStates.Where(x => x.StateNumber > lessonPlanApproval.StateNumber).FirstOrDefault();
            var now = DateTimeUtil.ServerTime;
            if (nextLessonApprovalState == null)
            {
                if (lessonPlanDocument.IsLate.HasValue && lessonPlanDocument.IsLate.Value)
                {
                    lessonPlan.Status = body.IsApproved ? "Late" : "Need Revision";
                    lessonPlanDocument.Status = body.IsApproved ? "Late" : "Need Revision";
                }
                else
                {
                    lessonPlan.Status = body.IsApproved ? "Approved" : "Need Revision";
                    lessonPlanDocument.Status = body.IsApproved ? "Approved" : "Need Revision";
                }
            }
            else
            {

                lessonPlan.Status = body.IsApproved ? "Need Approval" : "Need Revision";
                lessonPlanDocument.Status = body.IsApproved ? "Need Approval" : "Need Revision";


                if (body.IsApproved)
                {
                    _dbContext.Entity<TrLessonPlanApproval>().Add(new TrLessonPlanApproval
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdLessonPlan = lessonPlan.Id,
                        IdUser = nextLessonApprovalState.IdUser,
                        StateNumber = nextLessonApprovalState.StateNumber,
                        LessonPlanApprovalDate = now
                    });
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await ProcessEmailAndNotif(body.IdLessonPlanApproval, lessonPlan.Status);

            return Request.CreateApiResult2();
        }
        #region emailandnotif
        private async Task ProcessEmailAndNotif(string IdLessonPlanApproval, string lessonPlanStatus)
        {
            if (!string.IsNullOrEmpty(IdLessonPlanApproval))
            {
                var lessonPlansParamNotif = await (
                    from lpa in _dbContext.Entity<TrLessonPlanApproval>()
                    join lpd in _dbContext.Entity<TrLessonPlanDocument>() on lpa.IdLessonPlan equals lpd.IdLessonPlan
                    join lp in _dbContext.Entity<TrLessonPlan>() on lpa.IdLessonPlan equals lp.Id
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
                    where lpa.Id == IdLessonPlanApproval

                    select new GetLessonPlanNotificationResult
                    {
                        IdLessonPlanApproval = lpa.Id,
                        IdLessonPlan = lp.Id,
                        IdUser = lt.IdUser,
                        IdLessonTeacher = lp.IdLessonTeacher,
                        IdSubject = subj.Id,
                        IdSubjectLevel = xsl.Id,
                        IdWeekSettingDetail = wsd.Id,
                        AcademicYear = ay.Description,
                        Level = lv.Code,
                        Grade = g.Description,
                        Term = mp.Description,
                        Subject = subj.Description,
                        SubjectLevel = xsl.Code ?? "-",
                        Periode = "Week " + wsd.WeekNumber,
                        DeadlineDate = wsd.DeadlineDate.Value.Date.ToString("dd MMM yyyy"),
                        Notes = lpa.Reason,
                        Status = lpd.Status,
                        SchoolName = sch.Name,
                        TeacherName = s.FirstName + (s.LastName != null ? (" " + s.LastName) : " "),
                        RequestDate = lpd.LessonPlanDocumentDate.Date.ToString("dd MMM yyyy")
                    }
                ).FirstOrDefaultAsync();

                IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();
                paramTemplateNotification.Add("TeacherName", lessonPlansParamNotif.TeacherName.TrimStart().TrimEnd());
                paramTemplateNotification.Add("SchoolName", lessonPlansParamNotif.SchoolName.ToUpper());
                paramTemplateNotification.Add("AcademicYear", lessonPlansParamNotif.AcademicYear);
                paramTemplateNotification.Add("Level", lessonPlansParamNotif.Level);
                paramTemplateNotification.Add("Grade", lessonPlansParamNotif.Grade);
                paramTemplateNotification.Add("Term", lessonPlansParamNotif.Term);
                paramTemplateNotification.Add("Subject", lessonPlansParamNotif.Subject);
                paramTemplateNotification.Add("SubjLevel", lessonPlansParamNotif.SubjectLevel);
                paramTemplateNotification.Add("statApproveDetail", lessonPlansParamNotif.Status);
                paramTemplateNotification.Add("Periode", lessonPlansParamNotif.Periode);
                paramTemplateNotification.Add("Notes", lessonPlansParamNotif.Notes);
                paramTemplateNotification.Add("requestdate", lessonPlansParamNotif.RequestDate);
                paramTemplateNotification.Add("IdLessonPlan", lessonPlansParamNotif.IdLessonPlan);
                paramTemplateNotification.Add("IdUser", lessonPlansParamNotif.IdUser);

                if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                {
                    if (lessonPlanStatus == "Approved")
                    {
                        paramTemplateNotification.Add("statusApprove", "Approved");
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "LP3")
                        {
                            IdRecipients = new[] { lessonPlansParamNotif.IdUser },
                            KeyValues = paramTemplateNotification
                        });
                        collector.Add(message);
                    }
                    else if (lessonPlanStatus == "Need Revision")
                    {
                        paramTemplateNotification.Add("statusApprove", "Declined");
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "LP4")
                        {
                            IdRecipients = new[] { lessonPlansParamNotif.IdUser },
                            KeyValues = paramTemplateNotification
                        });
                        collector.Add(message);
                    }
                }
            }
        }
        #endregion

    }
}
