using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Common.Constants;
using BinusSchool.Persistence.TeachingDb.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Common.Exceptions;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{

    public class GetSubjectLessonPlanApprovalHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IRolePosition _serviceRolePosition;

        public GetSubjectLessonPlanApprovalHandler(ITeachingDbContext dbContext, IRolePosition serviceRolePosition)
        {
            _dbContext = dbContext;
            _serviceRolePosition = serviceRolePosition;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSubjectLessonPlanApprovalRequest>();

            var idSchool = await _dbContext.Entity<MsAcademicYear>()
                           .Where(e => e.Id == param.IdAcademicYear)
                           .Select(e => e.IdSchool)
                           .FirstOrDefaultAsync(CancellationToken);

            var msLessonPlanApprover = await _dbContext.Entity<MsLessonPlanApproverSetting>()
                                            .Include(x => x.Role)
                                            .Include(x => x.TeacherPosition)
                                                .ThenInclude(x => x.Position)
                                            .Where(x => x.Role.IdSchool == idSchool)
                                            .ToListAsync(CancellationToken);

            var approverSetting = new List<GetUserEmailRecepient>();

            foreach (var item in msLessonPlanApprover)
            {
                approverSetting.Add(new GetUserEmailRecepient
                {
                    IdRole = item.IdRole,
                    IdTeacherPosition = item.IdTeacherPosition,
                    IdUser = item.IdBinusian
                });
            }

            var paramUserSubject = new GetUserSubjectByEmailRecepientRequest
            {
                IdUser = param.IdUser,
                IdAcademicYear = param.IdAcademicYear,
                IdSchool = idSchool,
                IsShowIdUser = false,
                EmailRecepients = approverSetting
            };

            var json = JsonConvert.SerializeObject(paramUserSubject);

            var getApiSubjectByUser = await _serviceRolePosition.GetUserSubjectByEmailRecepient(paramUserSubject);

            var getSubjectByUser = getApiSubjectByUser.IsSuccess ? getApiSubjectByUser.Payload : null;

            var query = getSubjectByUser.Distinct();
            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(e => e.Level.Id == param.IdLevel);

            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(e => e.Grade.Id == param.IdGrade);

            getSubjectByUser = query.ToList();

            var getIdLessonByUser = new List<GetSubjectLessonPlanApprovalResult>();
            List<GetSubjectLessonPlanApprovalResult> items = new List<GetSubjectLessonPlanApprovalResult>();
            if (getSubjectByUser != null)
            {
                var listLevel = getSubjectByUser
                                .OrderBy(e => e.Level.OrderNumber)
                                .GroupBy(e => new
                                {
                                    IdLevel = e.Level.Id,
                                    Level = e.Level.Description,
                                    LevelCode = e.Level.Code,
                                    OrderNUmber = e.Level.OrderNumber
                                });

                foreach (var itemLevel in listLevel)
                {
                    var _listGrade = itemLevel
                        .Where(e => e.Level.Id == itemLevel.Key.IdLevel)
                        .OrderBy(e => e.Grade.OrderNumber)
                        .GroupBy(e => new
                        {
                            IdGrade = e.Grade.Id,
                            Grade = e.Grade.Description,
                            GradeCode = e.Grade.Code
                        });

                    List<GetGradeLessonPlanApproval> listGrade = new List<GetGradeLessonPlanApproval>();
                    foreach (var itemGrade in _listGrade)
                    {
                        var listSubject = itemGrade
                            .Where(e => e.Grade.Id == itemGrade.Key.IdGrade)
                            .OrderBy(e => e.Subject.Description)
                            .GroupBy(e => new
                            {
                                IdSubject = e.Subject.Id,
                                Subject = e.Subject.Description,
                                SubjectCode = e.Subject.Code
                            })
                            .Select(e => new ItemValueVm
                            {
                                Id = e.Key.IdSubject,
                                Description = e.Key.Subject,
                            }).ToList();

                        GetGradeLessonPlanApproval newGrade = new GetGradeLessonPlanApproval
                        {
                            Id = itemGrade.Key.IdGrade,
                            Description = itemGrade.Key.Grade,
                            Subject = listSubject
                        };

                        listGrade.Add(newGrade);
                    }


                    GetSubjectLessonPlanApprovalResult newLevel = new GetSubjectLessonPlanApprovalResult
                    {
                        Id = itemLevel.Key.IdLevel,
                        Description = itemLevel.Key.Level,
                        Grade = listGrade,
                        OrderNumber = itemLevel.Key.OrderNUmber
                    };
                    items.Add(newLevel);
                }
            }

            items = items.OrderBy(e => e.OrderNumber).Distinct().ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
