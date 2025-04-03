using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolsEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetSchoolEventRequest.IdAcademicYear),
            nameof(GetSchoolEventRequest.IdUser),
        };
        private static readonly string[] _columns = { "EventName", "EventType", "AssignedAs", "StartDate", "EndDate", "ApprovalStatus", "ApprovalDescription" };
        private readonly ISchedulingDbContext _dbContext;

        public GetSchoolEventHandler(ISchedulingDbContext SchoolEventDbContext)
        {
            _dbContext = SchoolEventDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSchoolEventRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<TrEvent>(x => x.IsActive==true);
            
            predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);
            
            if(!string.IsNullOrEmpty(param.StartDate) || !string.IsNullOrEmpty(param.EndDate))
            {
                predicate = predicate.And(x => x.EventDetails.Any(y
                    => y.StartDate == Convert.ToDateTime(param.StartDate) || y.EndDate == Convert.ToDateTime(param.EndDate) 
                    || (y.StartDate < Convert.ToDateTime(param.StartDate)
                        ? (y.EndDate > Convert.ToDateTime(param.StartDate) && y.EndDate < Convert.ToDateTime(param.EndDate)) || y.EndDate > Convert.ToDateTime(param.EndDate)
                        : (Convert.ToDateTime(param.EndDate) > y.StartDate && Convert.ToDateTime(param.EndDate) < y.EndDate) || Convert.ToDateTime(param.EndDate) > y.EndDate)));
            }

            if (!string.IsNullOrEmpty(param.IdEventType))
                predicate = predicate.And(x => x.IdEventType== param.IdEventType);
            if (!string.IsNullOrEmpty(param.ApprovalStatus))
            {
                if (param.ApprovalStatus == "OnReview1" || param.ApprovalStatus == "0")
                {
                    predicate = predicate.And(x => x.StatusEvent == "Approved" ? x.StatusEventAward == "On Review (1)" : x.StatusEvent == "On Review (1)");
                }
                else if (param.ApprovalStatus == "OnReview2" || param.ApprovalStatus == "1")
                {
                    predicate = predicate.And(x => x.StatusEvent == "Approved" ? x.StatusEventAward == "On Review (2)" : x.StatusEvent == "On Review (2)");
                }
                else
                {
                    predicate = predicate.And(x => x.StatusEvent == "Approved" ? x.StatusEventAward == param.ApprovalStatus : x.StatusEvent == param.ApprovalStatus);
                }
            }
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, $"%{param.Search}%"));
            if (!string.IsNullOrEmpty(param.AssignedAs))
            {
                switch (param.AssignedAs)
                {
                    case "EventCoordinator":
                        predicate = predicate.And(x => x.EventCoordinators.Any(e => e.IdEvent == x.Id && e.IdUser == param.IdUser));
                        break;
                    case "ActivityPIC":
                        predicate = predicate.And(x => x.EventActivities.Any(e => e.EventActivityPICs.Any(z => z.IdUser == param.IdUser)));
                        break;
                    case "Registrant":
                        predicate = predicate.And(x => x.EventCoordinators.Any(e => e.Event.EventActivities.Any(y => y.EventActivityRegistrants.Any(z => z.IdUser == param.IdUser))));
                        break;
                    case "Approver":
                        predicate = predicate.And(x => x.EventCoordinators.Any(e => e.Event.EventApprovers.Any(y => y.IdUser == param.IdUser)));
                        break;
                    case "AttendancePIC":
                        predicate = predicate.And(x => x.EventCoordinators.Any(e => e.Event.EventIntendedFor.Any(f => f.EventIntendedForAttendanceStudents.Any(y => y.EventIntendedForAtdPICStudents.Any(z => z.IdUser == param.IdUser)))));
                        break;
                    case "EventCreator":
                        predicate = predicate.And(x => x.EventCoordinators.Any(e => x.UserIn == param.IdUser));
                        break;
                }
                
            }

            predicate = predicate.And(x => x.IsStudentInvolvement == false);

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

            var dataQuery = _dbContext.Entity<TrEvent>()
                .Include(e => e.EventCoordinators)
                .Include(e => e.EventType)
                .Include(e => e.EventApprovers)
                .Include(e => e.EventActivities)
                .Include(e => e.EventActivities).ThenInclude(e => e.EventActivityPICs)
                .Include(e => e.EventActivities).ThenInclude(e => e.EventActivityRegistrants)
                .Include(e => e.EventIntendedFor).ThenInclude(e => e.EventIntendedForAttendanceStudents).ThenInclude(e => e.EventIntendedForAtdPICStudents)
                .Include(x => x.EventDetails)
                .Include(e => e.EventApprovals)
                .Where(predicate);

            if(CheckRole.IdRoleGroup == "PRT"){
                dataQuery.Include(e => e.EventActivities).ThenInclude(e => e.EventActivityAwards).ThenInclude(e => e.HomeroomStudent).ThenInclude(e => e.Student).ThenInclude(e => e.StudentParents).ThenInclude(e => e.Parent);
                predicate = predicate.And(x => x.EventActivities.Any(e => e.EventActivityAwards.Any(f => f.HomeroomStudent.Student.StudentParents.First().IdParent == param.IdUser)));
                // dataQuery.Where(predicate);
            }

            // dataQuery.Where(predicate);

            var query = dataQuery
               .Select(x => new
               {
                   EventId = x.Id,
                   EventName = x.Name,
                   IsAssignedAsCoordinator = x.EventCoordinators.Any(e => e.IdEvent == x.Id && e.IdUser == param.IdUser),
                   IsAssignedAsActivityPIC = x.EventActivities.Any(y => y.EventActivityPICs.Any(z => z.IdUser == param.IdUser)),
                   IsAssignedAsRegistrant = x.EventActivities.Any(y => y.EventActivityRegistrants.Any(z => z.IdUser == param.IdUser)),
                   IsAssignedAsApproval = x.EventApprovers.Any(e => e.IdEvent == x.Id && e.IdUser == param.IdUser),
                   IsAssignedAsAttendentPIC = x.EventIntendedFor.Any(e => e.EventIntendedForAttendanceStudents.Any(e => e.EventIntendedForAtdPICStudents.Any(e => e.IdUser == param.IdUser))),
                   IsAttendentEventCreator = x.UserIn == param.IdUser,
                   StartDate = x.EventDetails.Min(e=>e.StartDate),
                   EndDate = x.EventDetails.Max(e => e.EndDate),
                   ApprovalSatatus = x.StatusEvent == "Approved" ? x.StatusEventAward : x.StatusEvent,
                   ApprovalDescription = x.StatusEvent == "Approved" && x.StatusEventAward == "Approved" && x.DescriptionEventAward != null ? x.DescriptionEventAward : x.StatusEvent == "Approved" && x.StatusEventAward != "Approved" ? x.DescriptionEventAward : x.DescriptionEvent,
                   SortApprovalSatatus = (x.StatusEvent.Contains("On Review") || x.StatusEventAward.Contains("On Review")) == true ? 0 : 1,
                //    CanDelete = x.StatusEvent == "Approved" ?
                                // (x.EventApprovals.Where(e => e.IdEvent == x.Id && e.State == x.EventApprovals.Select(e => e.State).Max()).Count() > 0 ? true : false)
                                // :x.StatusEvent == "Decline" ? true : false,
                   CanApprove = (x.StatusEvent.Contains("On Review") || x.StatusEventAward.Contains("On Review")) ? true : false,
                   CanEdit = (x.StatusEvent.Contains("On Review") || x.StatusEventAward.Contains("On Review")) ? false : true,
                //    CanDelete = (trEvent.StatusEvent == "Declined" || trEvent.StatusEventAward == "Declined" || (trEvent.StatusEvent == "Approved" && trEvent.StatusEventAward == "Approved")) ? (trEvent.EventApprovers != null ? (trEvent.EventApprovers.Count() > 1 ? trEvent.EventApprovers.Last().IdUser == param.IdUser : trEvent.EventApprovers.First().IdUser == param.IdUser) : trEvent.StatusEvent == "Approved") ? true : false : false,
                //    CanDelete = (x.StatusEvent == "Declined" || x.StatusEventAward == "Declined" || (x.StatusEvent == "Approved" && x.StatusEventAward == "Approved")) ? (x.EventApprovers != null ? x.EventApprovers.First().IdUser == param.IdUser : x.StatusEvent == "Approved") ? true : false : false,
                    CanDelete = x.StatusEvent == "Declined" || x.StatusEventAward == "Declined" ?
                                    x.UserIn == param.IdUser || (x.EventApprovers.Any(y => y.IdUser == param.IdUser) || x.EventAwardApprovers.Any(y => y.IdUser == param.IdUser)) ?
                                    true : false
                                : x.StatusEvent == "Approved" && x.StatusEventAward == "Approved" && x.DescriptionEventAward == null ?
                                    x.EventApprovers.OrderByDescending(x => x.OrderNumber).First().IdUser == param.IdUser ?
                                    true : false
                                : x.StatusEvent == "Approved" && x.StatusEventAward == "Approved" && x.DescriptionEventAward != null ?
                                    x.EventAwardApprovers.OrderByDescending(x => x.OrderNumber).First().IdUser == param.IdUser ?
                                    true : false
                                : false
                    ,
                   EventType = x.EventType.Description
               })
               .Where(e => e.IsAssignedAsCoordinator || e.IsAssignedAsActivityPIC || e.IsAssignedAsRegistrant|| e.IsAssignedAsApproval|| e.IsAssignedAsAttendentPIC || e.IsAttendentEventCreator)
               .OrderBy(x => x.SortApprovalSatatus).ThenBy(x => x.StartDate);

            //ordering
            switch (param.OrderBy)
            {
                case "EventName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EventName)
                        : query.OrderBy(x => x.EventName);
                    break;
                case "EventType":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EventType)
                        : query.OrderBy(x => x.EventType);
                    break;
                case "StartDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StartDate)
                        : query.OrderBy(x => x.StartDate);
                    break;
                case "EndDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EndDate)
                        : query.OrderBy(x => x.EndDate);
                    break;
                case "ApprovalStatus":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ApprovalSatatus)
                        : query.OrderBy(x => x.ApprovalSatatus);
                    break;
                case "ApprovalDescription":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ApprovalDescription)
                        : query.OrderBy(x => x.ApprovalDescription);
                    break;
            };


            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetSchoolEventResult
                {
                    EventId = x.EventId,
                    EventName = x.EventName,
                    EventType = x.EventType,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ApprovalSatatus = x.ApprovalSatatus,
                    ApprovalDescription = x.ApprovalDescription,
                    CanDelete = x.CanDelete,
                    CanEdit = x.CanEdit,
                    CanApprove = x.CanApprove,
                    AssignedAs = GetAssignedAs(x.IsAssignedAsCoordinator, x.IsAssignedAsActivityPIC, x.IsAssignedAsRegistrant, x.IsAssignedAsApproval, x.IsAssignedAsAttendentPIC, x.IsAttendentEventCreator),
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
                items = result.Select(x => new GetSchoolEventResult
                {
                    EventId = x.EventId,
                    EventName = x.EventName,
                    EventType = x.EventType,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ApprovalSatatus = x.ApprovalSatatus,
                    ApprovalDescription = x.ApprovalDescription,
                    CanDelete = x.CanDelete,
                    CanEdit = x.CanEdit,
                    CanApprove = x.CanApprove,
                    AssignedAs = GetAssignedAs(x.IsAssignedAsCoordinator, x.IsAssignedAsActivityPIC, x.IsAssignedAsRegistrant, x.IsAssignedAsApproval, x.IsAssignedAsAttendentPIC, x.IsAttendentEventCreator),
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.EventId).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        public string GetAssignedAs (bool IsAssignedAsCoordinator, bool IsAssignedAsActivityPIC, bool IsAssignedAsRegistrant, bool IsAssignedAsApproval, bool IsAssignedAsAttendentPIC, bool IsAttendentEventCreator)
        {
            var NewAssignesAs = "";
            if (IsAssignedAsCoordinator)
                NewAssignesAs += "Event Coordinator,";
            if (IsAssignedAsActivityPIC)
                NewAssignesAs += "Activity PIC,";
            if (IsAssignedAsRegistrant)
                NewAssignesAs += "Regristrant,";
            if (IsAssignedAsApproval)
                NewAssignesAs += "Approver,";
            if (IsAssignedAsAttendentPIC)
                NewAssignesAs += "Attendance PIC,";
            if (IsAttendentEventCreator)
                NewAssignesAs += "Event Creator,";

            return NewAssignesAs.TrimEnd(',');
        }
    }
}
