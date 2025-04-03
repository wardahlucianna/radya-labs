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
using BinusSchool.Common.Utils;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class GetDetailLessonPlanDocumentHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public GetDetailLessonPlanDocumentHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailLessonPlanDocumentRequest>(
                nameof(GetDetailLessonPlanDocumentRequest.IdLessonPlanDocument)
            );

            var lessonPlanDocument = _dbContext.Entity<TrLessonPlanDocument>()
                .Include(x => x.LessonPlan)
                    .ThenInclude(x => x.WeekSettingDetail)
                        .ThenInclude(x => x.WeekSetting)
                            .ThenInclude(x => x.Period)
                                .ThenInclude(x => x.Grade)
                                    .ThenInclude(x => x.Level)
                                        .ThenInclude(x => x.AcademicYear)
                .Include(x => x.LessonPlan)
                    .ThenInclude(x => x.LessonTeacher)
                        .ThenInclude(x => x.Lesson)
                            .ThenInclude(x => x.Subject)
                .Include(x => x.LessonPlan)
                    .ThenInclude(x => x.LessonTeacher)
                        .ThenInclude(x => x.Staff)
                .Include(x => x.LessonPlan)
                    .ThenInclude(x => x.SubjectMappingSubjectLevel)
                        .ThenInclude(x => x.SubjectLevel)
                .Include(x => x.LessonPlan)
                    .ThenInclude(x => x.LessonPlanApprovals)
                        .ThenInclude(x => x.User)
                .FirstOrDefault(x => x.Id == param.IdLessonPlanDocument);
            if (lessonPlanDocument == null)
                throw new NotFoundException("Lesson plan document not found");
            
            var lessonPlan = _dbContext.Entity<TrLessonPlan>()
                .Include(x => x.LessonPlanDocuments)
                .FirstOrDefault(x => x.Id == lessonPlanDocument.IdLessonPlan);
            if (lessonPlan == null)
                throw new NotFoundException("Lesson plan not found");
            
            var latestApproval = lessonPlanDocument.LessonPlan.LessonPlanApprovals.Where(x => x.DateUp != null).OrderByDescending(x => x.DateUp).FirstOrDefault();
            var nextApproval = lessonPlanDocument.LessonPlan.LessonPlanApprovals.Where(x => x.DateUp == null).OrderBy(x => x.StateNumber).FirstOrDefault();
            var data = new GetDetailLessonPlanDocumentResult
            {
                Id = lessonPlanDocument.Id,
                AcademicYear = lessonPlanDocument.LessonPlan.WeekSettingDetail.WeekSetting.Period.Grade.Level.AcademicYear.Code,
                Level = lessonPlanDocument.LessonPlan.WeekSettingDetail.WeekSetting.Period.Grade.Level.Code,
                Grade = lessonPlanDocument.LessonPlan.WeekSettingDetail.WeekSetting.Period.Grade.Description,
                Term = lessonPlanDocument.LessonPlan.WeekSettingDetail.WeekSetting.Period.Description,
                IdSubject = lessonPlanDocument.LessonPlan.LessonTeacher.Lesson.Subject.Id,
                Subject = lessonPlanDocument.LessonPlan.LessonTeacher.Lesson.Subject.Description,
                SubjectLevel = lessonPlanDocument.LessonPlan.SubjectMappingSubjectLevel != null ? lessonPlanDocument.LessonPlan.SubjectMappingSubjectLevel.SubjectLevel.Code : "-",
                CreatedBy = lessonPlanDocument.LessonPlan.LessonTeacher.Staff.FirstName + (lessonPlanDocument.LessonPlan.LessonTeacher.Staff.LastName != null ? (" " + lessonPlanDocument.LessonPlan.LessonTeacher.Staff.LastName) : ""),
                CreatedDate = lessonPlanDocument.LessonPlanDocumentDate,
                PathFile = lessonPlanDocument.PathFile,
                Filename = lessonPlanDocument.Filename,
                Periode = "Week " + lessonPlanDocument.LessonPlan.WeekSettingDetail.WeekNumber,
                Status = lessonPlanDocument.Status == "Approved"
                            ? lessonPlanDocument.Status
                            : lessonPlanDocument.LessonPlan.WeekSettingDetail.DeadlineDate > (lessonPlanDocument.DateUp.HasValue ? lessonPlanDocument.DateUp.Value : lessonPlanDocument.LessonPlanDocumentDate) 
                                ? lessonPlanDocument.Status : "Late",
                RespondedBy = latestApproval?.User?.DisplayName,
                RespondedDate = latestApproval?.LessonPlanApprovalDate,
                Reason = latestApproval?.Reason,
                ApprovalState = latestApproval != null ? latestApproval.StateNumber : 0,
                NextApproval = nextApproval?.IdUser,
                IdLessonPlanApproval = nextApproval?.Id,
                IdUser = lessonPlanDocument.LessonPlan.LessonTeacher.Staff.IdBinusian,
                DisplayNote = lessonPlan.LessonPlanDocuments.OrderByDescending(x => x.LessonPlanDocumentDate).FirstOrDefault().Id == lessonPlanDocument.Id
            };
                
            return Task.FromResult(Request.CreateApiResult2(data as object));
        }
    }
}
