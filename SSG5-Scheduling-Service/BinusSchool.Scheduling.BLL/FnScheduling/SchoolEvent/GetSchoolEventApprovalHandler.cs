using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventApprovalHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetSchoolEventApprovalRequest.IdUser),
        };
        private static readonly string[] _columns = { "EventName", "EventType", "StartDate", "EndDate", "ApprovalStatus", "ApprovalDescription" };
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        
        public GetSchoolEventApprovalHandler(
            ISchedulingDbContext SchoolEventDbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = SchoolEventDbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSchoolEventApprovalRequest>(_requiredParams);

            var CheckRole = await (from a in _dbContext.Entity<MsUser>()
                                   join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                   join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                   where a.Id == param.IdUser
                                    
                                  select new LtRole
                                  {
                                      IdRoleGroup = rg.IdRoleGroup,
                                      IdSchool = rg.IdSchool
                                  }).FirstOrDefaultAsync(CancellationToken);

            if(CheckRole == null)
                throw new BadRequestException($"User role not found");

            // var currentAY = await _dbContext.Entity<MsPeriod>()
            //    .Include(x => x.Grade)
            //        .ThenInclude(x => x.Level)
            //            .ThenInclude(x => x.AcademicYear)
            //    .Where(x => x.Grade.Level.AcademicYear.IdSchool == CheckRole.IdSchool)
            //    .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
            //    .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
            //    .Select(x => new
            //    {
            //        Id = x.Grade.Level.AcademicYear.Id
            //    }).FirstOrDefaultAsync();

            if(CheckRole.IdRoleGroup == "PRT")
            {
                var predicateparent = PredicateBuilder.Create<TrUserEvent>(x => x.IsActive==true);
                
                // if(currentAY != null)
                //     predicateparent = predicateparent.And(x => x.EventDetail.Event.IdAcademicYear == currentAY.Id);
                
                if(!string.IsNullOrEmpty(param.StartDate) || !string.IsNullOrEmpty(param.EndDate))
                {
                    predicateparent = predicateparent.And(x => x.EventDetail.StartDate == Convert.ToDateTime(param.StartDate) || x.EventDetail.EndDate == Convert.ToDateTime(param.EndDate) 
                    || (x.EventDetail.StartDate < Convert.ToDateTime(param.StartDate)
                        ? (x.EventDetail.EndDate > Convert.ToDateTime(param.StartDate) && x.EventDetail.EndDate < Convert.ToDateTime(param.EndDate)) || x.EventDetail.EndDate > Convert.ToDateTime(param.EndDate)
                        : (Convert.ToDateTime(param.EndDate) > x.EventDetail.StartDate && Convert.ToDateTime(param.EndDate) < x.EventDetail.EndDate) || Convert.ToDateTime(param.EndDate) > x.EventDetail.EndDate));
                }
                if (!string.IsNullOrWhiteSpace(param.Search))
                    predicateparent = predicateparent.And(x => EF.Functions.Like(x.EventDetail.Event.Name, $"%{param.Search}%"));

                if (!string.IsNullOrEmpty(param.IdStudent))
                    predicateparent = predicateparent.And(x => x.IdUser == param.IdStudent);
                if (!string.IsNullOrEmpty(param.ApprovalStatus))
                {
                    if (param.ApprovalStatus == "OnReview1" || param.ApprovalStatus == "OnReview2")
                    {
                        predicateparent = predicateparent.And(x => x.IsNeedApproval == true && x.DateUp == null && x.IsApproved == false);
                    }
                    else if (param.ApprovalStatus == "Approved")
                    {
                        predicateparent = predicateparent.And(x => x.IsNeedApproval == true && x.DateUp != null && x.IsApproved == true);
                    }
                    else
                    {
                        predicateparent = predicateparent.And(x => x.IsNeedApproval == true && x.DateUp != null && x.IsApproved == false);
                    }
                }
                    
                    predicateparent = predicateparent.And(x => x.IsNeedApproval == true && x.EventDetail.Event.StatusEvent == "Approved");

                var dataqueryparent = _dbContext.Entity<TrUserEvent>()
                    .Include(x => x.EventDetail)
                        .ThenInclude(x => x.Event)
                            .ThenInclude(x=> x.AcademicYear)
                    .Include(x => x.User)
                    .Where(predicateparent)
                    .Where (x=>x.EventDetail.Event.AcademicYear.IdSchool == CheckRole.IdSchool);

                var queryparent = dataqueryparent
                .Select(x => new
                {
                    Id = x.Id,
                    EventId = x.EventDetail.Event.Id,
                    EventName = x.EventDetail.Event.Name,
                    StartDate = x.EventDetail.StartDate,
                    EndDate = x.EventDetail.EndDate,
                    ApprovalSatatus = x.IsNeedApproval == true && x.UserUp == null ? "On Review" : x.IsNeedApproval == true && x.IsApproved == true ? "Approved" : "Declined",
                    SortApprovalStatus = (x.EventDetail.Event.StatusEvent.Contains("On Review") || x.EventDetail.Event.StatusEventAward.Contains("On Review")) == true ? 0 : 1,
                    CanApprove = x.IsNeedApproval == true && x.UserUp == null  ? true : false,
                    IsApproved = x.IsApproved
                })
                .OrderBy(x => x.SortApprovalStatus).ThenBy(x => x.StartDate);

                //ordering
                switch (param.OrderBy)
                {
                    case "EventName":
                        queryparent = param.OrderType == OrderType.Desc
                            ? queryparent.OrderByDescending(x => x.EventName)
                            : queryparent.OrderBy(x => x.EventName);
                        break;
                    case "StartDate":
                        queryparent = param.OrderType == OrderType.Desc
                            ? queryparent.OrderByDescending(x => x.StartDate)
                            : queryparent.OrderBy(x => x.StartDate);
                        break;
                    case "EndDate":
                        queryparent = param.OrderType == OrderType.Desc
                            ? queryparent.OrderByDescending(x => x.EndDate)
                            : queryparent.OrderBy(x => x.EndDate);
                        break;
                    case "ApprovalStatus":
                        queryparent = param.OrderType == OrderType.Desc
                            ? queryparent.OrderByDescending(x => x.ApprovalSatatus)
                            : queryparent.OrderBy(x => x.ApprovalSatatus);
                        break;
                };

                IReadOnlyList<IItemValueVm> itemsParent;

                var resultParentApproved = await queryparent
                    .Where(x => x.ApprovalSatatus == "Approved")
                    .ToListAsync(CancellationToken);

                var resultParentDeclined = await queryparent
                    .Where(x => x.ApprovalSatatus == "Declined")
                    .ToListAsync(CancellationToken);

                var resultParentWaiting = await queryparent
                    .Where(x => x.ApprovalSatatus.Contains("On Review"))
                    .ToListAsync(CancellationToken);

                var resultParent = await queryparent
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                itemsParent = resultParent.Select(x => new GetSchoolEventApprovalResult
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    EventName = x.EventName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ApprovalSatatus = x.ApprovalSatatus,
                    CanApprove = x.CanApprove,
                    TotalApproved = resultParentApproved != null ? resultParentApproved.Count() : 0,
                    TotalDeclined = resultParentDeclined != null ? resultParentDeclined.Count() : 0,
                    TotalWaiting = resultParentWaiting != null ? resultParentWaiting.Count() : 0,
                }).ToList();

                var count = param.CanCountWithoutFetchDb(itemsParent.Count)
                ? itemsParent.Count
                : await queryparent.Select(x => x.EventId).CountAsync(CancellationToken);

                return Request.CreateApiResult2(itemsParent as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
            }
            else
            {
                var predicate = PredicateBuilder.Create<TrEvent>(x => x.IsActive==true);

                // if(currentAY != null)
                //     predicate = predicate.And(x => x.IdAcademicYear == currentAY.Id);
                
                if(!string.IsNullOrEmpty(param.StartDate) || !string.IsNullOrEmpty(param.EndDate))
                {
                    predicate = predicate.And(x => x.EventDetails.Any(y
                    => y.StartDate == Convert.ToDateTime(param.StartDate) || y.EndDate == Convert.ToDateTime(param.EndDate) 
                    || (y.StartDate < Convert.ToDateTime(param.StartDate)
                        ? (y.EndDate > Convert.ToDateTime(param.StartDate) && y.EndDate < Convert.ToDateTime(param.EndDate)) || y.EndDate > Convert.ToDateTime(param.EndDate)
                        : (Convert.ToDateTime(param.EndDate) > y.StartDate && Convert.ToDateTime(param.EndDate) < y.EndDate) || Convert.ToDateTime(param.EndDate) > y.EndDate)));
                }
                if (!string.IsNullOrWhiteSpace(param.Search))
                    predicate = predicate.And(x => EF.Functions.Like(x.Name, $"%{param.Search}%"));
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
                        => EF.Functions.Like(x.Name, param.SearchPattern()));

                predicate = predicate.And(x => x.IsStudentInvolvement == false);

                // if(CheckRole.IdRoleGroup == "TCH"){
                //     var datahomeroomteacher = _dbContext.Entity<MsHomeroomTeacher>().Where(x => x.IdBinusian == param.IdUser).FirstOrDefault();
                //     if(datahomeroomteacher == null)
                //         throw new BadRequestException($"Homeroom Teacher not found");
                //     predicate = predicate.And(x => x.EventActivities.Any(a => a.EventActivityAwards.Any(b => b.HomeroomStudent.Homeroom.Id == datahomeroomteacher.IdHomeroom)));
                // }
                
                var dataquery = _dbContext.Entity<TrEvent>()
                    .Include(e => e.EventType)
                    .Include(e => e.EventApprovers)
                    .Include(x => x.EventDetails)
                    .Include(x => x.EventActivities)
                        .ThenInclude(x => x.EventActivityAwards)
                            .ThenInclude(x => x.HomeroomStudent)
                                .ThenInclude(x => x.Student)
                                    .ThenInclude(x => x.StudentParents)
                                        .ThenInclude(x => x.Parent)
                    .Include(x => x.EventActivities)
                        .ThenInclude(x => x.EventActivityAwards)
                            .ThenInclude(x => x.HomeroomStudent)
                                .ThenInclude(x => x.Homeroom)
                    .Where(predicate)
                    .Where(x=>x.AcademicYear.IdSchool == CheckRole.IdSchool);

                var query = dataquery
                .Select(x => new
                {
                    Id = x.Id,
                    EventId = x.Id,
                    EventName = x.Name,
                    StartDate = x.EventDetails.Min(e=>e.StartDate),
                    EndDate = x.EventDetails.Max(e => e.EndDate),
                    ApprovalSatatus = x.StatusEvent == "Approved" ? x.StatusEventAward : x.StatusEvent,
                    ApprovalDescription = x.StatusEvent == "Approved" && x.StatusEventAward == "Approved" ? x.DescriptionEvent : x.StatusEvent == "Approved" && x.StatusEventAward != "Approved" ? x.DescriptionEventAward : x.DescriptionEvent,
                    SortApprovalStatus = (x.StatusEvent.Contains("On Review") || x.StatusEventAward.Contains("On Review")) == true ? 0 : 1,
                    CanApprove = x.StatusEvent.Contains("On Review") || x.StatusEventAward.Contains("On Review") ?
                                    x.StatusEvent == "On Review (1)" ?
                                        x.EventApprovers.Where(y => y.IdUser == param.IdUser && y.OrderNumber == 1).First().IdUser == param.IdUser ?
                                            true : false
                                    : x.StatusEvent == "On Review (2)" ?
                                        x.EventApprovers.Where(y => y.IdUser == param.IdUser && y.OrderNumber == 2).First().IdUser == param.IdUser ?
                                            true : false
                                    : x.StatusEventAward == "On Review (1)" ?
                                        x.EventAwardApprovers.Where(y => y.IdUser == param.IdUser && y.OrderNumber == 1).First().IdUser == param.IdUser ?
                                            true : false
                                    : x.StatusEventAward == "On Review (2)" ?
                                        x.EventAwardApprovers.Where(y => y.IdUser == param.IdUser && y.OrderNumber == 2).First().IdUser == param.IdUser ?
                                            true : false
                                 : true : false,
                    EventType = x.EventType.Description,
                    EventApprover = x.EventApprovers.FirstOrDefault().IdUser,
                    IsShowAcademicCalender  = x.IsShowOnCalendarAcademic
                })
               .OrderBy(x => x.SortApprovalStatus).ThenBy(x => x.StartDate);

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

                // if(param.IdUser != null){
                //     query = query.Where(x => x.EventApprover == param.IdUser);
                // }

                IReadOnlyList<IItemValueVm> items;
                
                var resultApproved = await query
                    .Where(x => x.ApprovalSatatus == "Approved")
                    .ToListAsync(CancellationToken);

                var resultDeclined = await query
                    .Where(x => x.ApprovalSatatus == "Declined")
                    .ToListAsync(CancellationToken);

                var resultWaiting = await query
                    .Where(x => (x.ApprovalSatatus.Contains("On Review")) && x.CanApprove == true)
                    .ToListAsync(CancellationToken);

                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetSchoolEventApprovalResult
                {
                    Id = x.Id,
                    EventId = x.EventId,
                    EventName = x.EventName,
                    EventType = x.EventType,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ApprovalSatatus = x.ApprovalSatatus,
                    ApprovalDescription = x.ApprovalDescription,
                    CanApprove = x.CanApprove,
                    TotalApproved = resultApproved != null ? resultApproved.Count() : 0,
                    TotalDeclined = resultDeclined != null ? resultDeclined.Count() : 0,
                    TotalWaiting = resultWaiting != null ? resultWaiting.Count() : 0,
                    IsShowAcademicCalender  = x.IsShowAcademicCalender
                }).ToList();

                    var count = param.CanCountWithoutFetchDb(items.Count)
                    ? items.Count
                    : await query.Select(x => x.EventId).CountAsync(CancellationToken);

                    return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
            }

            
        }
    }
}
