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
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetListParentPermissionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetListParentPermissionHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListParentPermissionRequest>(nameof(GetListParentPermissionRequest.IdEvent));

            var dataUserEvent = await (from tue in _dbContext.Entity<TrUserEvent>()
                                join u in _dbContext.Entity<MsUser>() on tue.IdUser equals u.Id
                                join ted in _dbContext.Entity<TrEventDetail>() on tue.IdEventDetail equals ted.Id
                                join te in _dbContext.Entity<TrEvent>() on ted.IdEvent equals te.Id
                                join s in _dbContext.Entity<MsStudent>() on tue.IdUser equals s.Id
                                join sg in _dbContext.Entity<MsStudentGrade>() on tue.IdUser equals sg.IdStudent
                                join g in _dbContext.Entity<MsGrade>() on sg.IdGrade equals g.Id
                                join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
                                join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id
                                join h in _dbContext.Entity<MsHomeroom>() on g.Id equals h.IdGrade
                                join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on h.IdGradePathwayClassRoom equals gpc.Id
                                join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
                                where tue.IsNeedApproval == true && te.Id == param.IdEvent
                                
                                select new GetListParentPermissionResult
                                {
                                    StudentId = s.Id,
                                    Username = u.Username,
                                    FullName = s.FirstName + " " + s.LastName,
                                    BinusianId = s.Id,
                                    IdGrade = g.Id,
                                    Grade = g.Description,
                                    IdHomeroom = h.Id,
                                    Homeroom = g.Code + c.Code,
                                    ApprovalStatus = tue.IsNeedApproval == true && tue.IsApproved == false && tue.DateUp == null ? "On Review" : tue.IsNeedApproval == true && tue.IsApproved == false && tue.DateUp != null ? "Declined" : "Approved",
                                    Reason = tue.Reason
                                }).ToListAsync(CancellationToken);

            if (!string.IsNullOrEmpty(param.Search))
                dataUserEvent = dataUserEvent.Where(x => x.BinusianId.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower()) || x.FullName.ToLower().Contains(param.Search.ToLower())).ToList();
                
            if(!string.IsNullOrEmpty(param.IdGrade))
                dataUserEvent = dataUserEvent.Where(x => x.IdGrade == param.IdGrade).ToList();

            if(!string.IsNullOrEmpty(param.IdHomeroom))
                dataUserEvent = dataUserEvent.Where(x => x.IdHomeroom == param.IdHomeroom).ToList();

            if(!string.IsNullOrEmpty(param.ApprovalStatus))
            {
                dataUserEvent = dataUserEvent.Where(x => x.ApprovalStatus == param.ApprovalStatus).ToList();
            }

                switch(param.OrderBy)
                {
                    case "fullname":
                        dataUserEvent = param.OrderType == OrderType.Desc 
                            ? dataUserEvent.OrderByDescending(x => x.FullName).ToList()
                            : dataUserEvent.OrderBy(x => x.FullName).ToList();
                        break;
                    case "binusianid":
                        dataUserEvent = param.OrderType == OrderType.Desc 
                            ? dataUserEvent.OrderByDescending(x => x.BinusianId).ToList()
                            : dataUserEvent.OrderBy(x => x.BinusianId).ToList();
                        break;
                    case "grade":
                        dataUserEvent = param.OrderType == OrderType.Desc 
                            ? dataUserEvent.OrderByDescending(x => x.Grade).ToList()
                            : dataUserEvent.OrderBy(x => x.Grade).ToList();
                        break;
                    case "homeroom":
                        dataUserEvent = param.OrderType == OrderType.Desc 
                            ? dataUserEvent.OrderByDescending(x => x.Homeroom).ToList()
                            : dataUserEvent.OrderBy(x => x.Homeroom).ToList();
                        break;
                };

                var dataUserPagination = dataUserEvent
                .SetPagination(param)
                .Select(x => new GetListParentPermissionResult
                {
                    StudentId = x.StudentId,
                    Username = x.Username,
                    FullName = x.FullName,
                    BinusianId = x.BinusianId,
                    IdGrade = x.IdGrade,
                    Grade = x.Grade,
                    IdHomeroom = x.IdHomeroom,
                    Homeroom = x.Homeroom,
                    ApprovalStatus = x.ApprovalStatus,
                    Reason = x.Reason
                })
                .Distinct()
                .ToList();

                var count = param.CanCountWithoutFetchDb(dataUserPagination.Count) 
                ? dataUserPagination.Count 
                : dataUserEvent.Count;

                return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));

        }
    }
}
