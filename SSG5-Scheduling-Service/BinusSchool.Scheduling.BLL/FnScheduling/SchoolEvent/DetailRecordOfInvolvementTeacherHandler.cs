using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Constants;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class DetailRecordOfInvolvementTeacherHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DetailRecordOfInvolvementTeacherHandler(ISchedulingDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        private string GetUser(string idUser)
        {
            var dataUser = _dbContext.Entity<MsUser>().SingleOrDefault();
            if(dataUser == null) return "-";

            return dataUser.DisplayName;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DetailRecordOfInvolvementRequest>(nameof(DetailRecordOfInvolvementRequest.IdEvent),nameof(DetailRecordOfInvolvementRequest.IdUser));

            var CheckRole = await (from a in _dbContext.Entity<MsUser>()
                                   join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                   join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                   where a.Id == param.IdUser
                                    
                                  select new LtRole
                                  {
                                      IdRoleGroup = rg.IdRoleGroup
                                  }).FirstOrDefaultAsync(CancellationToken);

            if(CheckRole == null)
                throw new BadRequestException($"User in this role not found");
                
            var predicate = PredicateBuilder.Create<TrEvent>(x => true);

            var query = _dbContext.Entity<TrEvent>()
                .Include(x => x)
                    .ThenInclude(x => x.CertificateTemplate)
                .Include(x => x.EventType)
                .Include(x => x.AcademicYear)
                .Include(x => x.EventDetails)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.Activity)
                        .ThenInclude(x => x.HistoryEventActivities)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.EventActivityPICs)
                        .ThenInclude(x => x.User)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.EventActivityRegistrants)
                        .ThenInclude(x => x.User)
                .Include(x => x.EventActivities)
                    .ThenInclude(x => x.EventActivityAwardTeachers)
                        .ThenInclude(x => x.Award)
                .Include(x => x.EventApprovals)
                    .ThenInclude(x => x.User)
                .Include(x => x.EventAwardApprovers)
                    .ThenInclude(x => x.User)
                .Where(predicate);

            if (param.IdEvent != null)
                query = query.Where(x => x.Id == param.IdEvent);
            
            var trEvent = await query
                    .SingleOrDefaultAsync(CancellationToken);

            var result = new DetailRecordOfInvolvementTeacherResult
                {
                    Id = trEvent.Id,
                    EventName = trEvent.Name,
                    AcademicYear = new CodeWithIdVm(trEvent.IdAcademicYear, trEvent.AcademicYear.Code, trEvent.AcademicYear.Description),
                    Dates = trEvent.EventDetails.OrderBy(y => y.StartDate).Select(y => new DateTimeRange
                    {
                        Start = y.StartDate,
                        End = y.EndDate
                    }),
                    EventType = new CalendarEventTypeVm
                    {
                        Id = trEvent.IdEventType,
                        Code = trEvent.EventType.Code,
                        Description = trEvent.EventType.Description,
                        Color = trEvent.EventType.Color
                    },
                    Activity = trEvent.EventActivities != null ? trEvent.EventActivities.Select(y => new DataEventActivityTeacher{
                        Id = y.Id,
                        IdActivity = y.IdActivity,
                        NameActivity = y.Activity.Description,
                        EventActivityPICIdUser =  y.EventActivityPICs != null ? y.EventActivityPICs.Select(z => new DataUserActivity {Id = z.IdUser, DisplayName = z.User.DisplayName}).ToList() : null,
                        EventActivityRegistrantIdUser = y.EventActivityRegistrants != null ? y.EventActivityRegistrants.Select(z => new DataUserActivity {Id = z.Id, DisplayName = z.User.DisplayName}).ToList() : null,
                        EventActivityAwardTeacherIdUser = y.EventActivityAwardTeachers != null ? y.EventActivityAwardTeachers.Select(z => new DetailDataEventActivityAwardTeacher{
                            IdEventActivityAwardTeacher = z.Id,
                            IdStaff = z.IdStaff,
                            IdAward = z.Award.Id,
                            NameAward = z.Award.Description,
                            // IdAward = z.IdAward,
                            Url = z.Url,
                            Filename = z.Filename,
                            Filetype = z.Filetype,
                            Filesize = z.Filesize,
                            OriginalFilename = z.OriginalFilename,
                        }).ToList() : null
                    }).ToList() : null,
                    CertificateTemplate = trEvent.CertificateTemplate != null ? new CodeWithIdVm(trEvent.CertificateTemplate.Id,null,trEvent.CertificateTemplate.Description) : null,
                    ApprovalSatatus = trEvent.StatusEvent,
                    StatusDeclined = trEvent.StatusEvent == "Declined" ? trEvent.EventApprovals.Where(x => !x.IsApproved).Select(x => new Declained
                    {
                        ApprovalCount = trEvent.EventApprovals.Count(y => y.IsApproved) + 1,
                        DeclinedBy = x.User.DisplayName,
                        DeclinedDate = trEvent.DateIn,
                        Note = x.Reason
                    }).FirstOrDefault() : null,
                    CanApprove = CheckRole.IdRoleGroup != "STD" && (trEvent.StatusEvent.Contains("On Review")),
                    CanEdit = CheckRole.IdRoleGroup == "STD" && (trEvent.StatusEvent == "Declined"),
                    CanDelete = CheckRole.IdRoleGroup == "STD" && (trEvent.StatusEvent == "Declined"),
                };

            return Request.CreateApiResult2(result as object);
        }
    }
}
