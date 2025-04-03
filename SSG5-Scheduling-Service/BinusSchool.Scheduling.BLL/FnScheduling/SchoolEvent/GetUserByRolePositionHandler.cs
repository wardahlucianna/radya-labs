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
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolsEvent;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetUserByRolePositionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetUserByRolePositionHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserByRolePositionRequest>(nameof(GetUserByRolePositionRequest.IdRole), nameof(GetUserByRolePositionRequest.IdAcademicYear));

            var CheckRole = await (from a in _dbContext.Entity<MsUser>()
                                   join us in _dbContext.Entity<MsUserSchool>() on a.Id equals us.IdUser
                                   join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                   join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                   where r.IdRole == param.IdRole
                                    
                                  select new
                                  {
                                      IdSchool = us.IdSchool,
                                      IdRole = rg.Id,
                                      IdRoleGroup = rg.IdRoleGroup
                                  }).FirstOrDefaultAsync(CancellationToken);

            if(CheckRole == null)
                throw new BadRequestException($"User in this role not found");

            if(CheckRole.IdRoleGroup == "STD")
            {
                var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                   join us in _dbContext.Entity<MsUserSchool>() on a.Id equals us.IdUser
                                   join s in _dbContext.Entity<MsStudent>() on a.Id equals s.Id
                                   where us.IdSchool == CheckRole.IdSchool
                                  select new GetUserByRolePositionResult
                                  {
                                      Id = a.Id,
                                      Fullname = s.FirstName + (!string.IsNullOrEmpty(s.MiddleName)?" "+ s.MiddleName : "") + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                      BinusianID = s.Id,
                                      Username = a.Username,
                                      Email = a.Email
                                  }).ToListAsync(CancellationToken);

                if (!string.IsNullOrEmpty(param.Search))
                    dataUser = dataUser.Where(x => x.Fullname.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();
                
                switch(param.OrderBy)
                {
                    case "fullname":
                        dataUser = param.OrderType == OrderType.Desc 
                            ? dataUser.OrderByDescending(x => x.Fullname).ToList()
                            : dataUser.OrderBy(x => x.Fullname).ToList();
                        break;
                    case "binusianid":
                        dataUser = param.OrderType == OrderType.Desc 
                            ? dataUser.OrderByDescending(x => x.BinusianID).ToList()
                            : dataUser.OrderBy(x => x.BinusianID).ToList();
                        break;
                    case "username":
                        dataUser = param.OrderType == OrderType.Desc 
                            ? dataUser.OrderByDescending(x => x.Username).ToList()
                            : dataUser.OrderBy(x => x.Username).ToList();
                        break;
                };

                var dataUserPagination = dataUser
                .SetPagination(param)
                .Select(x => new GetUserByRolePositionResult
                {
                    Id = x.Id,
                    Fullname = x.Fullname,
                    BinusianID = x.BinusianID,
                    Username = x.Username,
                    Email = x.Email
                })
                .ToList();

                var count = param.CanCountWithoutFetchDb(dataUserPagination.Count) 
                ? dataUserPagination.Count 
                : dataUser.Count;

                return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
            }
            else if(CheckRole.IdRoleGroup == "STF")
            {
                var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                   join ur in _dbContext.Entity<MsUserRole>() on a.Id equals ur.IdUser
                                   join us in _dbContext.Entity<MsUserSchool>() on a.Id equals us.IdUser
                                   join s in _dbContext.Entity<MsStaff>() on a.Id equals s.IdBinusian
                                   where ur.IdRole == CheckRole.IdRole && us.IdSchool == CheckRole.IdSchool
                                  select new GetUserByRolePositionResult
                                  {
                                      Id = a.Id,
                                      Fullname = s.FirstName + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                      BinusianID = s.IdBinusian,
                                      Username = a.Username,
                                      Email = a.Email,
                                      Contact = s.MobilePhoneNumber1
                                  }).ToListAsync(CancellationToken);

                if (!string.IsNullOrEmpty(param.Search))
                    dataUser = dataUser.Where(x => x.Fullname.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();
                
                switch(param.OrderBy)
                {
                    case "fullname":
                        dataUser = param.OrderType == OrderType.Desc 
                            ? dataUser.OrderByDescending(x => x.Fullname).ToList()
                            : dataUser.OrderBy(x => x.Fullname).ToList();
                        break;
                    case "binusianid":
                        dataUser = param.OrderType == OrderType.Desc 
                            ? dataUser.OrderByDescending(x => x.BinusianID).ToList()
                            : dataUser.OrderBy(x => x.BinusianID).ToList();
                        break;
                    case "username":
                        dataUser = param.OrderType == OrderType.Desc 
                            ? dataUser.OrderByDescending(x => x.Username).ToList()
                            : dataUser.OrderBy(x => x.Username).ToList();
                        break;
                };

                var dataUserPagination = dataUser
                .SetPagination(param)
                .Select(x => new GetUserByRolePositionResult
                {
                    Id = x.Id,
                    Fullname = x.Fullname,
                    BinusianID = x.BinusianID,
                    Username = x.Username,
                    Email = x.Email,
                    Contact = x.Contact
                })
                .ToList();

                var count = param.CanCountWithoutFetchDb(dataUserPagination.Count) 
                ? dataUserPagination.Count 
                : dataUser.Count;

                return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
            }
            else
            {
                if(param.CodePosition == null)
                {
                    var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                          join ur in _dbContext.Entity<MsUserRole>() on a.Id equals ur.IdUser
                                          join s in _dbContext.Entity<MsStaff>() on a.Id equals s.IdBinusian
                                          where ur.IdRole == CheckRole.IdRole
                                          select new GetUserByRolePositionResult
                                          {
                                              Id = a.Id,
                                              Fullname = s.FirstName + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                              BinusianID = s.IdBinusian,
                                              Username = a.Username,
                                              Email = a.Email,
                                              Contact = s.MobilePhoneNumber1
                                          }).Distinct().ToListAsync(CancellationToken);

                    if (!string.IsNullOrEmpty(param.Search))
                        dataUser = dataUser.Where(x => x.Fullname.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();

                    switch (param.OrderBy)
                    {
                        case "fullname":
                            dataUser = param.OrderType == OrderType.Desc
                                ? dataUser.OrderByDescending(x => x.Fullname).ToList()
                                : dataUser.OrderBy(x => x.Fullname).ToList();
                            break;
                        case "binusianid":
                            dataUser = param.OrderType == OrderType.Desc
                                ? dataUser.OrderByDescending(x => x.BinusianID).ToList()
                                : dataUser.OrderBy(x => x.BinusianID).ToList();
                            break;
                        case "username":
                            dataUser = param.OrderType == OrderType.Desc
                                ? dataUser.OrderByDescending(x => x.Username).ToList()
                                : dataUser.OrderBy(x => x.Username).ToList();
                            break;
                    };

                    var dataUserPagination = dataUser
                    .SetPagination(param)
                    .Select(x => new GetUserByRolePositionResult
                    {
                        Id = x.Id,
                        Fullname = x.Fullname,
                        BinusianID = x.BinusianID,
                        Username = x.Username,
                        Email = x.Email,
                        Contact = x.Contact
                    })
                    .ToList();

                    var count = param.CanCountWithoutFetchDb(dataUserPagination.Count)
                    ? dataUserPagination.Count
                    : dataUser.Count;

                    return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
                }

                param = Request.ValidateParams<GetUserByRolePositionRequest>(nameof(GetUserByRolePositionRequest.CodePosition));
                
                var checkPosition = await _dbContext.Entity<LtPosition>()
                .Where(x => x.Id == param.CodePosition)
                .FirstOrDefaultAsync(CancellationToken);
                if (checkPosition == null)
                    throw new BadRequestException($"Position code {param.CodePosition} not found");

                if(checkPosition.Code == "CA" || checkPosition.Code == "COT"){

                    var dataUser = await (from ht in _dbContext.Entity<MsHomeroomTeacher>()
                                    join h in _dbContext.Entity<MsHomeroom>() on ht.IdHomeroom equals h.Id
                                    join tp in _dbContext.Entity<MsTeacherPosition>() on ht.IdTeacherPosition equals tp.Id
                                    join p in _dbContext.Entity<LtPosition>() on tp.IdPosition equals p.Id
                                    join u in _dbContext.Entity<MsUser>() on ht.IdBinusian equals u.Id
                                    join ur in _dbContext.Entity<MsUserRole>() on u.Id equals ur.IdUser
                                    join s in _dbContext.Entity<MsStaff>() on ht.IdBinusian equals s.IdBinusian
                                    where
                                        h.IdAcademicYear == param.IdAcademicYear && p.Code == checkPosition.Code && ur.IdRole == param.IdRole
                                    select new GetUserByRolePositionResult
                                    {
                                        Id = u.Id,
                                        Fullname = s.FirstName +  (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                        BinusianID = s.IdBinusian,
                                        Username = u.Username,
                                        Email = u.Email,
                                        Contact = s.MobilePhoneNumber1
                                    }).Distinct().ToListAsync(CancellationToken);

                    if (!string.IsNullOrEmpty(param.Search))
                    dataUser = dataUser.Where(x => x.Fullname.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();
                
                    switch(param.OrderBy)
                    {
                        case "fullname":
                            dataUser = param.OrderType == OrderType.Desc 
                                ? dataUser.OrderByDescending(x => x.Fullname).ToList()
                                : dataUser.OrderBy(x => x.Fullname).ToList();
                            break;
                        case "binusianid":
                            dataUser = param.OrderType == OrderType.Desc 
                                ? dataUser.OrderByDescending(x => x.BinusianID).ToList()
                                : dataUser.OrderBy(x => x.BinusianID).ToList();
                            break;
                        case "username":
                            dataUser = param.OrderType == OrderType.Desc 
                                ? dataUser.OrderByDescending(x => x.Username).ToList()
                                : dataUser.OrderBy(x => x.Username).ToList();
                            break;
                    };

                    var dataUserPagination = dataUser
                    .SetPagination(param)
                    .Select(x => new GetUserByRolePositionResult
                    {
                        Id = x.Id,
                        Fullname = x.Fullname,
                        BinusianID = x.BinusianID,
                        Username = x.Username,
                        Email = x.Email,
                        Contact = x.Contact
                    })
                    .ToList();

                    var count = param.CanCountWithoutFetchDb(dataUserPagination.Count) 
                    ? dataUserPagination.Count 
                    : dataUser.Count;

                    return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
                }else if(checkPosition.Code == "ST"){

                    var dataUser = await (from lt in _dbContext.Entity<MsLessonTeacher>()
                                    join l in _dbContext.Entity<MsLesson>() on lt.IdLesson equals l.Id
                                    join u in _dbContext.Entity<MsUser>() on lt.IdUser equals u.Id
                                    join ur in _dbContext.Entity<MsUserRole>() on u.Id equals ur.IdUser
                                    join s in _dbContext.Entity<MsStaff>() on lt.IdUser equals s.IdBinusian
                                    where
                                        l.IdAcademicYear == param.IdAcademicYear && ur.IdRole == param.IdRole
                                    select new GetUserByRolePositionResult
                                    {
                                        Id = u.Id,
                                        Fullname = u.DisplayName,
                                        BinusianID = u.Id,
                                        Username = u.Username,
                                        Email = u.Email,
                                        Contact = s.MobilePhoneNumber1
                                    })
                                    .Distinct()
                                    .ToListAsync(CancellationToken);

                    if (!string.IsNullOrEmpty(param.Search))
                    dataUser = dataUser.Where(x => x.Fullname.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();
                
                    switch(param.OrderBy)
                    {
                        case "fullname":
                            dataUser = param.OrderType == OrderType.Desc 
                                ? dataUser.OrderByDescending(x => x.Fullname).ToList()
                                : dataUser.OrderBy(x => x.Fullname).ToList();
                            break;
                        case "binusianid":
                            dataUser = param.OrderType == OrderType.Desc 
                                ? dataUser.OrderByDescending(x => x.BinusianID).ToList()
                                : dataUser.OrderBy(x => x.BinusianID).ToList();
                            break;
                        case "username":
                            dataUser = param.OrderType == OrderType.Desc 
                                ? dataUser.OrderByDescending(x => x.Username).ToList()
                                : dataUser.OrderBy(x => x.Username).ToList();
                            break;
                    };

                    dataUser.GroupBy(x => x.BinusianID).ToList();

                    var dataUserPagination = dataUser
                    .SetPagination(param)
                    .Select(x => new GetUserByRolePositionResult
                    {
                        Id = x.Id,
                        Fullname = x.Fullname,
                        BinusianID = x.BinusianID,
                        Username = x.Username,
                        Email = x.Email,
                        Contact = x.Contact
                    })
                    .ToList();

                    var count = param.CanCountWithoutFetchDb(dataUserPagination.Count) 
                    ? dataUserPagination.Count 
                    : dataUser.Count;

                    return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
                }else{

                    if(param.CodePosition != null)
                    {
                        if (checkPosition.Code != "ADM")
                        {
                            var dataUser = await (from ntl in _dbContext.Entity<TrNonTeachingLoad>()
                                                  join mntl in _dbContext.Entity<MsNonTeachingLoad>() on ntl.IdMsNonTeachingLoad equals mntl.Id
                                                  join tp in _dbContext.Entity<MsTeacherPosition>() on mntl.IdTeacherPosition equals tp.Id
                                                  join p in _dbContext.Entity<LtPosition>() on tp.IdPosition equals p.Id
                                                  join u in _dbContext.Entity<MsUser>() on ntl.IdUser equals u.Id
                                                  join ur in _dbContext.Entity<MsUserRole>() on u.Id equals ur.IdUser
                                                  join s in _dbContext.Entity<MsStaff>() on u.Id equals s.IdBinusian
                                                  where
                                                      mntl.IdAcademicYear == param.IdAcademicYear && p.Code == checkPosition.Code && ur.IdRole == param.IdRole
                                                  select new GetUserByRolePositionResult
                                                  {
                                                      Id = u.Id,
                                                      Fullname = u.DisplayName,
                                                      BinusianID = u.Id,
                                                      Username = u.Username,
                                                      Email = u.Email,
                                                      Contact = s.MobilePhoneNumber1
                                                  }).Distinct().ToListAsync(CancellationToken);

                            if (!string.IsNullOrEmpty(param.Search))
                                dataUser = dataUser.Where(x => x.Fullname.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();

                            switch (param.OrderBy)
                            {
                                case "fullname":
                                    dataUser = param.OrderType == OrderType.Desc
                                        ? dataUser.OrderByDescending(x => x.Fullname).ToList()
                                        : dataUser.OrderBy(x => x.Fullname).ToList();
                                    break;
                                case "binusianid":
                                    dataUser = param.OrderType == OrderType.Desc
                                        ? dataUser.OrderByDescending(x => x.BinusianID).ToList()
                                        : dataUser.OrderBy(x => x.BinusianID).ToList();
                                    break;
                                case "username":
                                    dataUser = param.OrderType == OrderType.Desc
                                        ? dataUser.OrderByDescending(x => x.Username).ToList()
                                        : dataUser.OrderBy(x => x.Username).ToList();
                                    break;
                            };

                            var dataUserPagination = dataUser
                            .SetPagination(param)
                            .Select(x => new GetUserByRolePositionResult
                            {
                                Id = x.Id,
                                Fullname = x.Fullname,
                                BinusianID = x.BinusianID,
                                Username = x.Username,
                                Email = x.Email,
                                Contact = x.Contact
                            })
                            .Distinct().ToList();

                            var count = param.CanCountWithoutFetchDb(dataUserPagination.Count)
                            ? dataUserPagination.Count
                            : dataUser.Count;

                            return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
                        }
                        else
                        {
                            var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                                  join ur in _dbContext.Entity<MsUserRole>() on a.Id equals ur.IdUser
                                                  join s in _dbContext.Entity<MsStaff>() on a.Id equals s.IdBinusian
                                                  where ur.IdRole == CheckRole.IdRole
                                                  select new GetUserByRolePositionResult
                                                  {
                                                      Id = a.Id,
                                                      Fullname = s.FirstName + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                                      BinusianID = s.IdBinusian,
                                                      Username = a.Username,
                                                      Email = a.Email,
                                                      Contact = s.MobilePhoneNumber1
                                                  }).Distinct().ToListAsync(CancellationToken);

                            if (!string.IsNullOrEmpty(param.Search))
                                dataUser = dataUser.Where(x => x.Fullname.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();

                            switch (param.OrderBy)
                            {
                                case "fullname":
                                    dataUser = param.OrderType == OrderType.Desc
                                        ? dataUser.OrderByDescending(x => x.Fullname).ToList()
                                        : dataUser.OrderBy(x => x.Fullname).ToList();
                                    break;
                                case "binusianid":
                                    dataUser = param.OrderType == OrderType.Desc
                                        ? dataUser.OrderByDescending(x => x.BinusianID).ToList()
                                        : dataUser.OrderBy(x => x.BinusianID).ToList();
                                    break;
                                case "username":
                                    dataUser = param.OrderType == OrderType.Desc
                                        ? dataUser.OrderByDescending(x => x.Username).ToList()
                                        : dataUser.OrderBy(x => x.Username).ToList();
                                    break;
                            };

                            var dataUserPagination = dataUser
                            .SetPagination(param)
                            .Select(x => new GetUserByRolePositionResult
                            {
                                Id = x.Id,
                                Fullname = x.Fullname,
                                BinusianID = x.BinusianID,
                                Username = x.Username,
                                Email = x.Email,
                                Contact = x.Contact
                            })
                            .ToList();

                            var count = param.CanCountWithoutFetchDb(dataUserPagination.Count)
                            ? dataUserPagination.Count
                            : dataUser.Count;

                            return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
                        }
                    }
                    else
                    {
                        var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                   join ur in _dbContext.Entity<MsUserRole>() on a.Id equals ur.IdUser
                                   join s in _dbContext.Entity<MsStaff>() on a.Id equals s.IdBinusian
                                   where ur.IdRole == CheckRole.IdRole
                                  select new GetUserByRolePositionResult
                                  {
                                      Id = a.Id,
                                      Fullname = s.FirstName + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                      BinusianID = s.IdBinusian,
                                      Username = a.Username,
                                      Email = a.Email,
                                      Contact = s.MobilePhoneNumber1
                                  }).Distinct().ToListAsync(CancellationToken);

                        if (!string.IsNullOrEmpty(param.Search))
                            dataUser = dataUser.Where(x => x.Fullname.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();
                        
                        switch(param.OrderBy)
                        {
                            case "fullname":
                                dataUser = param.OrderType == OrderType.Desc 
                                    ? dataUser.OrderByDescending(x => x.Fullname).ToList()
                                    : dataUser.OrderBy(x => x.Fullname).ToList();
                                break;
                            case "binusianid":
                                dataUser = param.OrderType == OrderType.Desc 
                                    ? dataUser.OrderByDescending(x => x.BinusianID).ToList()
                                    : dataUser.OrderBy(x => x.BinusianID).ToList();
                                break;
                            case "username":
                                dataUser = param.OrderType == OrderType.Desc 
                                    ? dataUser.OrderByDescending(x => x.Username).ToList()
                                    : dataUser.OrderBy(x => x.Username).ToList();
                                break;
                        };

                        var dataUserPagination = dataUser
                        .SetPagination(param)
                        .Select(x => new GetUserByRolePositionResult
                        {
                            Id = x.Id,
                            Fullname = x.Fullname,
                            BinusianID = x.BinusianID,
                            Username = x.Username,
                            Email = x.Email,
                            Contact = x.Contact
                        })
                        .ToList();

                        var count = param.CanCountWithoutFetchDb(dataUserPagination.Count) 
                        ? dataUserPagination.Count 
                        : dataUser.Count;

                        return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
                    }
                }
                
            }

        }
    }
}
