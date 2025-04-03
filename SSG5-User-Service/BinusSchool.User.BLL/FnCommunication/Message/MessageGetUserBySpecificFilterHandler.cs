using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Employee.FnStaff;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.Persistence.UserDb.Entities.Student;
using BinusSchool.Persistence.UserDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageGetUserBySpecificFilterHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        public MessageGetUserBySpecificFilterHandler(IUserDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserBySpecificFilterRequest>(nameof(GetUserBySpecificFilterRequest.IdRole));

            var CheckRole = await (from a in _dbContext.Entity<MsUser>()
                                   join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                   join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                   where r.IdRole == param.IdRole

                                   select new GetUserBySpecificFilterResult
                                   {
                                       Role = rg.IdRoleGroup
                                   }).FirstOrDefaultAsync(CancellationToken);

            if (CheckRole.Role == "STD")
            {
                param = Request.ValidateParams<GetUserBySpecificFilterRequest>(nameof(GetUserBySpecificFilterRequest.IdAcademicYear));
                var dataUser = await (from a in _dbContext.Entity<MsHomeroomStudent>()
                                      join s in _dbContext.Entity<MsHomeroom>() on a.IdHomeroom equals s.Id
                                      join sg in _dbContext.Entity<MsStudent>() on a.IdStudent equals sg.Id
                                      join g in _dbContext.Entity<MsGrade>() on s.IdGrade equals g.Id
                                      join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
                                      join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id
                                      join u in _dbContext.Entity<MsUser>() on sg.Id equals u.Id
                                      join ur in _dbContext.Entity<MsUserRole>() on u.Id equals ur.IdUser
                                      join r in _dbContext.Entity<LtRole>() on ur.IdRole equals r.Id
                                      join rg in _dbContext.Entity<LtRoleGroup>() on r.IdRoleGroup equals rg.Id
                                      join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on s.IdGradePathwayClassRoom equals gpc.Id
                                      join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
                                      where ay.Id == param.IdAcademicYear
                                      && r.Id == param.IdRole
                                      select new GetUserBySpecificFilterQueriable
                                      {
                                          Id = u.Id,
                                          DisplayName = string.IsNullOrEmpty(sg.FirstName) ? sg.LastName : $"{sg.FirstName} {sg.LastName}",
                                          Description = u.DisplayName,
                                          Role = r.Description,
                                          IdLevel = l.Id,
                                          Level = l.Description,
                                          IdGrade = g.Id,
                                          Grade = g.Description,
                                          Semester = s.Semester,
                                          IdHomeroom = s.Id,
                                          Homeroom = g.Code + c.Code,
                                          BinusianID = sg.Id,
                                          Username = u.Username
                                      }).ToListAsync(CancellationToken);

                if (!string.IsNullOrEmpty(param.IdLevel))
                    dataUser = dataUser.Where(x => x.IdLevel == param.IdLevel).ToList();

                if (!string.IsNullOrEmpty(param.IdGrade))
                    dataUser = dataUser.Where(x => x.IdGrade == param.IdGrade).ToList();

                if (param.Semester != null)
                    dataUser = dataUser.Where(x => x.Semester.ToString() == param.Semester).ToList();

                if (!string.IsNullOrEmpty(param.IdHomeroom))
                    dataUser = dataUser.Where(x => x.IdHomeroom == param.IdHomeroom).ToList();

                if (!string.IsNullOrEmpty(param.Search))
                    dataUser = dataUser.Where(x => x.DisplayName.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();

                switch (param.OrderBy)
                {
                    case "role":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Role).ToList()
                            : dataUser.OrderBy(x => x.Role).ToList();
                        break;
                    case "level":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Level).ToList()
                            : dataUser.OrderBy(x => x.Level).ToList();
                        break;
                    case "grade":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Grade).ToList()
                            : dataUser.OrderBy(x => x.Grade).ToList();
                        break;
                    case "homeroom":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Homeroom).ToList()
                            : dataUser.OrderBy(x => x.Homeroom).ToList();
                        break;
                    case "binusianId":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.BinusianID).ToList()
                            : dataUser.OrderBy(x => x.BinusianID).ToList();
                        break;
                    case "username":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Username).ToList()
                            : dataUser.OrderBy(x => x.Username).ToList();
                        break;
                    case "fullname":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.DisplayName).ToList()
                            : dataUser.OrderBy(x => x.DisplayName).ToList();
                        break;
                };

                dataUser = dataUser.GroupBy(x => x.BinusianID).Select(x => x.First()).ToList();

                var dataUserPagination = dataUser
                .SetPagination(param)
                .Select(x => new GetUserBySpecificFilterResult
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    Description = x.DisplayName,
                    Role = "Student",
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    BinusianID = x.BinusianID,
                    Username = x.Username
                })
                .ToList();

                var count = param.CanCountWithoutFetchDb(dataUserPagination.Count)
                ? dataUserPagination.Count
                : dataUser.Count;

                return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
            }
            else if (CheckRole.Role == "PRT")
            {
                param = Request.ValidateParams<GetUserBySpecificFilterRequest>(nameof(GetUserBySpecificFilterRequest.IdAcademicYear));

                var DataParent = await (from a in _dbContext.Entity<MsUser>()
                                        join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                        join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                        where r.IdRole == param.IdRole
                                        select new 
                                        {
                                            IdStudent = a.Username.Substring(1),
                                            Id = a.Id,
                                            DisplayName = a.DisplayName,
                                            Description = a.DisplayName,
                                            Role = rg.Description,
                                            Level = "-",
                                            Grade = "-",
                                            Homeroom = "-",
                                            BinusianID = a.Id,
                                            Username = a.Username
                                        }).Distinct().ToListAsync(CancellationToken);

                var DataStudent = await (from a in _dbContext.Entity<MsHomeroomStudent>()
                                         join s in _dbContext.Entity<MsHomeroom>() on a.IdHomeroom equals s.Id
                                         join g in _dbContext.Entity<MsGrade>() on s.IdGrade equals g.Id
                                         join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
                                         join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id
                                         join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on s.IdGradePathwayClassRoom equals gpc.Id
                                         join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
                                         where 
                                         DataParent.Select(e=>e.IdStudent).ToList().Contains(a.IdStudent)
                                         select new 
                                         {
                                             IdStudent = a.IdStudent,
                                             IdLevel = l.Id,
                                             IdAcademicYear = ay.Id,
                                             Level = l.Description,
                                             IdGrade = g.Id,
                                             Grade = g.Description,
                                             Semester = s.Semester,
                                             IdHomeroom = s.Id,
                                             Homeroom = g.Code + c.Code,
                                         }).ToListAsync(CancellationToken);

                var dataUser = (from p in DataParent
                               join s in DataStudent on p.IdStudent equals s.IdStudent
                               select new GetUserBySpecificFilterQueriable
                               {
                                   Id = p.Id,
                                   DisplayName = p.DisplayName,
                                   Description = p.DisplayName,
                                   Role = p.Role,
                                   IdLevel = s.IdLevel,
                                   Level = s.Level,
                                   IdGrade = s.IdGrade,
                                   Grade = s.Grade,
                                   Semester = s.Semester,
                                   IdHomeroom = s.IdHomeroom,
                                   Homeroom = s.Homeroom,
                                   BinusianID = p.BinusianID,
                                   Username = p.Username,
                                   IdAcademicYear = s.IdAcademicYear
                               });

                var coba = dataUser.ToList();

                if (!string.IsNullOrEmpty(param.IdAcademicYear))
                    dataUser = dataUser.Where(x => x.IdAcademicYear == param.IdAcademicYear).ToList();

                if (!string.IsNullOrEmpty(param.IdLevel))
                    dataUser = dataUser.Where(x => x.IdLevel == param.IdLevel).ToList();

                if (!string.IsNullOrEmpty(param.IdGrade))
                    dataUser = dataUser.Where(x => x.IdGrade == param.IdGrade).ToList();

                if (param.Semester != null)
                    dataUser = dataUser.Where(x => x.Semester.ToString() == param.Semester).ToList();

                if (!string.IsNullOrEmpty(param.IdHomeroom))
                    dataUser = dataUser.Where(x => x.IdHomeroom == param.IdHomeroom).ToList();

                if (!string.IsNullOrEmpty(param.Search))
                    dataUser = dataUser.Where(x => x.DisplayName.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();

                switch (param.OrderBy)
                {
                    case "role":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Role).ToList()
                            : dataUser.OrderBy(x => x.Role).ToList();
                        break;
                    case "level":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Level).ToList()
                            : dataUser.OrderBy(x => x.Level).ToList();
                        break;
                    case "grade":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Grade).ToList()
                            : dataUser.OrderBy(x => x.Grade).ToList();
                        break;
                    case "homeroom":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Homeroom).ToList()
                            : dataUser.OrderBy(x => x.Homeroom).ToList();
                        break;
                    case "binusianId":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.BinusianID).ToList()
                            : dataUser.OrderBy(x => x.BinusianID).ToList();
                        break;
                    case "username":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Username).ToList()
                            : dataUser.OrderBy(x => x.Username).ToList();
                        break;
                    case "fullname":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.DisplayName).ToList()
                            : dataUser.OrderBy(x => x.DisplayName).ToList();
                        break;
                };

                dataUser = dataUser.GroupBy(x => x.BinusianID).Select(x => x.First()).ToList();

                var dataUserPagination = dataUser
                .SetPagination(param)
                .Select(x => new GetUserBySpecificFilterResult
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    Description = x.DisplayName,
                    Role = "Parent",
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    BinusianID = x.BinusianID,
                    Username = x.Username
                })
                .ToList();

                var count = param.CanCountWithoutFetchDb(dataUserPagination.Count())
                ? dataUserPagination.Count()
                : dataUser.Count();

                return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
            }
            else
            {
                var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                      join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                      join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                      where r.IdRole == param.IdRole
                                      select new GetUserBySpecificFilterResult
                                      {
                                          Id = a.Id,
                                          DisplayName = a.DisplayName,
                                          Description = a.DisplayName,
                                          Role = rg.Description,
                                          Level = "-",
                                          Grade = "-",
                                          Homeroom = "-",
                                          BinusianID = a.Id,
                                          Username = a.Username
                                      }).ToListAsync(CancellationToken);


                if (!string.IsNullOrEmpty(param.Search))
                    dataUser = dataUser.Where(x => x.DisplayName.ToLower().Contains(param.Search.ToLower()) || x.BinusianID.ToLower().Contains(param.Search.ToLower()) || x.Username.ToLower().Contains(param.Search.ToLower())).ToList();

                switch (param.OrderBy)
                {
                    case "role":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Role).ToList()
                            : dataUser.OrderBy(x => x.Role).ToList();
                        break;
                    case "level":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Level).ToList()
                            : dataUser.OrderBy(x => x.Level).ToList();
                        break;
                    case "grade":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Grade).ToList()
                            : dataUser.OrderBy(x => x.Grade).ToList();
                        break;
                    case "homeroom":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Homeroom).ToList()
                            : dataUser.OrderBy(x => x.Homeroom).ToList();
                        break;
                    case "binusianId":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.BinusianID).ToList()
                            : dataUser.OrderBy(x => x.BinusianID).ToList();
                        break;
                    case "username":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.Username).ToList()
                            : dataUser.OrderBy(x => x.Username).ToList();
                        break;
                    case "fullname":
                        dataUser = param.OrderType == OrderType.Desc
                            ? dataUser.OrderByDescending(x => x.DisplayName).ToList()
                            : dataUser.OrderBy(x => x.DisplayName).ToList();
                        break;
                };

                var dataUserPagination = dataUser
                .SetPagination(param)
                .Select(x => new GetUserBySpecificFilterResult
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    Description = x.DisplayName,
                    Role = x.Role,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    BinusianID = x.BinusianID,
                    Username = x.Username
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
