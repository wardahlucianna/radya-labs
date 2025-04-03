using System.Linq;
using System.Collections.Generic;
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
using BinusSchool.Common.Abstractions;
using BinusSchool.Domain.NoEntities.FormBuilder;
using BinusSchool.Persistence.TeachingDb.Entities.User;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanDocumentListHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public LessonPlanDocumentListHandler(ITeachingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var validUpload = true;
            var param = Request.ValidateParams<GetLessonPlanDocumentListRequest>(
                nameof(GetLessonPlanDocumentListRequest.IdUser),
                nameof(GetLessonPlanDocumentListRequest.IdLessonPlan)
            );

            var AcademicYearTransaction = await (
                from tlp in _dbContext.Entity<TrLessonPlan>()
                join mlt in _dbContext.Entity<MsLessonTeacher>() on tlp.IdLessonTeacher equals mlt.Id
                join ml in _dbContext.Entity<MsLesson>() on mlt.IdLesson equals ml.Id
                join ms in _dbContext.Entity<MsSubject>() on ml.IdSubject equals ms.Id
                join mg in _dbContext.Entity<MsGrade>() on ms.IdGrade equals mg.Id
                join ml2 in _dbContext.Entity<MsLevel>() on mg.IdLevel equals ml2.Id
                where mlt.IdLesson == param.IdLesson && tlp.Id == param.IdLessonPlan
                select new
                {
                    ml2.IdAcademicYear
                }
            ).FirstOrDefaultAsync(CancellationToken);

            var idDataLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
              .Include(x => x.Lesson)
              .Where(x => x.IdUser == param.IdUser && x.Lesson.IdAcademicYear == AcademicYearTransaction.IdAcademicYear && x.IsLessonPlan)
              .Select(x => x.IdUser)
              .FirstOrDefaultAsync(CancellationToken);

            var isLessonTeacher = false;

            if (!string.IsNullOrEmpty(idDataLessonTeacher))
            {
                if (idDataLessonTeacher == param.IdLesson)
                    isLessonTeacher = true;
            }

            if (!isLessonTeacher)
            {
                validUpload = false;
            }

            var _trLessonPlan = _dbContext.Entity<TrLessonPlan>()
                                .Include(e => e.LessonPlanDocuments)
                                .Include(x => x.LessonTeacher).ThenInclude(x => x.Lesson);

            var queryResult = await (
                from tlp in _dbContext.Entity<TrLessonPlan>()
                join mlt in _dbContext.Entity<MsLessonTeacher>() on tlp.IdLessonTeacher equals mlt.Id
                join ml in _dbContext.Entity<MsLesson>() on mlt.IdLesson equals ml.Id
                join ms in _dbContext.Entity<MsSubject>() on ml.IdSubject equals ms.Id
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
                where mlt.IdLesson == param.IdLesson && tlp.Id == param.IdLessonPlan

                select new GetLessonPlanDocumentListResult
                {
                    // IdClass = ml.ClassIdGenerated,
                    IdLessonPlan = tlp.Id,
                    IdUser = mlt.IdUser,
                    IdAcademicYear = may.Id,
                    AcademicYear = may.Code,
                    IdLevel = ml2.Id,
                    Level = ml2.Code,
                    IdGrade = mg.Id,
                    Grade = mg.Description,
                    Semester = mp.Semester,
                    IdPeriod = mws.IdPeriod,
                    Term = mp.Description,
                    Subject = ms.Description,
                    SubjectLevel = subMsl != null ? subMsl.Code : "-",
                    WeekNumber = mwsd.WeekNumber,
                    Periode = "Week " + mwsd.WeekNumber,
                    //Status = tlp.Status,
                    Status = mwsd.Status == true
                            ? _trLessonPlan
                                .Where(e => e.IdWeekSettingDetail == mwsd.Id
                                        && e.WeekSettingDetail.WeekNumber == mwsd.WeekNumber
                                        && e.LessonTeacher.IdLesson == mlt.IdLesson
                                        && e.LessonTeacher.Lesson.IdSubject == ms.Id
                                        && e.IdSubjectMappingSubjectLevel == tlp.IdSubjectMappingSubjectLevel)
                                .FirstOrDefault().Status
                            : _trLessonPlan
                                .Where(e => e.IdWeekSettingDetail == mwsd.Id
                                        && e.WeekSettingDetail.WeekNumber == mwsd.WeekNumber
                                        && e.LessonTeacher.IdLesson == mlt.IdLesson
                                        && e.IdSubjectMappingSubjectLevel == tlp.IdSubjectMappingSubjectLevel)
                                .FirstOrDefault().Status == "Need Approval"
                                    ? "Inactive"
                                    : _trLessonPlan
                                        .Where(e => e.IdWeekSettingDetail == mwsd.Id
                                            && e.WeekSettingDetail.WeekNumber == mwsd.WeekNumber
                                            && e.LessonTeacher.IdLesson == mlt.IdLesson
                                            && e.IdSubjectMappingSubjectLevel == tlp.IdSubjectMappingSubjectLevel).FirstOrDefault().Status,
                    DeadlineDate = mwsd.DeadlineDate,
                    TeacherName = ms2.FirstName + (ms2.LastName != null ? (" " + ms2.LastName) : ""),
                    CanUpload = validUpload == false ? false : ((tlp.Status == "Created" || tlp.Status == "Unsubmitted" || tlp.Status == "Need Revision" || tlp.Status == "Approved") && (mwsd.DeadlineDate >= _dateTime.ServerTime)) ? true : false,
                }
            ).ToListAsync(CancellationToken);

            var data = queryResult.FirstOrDefault();
            if (data == null)
                throw new NotFoundException("Lesson plan not found");

            var documents = await (from document in _dbContext.Entity<TrLessonPlanDocument>()
                                   join userCreate in _dbContext.Entity<MsUser>() on document.UserIn equals userCreate.Id
                                   where document.IdLessonPlan == data.IdLessonPlan
                                   select new LessonPlanDocument
                                   {
                                       IdLessonPlanDocument = document.Id,
                                       CreatedDate = document.DateUp.HasValue ? document.DateUp.Value : document.LessonPlanDocumentDate,
                                       UpdatedDate = document.DateUp,
                                       Status = data.Status == "Approved"
                                            ? data.Status
                                            : data.DeadlineDate > (document.DateUp.HasValue ? document.DateUp.Value : document.LessonPlanDocumentDate)
                                                ? data.Status : "Late",
                                       UserCreate = userCreate.DisplayName
                                   }).ToListAsync(CancellationToken);

            data.LessonPlanDocuments = documents.OrderByDescending(x => x.CreatedDate).ToList();

            if (data.Status == "Approved" || !data.LessonPlanDocuments.Any() || data.DeadlineDate > data.LessonPlanDocuments.Max(e => e.CreatedDate))
                data.Status = data.Status;
            else
                data.Status = "Late";

            return Request.CreateApiResult2(data as object);
        }
    }
}
