using System;
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
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Teaching.FnLessonPlan.LessonPlan.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class SetLessonPlanApprovalSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public SetLessonPlanApprovalSettingHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetLessonPlanApprovalSettingRequest, SetLessonPlanApprovalSettingValidator>();

            var levelApproval = _dbContext.Entity<MsLevelApproval>().Find(body.IdLevelApproval);
            var now = DateTimeUtil.ServerTime;
            // if (levelApproval == null) throw new NotFoundException("Level approval not found");
            if (levelApproval == null)
            {
                var checkSchoolByLevel = _dbContext.Entity<MsLevel>().Include(x => x.AcademicYear).ThenInclude(x => x.School).Where(x => x.Id == body.IdLevelApproval).FirstOrDefault();
                var dataLessonApproval = _dbContext.Entity<MsLessonApproval>().Where(x => x.IdSchool == checkSchoolByLevel.AcademicYear.School.Id).FirstOrDefault();
                if (dataLessonApproval == null) throw new NotFoundException("Lesson approval not found");
                var guidIdLevelApproval = Guid.NewGuid().ToString();
                _dbContext.Entity<MsLevelApproval>().Add(new MsLevelApproval
                {
                    Id = guidIdLevelApproval,
                    IdLevel = body.IdLevelApproval,
                    IdLessonApproval = dataLessonApproval.Id,
                    IsApproval = body.IsApproved,
                });
            }
            else
            {
                levelApproval.IsApproval = body.IsApproved;
                if (body.IsApproved == false)
                {
                    var updatedlessonPlans = await _dbContext.Entity<TrLessonPlan>()
                        .Include(x => x.LessonPlanApprovals)
                        .Include(x => x.LessonPlanDocuments)
                        .Include(x => x.LessonTeacher)
                            .ThenInclude(x => x.Lesson)
                                .ThenInclude(x => x.Grade)
                                    .ThenInclude(x => x.Level)
                        .Where(x => x.LessonTeacher.Lesson.Grade.IdLevel == levelApproval.IdLevel)
                        .Where(x => x.Status == "Need Approval")
                        .Where(x => x.LessonPlanApprovals.Any(o => o.IdLessonPlan == x.Id))
                        .Select(x => x)
                        .ToListAsync(CancellationToken);
                    foreach (var item in updatedlessonPlans)
                    {
                        item.Status = "Created";
                        var newDocument = new TrLessonPlanDocument
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdLessonPlan = item.Id,
                            PathFile = item.LessonPlanDocuments.OrderByDescending(x => x.LessonPlanDocumentDate).FirstOrDefault()?.PathFile,
                            Filename = item.LessonPlanDocuments.OrderByDescending(x => x.LessonPlanDocumentDate).FirstOrDefault()?.Filename,
                            Status = "Created",
                            LessonPlanDocumentDate = now
                        };
                        var approval = item.LessonPlanApprovals.OrderByDescending(x => x.LessonPlanApprovalDate).FirstOrDefault();
                        approval.IsActive = false;
                        _dbContext.Entity<TrLessonPlanApproval>().Update(approval);
                        _dbContext.Entity<TrLessonPlanDocument>().Add(newDocument);
                    }
                    _dbContext.Entity<TrLessonPlan>().UpdateRange(updatedlessonPlans);
                }
                else
                {
                    var updatedlessonPlans = await _dbContext.Entity<TrLessonPlan>()
                        .Include(x => x.LessonPlanApprovals)
                        .Include(x => x.LessonPlanDocuments)
                        .Include(x => x.LessonTeacher)
                            .ThenInclude(x => x.Lesson)
                                .ThenInclude(x => x.Grade)
                                    .ThenInclude(x => x.Level)
                        .Where(x => x.LessonTeacher.Lesson.Grade.IdLevel == levelApproval.IdLevel)
                        .Where(x => x.Status == "Created")
                        .Where(x => x.LessonPlanApprovals.Any(o => o.IdLessonPlan == x.Id))
                        .IgnoreQueryFilters()
                        .Select(x => x)
                        .ToListAsync(CancellationToken);
                    foreach (var item in updatedlessonPlans)
                    {
                        item.Status = "Need Approval";
                        var newDocument = new TrLessonPlanDocument
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdLessonPlan = item.Id,
                            PathFile = item.LessonPlanDocuments.OrderByDescending(x => x.LessonPlanDocumentDate).FirstOrDefault()?.PathFile,
                            Filename = item.LessonPlanDocuments.OrderByDescending(x => x.LessonPlanDocumentDate).FirstOrDefault()?.Filename,
                            Status = "Need Approval",
                            LessonPlanDocumentDate = now
                        };
                        var approval = item.LessonPlanApprovals.OrderByDescending(x => x.LessonPlanApprovalDate).FirstOrDefault();
                        approval.IsActive = true;
                        _dbContext.Entity<TrLessonPlanApproval>().Update(approval);
                        _dbContext.Entity<TrLessonPlanDocument>().Add(newDocument);
                    }
                    _dbContext.Entity<TrLessonPlan>().UpdateRange(updatedlessonPlans);
                }
            }


            // var weekSettings = _dbContext.Entity<MsWeekSetting>()
            //     .Include(x => x.Period)
            //         .ThenInclude(x => x.Grade)
            //     .Include(x => x.WeekSettingDetails)
            //     .Where(x => x.Period.Grade.IdLevel == levelApproval.IdLevel && x.Status)
            //     .ToList();

            // if (weekSettings.Any(x => x.WeekSettingDetails.Any(y => DateTime.Now >= y.DeadlineDate))) 
            //     throw new Exception("Can't save approval setting because one of the week setting in this level is already on progress");


            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
