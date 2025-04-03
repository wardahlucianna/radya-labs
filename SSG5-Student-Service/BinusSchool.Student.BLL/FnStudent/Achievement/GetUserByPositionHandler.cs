using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Common.Exceptions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnStudent;

namespace BinusSchool.Student.FnStudent.Achievement
{
    public class GetUserByPositionHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetUserByPositionHandler(IStudentDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserByPositionRequest>();

            var checkPosition = await _dbContext.Entity<LtPosition>()
                .Where(x => x.Code == param.CodePosition)
                .FirstOrDefaultAsync(CancellationToken);

            if (checkPosition == null)
                throw new BadRequestException($"Position code {param.CodePosition} not found");

            if (checkPosition.Code == "CA" || checkPosition.Code == "COT")
            {

                var dataUser = await (from ht in _dbContext.Entity<MsHomeroomTeacher>()
                                      join h in _dbContext.Entity<MsHomeroom>() on ht.IdHomeroom equals h.Id
                                      join g in _dbContext.Entity<MsGrade>() on h.IdGrade equals g.Id
                                      join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
                                      join tp in _dbContext.Entity<MsTeacherPosition>() on ht.IdTeacherPosition equals tp.Id
                                      join p in _dbContext.Entity<LtPosition>() on tp.IdPosition equals p.Id
                                      join u in _dbContext.Entity<MsUser>() on ht.IdBinusian equals u.Id
                                      join ur in _dbContext.Entity<MsUserRole>() on u.Id equals ur.IdUser
                                      join s in _dbContext.Entity<MsStaff>() on ht.IdBinusian equals s.IdBinusian
                                      where
                                          l.IdAcademicYear == param.IdAcademicYear && p.Code == checkPosition.Code
                                      select new GetUserByPositionResult
                                      {
                                          Id = u.Id,
                                          Fullname = s.FirstName + (!string.IsNullOrEmpty(s.LastName) ? " " + s.LastName : ""),
                                          BinusianID = s.IdBinusian,
                                          Username = u.Username,
                                          Email = u.Email,
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
                .Select(x => new GetUserByPositionResult
                {
                    Id = x.Id,
                    Fullname = x.Fullname,
                    BinusianID = x.BinusianID,
                    Username = x.Username,
                    Email = x.Email,
                })
                .ToList();

                var count = param.CanCountWithoutFetchDb(dataUserPagination.Count)
                ? dataUserPagination.Count
                : dataUser.Count;

                return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
            }
            else if (checkPosition.Code == "ST")
            {

                var dataUser = await (from lt in _dbContext.Entity<MsLessonTeacher>()
                                      join l in _dbContext.Entity<MsLesson>() on lt.IdLesson equals l.Id
                                      join u in _dbContext.Entity<MsUser>() on lt.IdUser equals u.Id
                                      join ur in _dbContext.Entity<MsUserRole>() on u.Id equals ur.IdUser
                                      join s in _dbContext.Entity<MsStaff>() on lt.IdUser equals s.IdBinusian
                                      where
                                          l.IdAcademicYear == param.IdAcademicYear
                                      select new GetUserByPositionResult
                                      {
                                          Id = u.Id,
                                          Fullname = u.DisplayName,
                                          BinusianID = u.Id,
                                          Username = u.Username,
                                          Email = u.Email,
                                      })
                                .Distinct()
                                .ToListAsync(CancellationToken);

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

                dataUser.GroupBy(x => x.BinusianID).ToList();

                var dataUserPagination = dataUser
                .SetPagination(param)
                .Select(x => new GetUserByPositionResult
                {
                    Id = x.Id,
                    Fullname = x.Fullname,
                    BinusianID = x.BinusianID,
                    Username = x.Username,
                    Email = x.Email,
                })
                .ToList();

                var count = param.CanCountWithoutFetchDb(dataUserPagination.Count)
                ? dataUserPagination.Count
                : dataUser.Count;

                return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
            }
            else
            {

                if (param.CodePosition != null)
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
                                                  mntl.IdAcademicYear == param.IdAcademicYear && p.Code == checkPosition.Code
                                              select new GetUserByPositionResult
                                              {
                                                  Id = u.Id,
                                                  Fullname = u.DisplayName,
                                                  BinusianID = u.Id,
                                                  Username = u.Username,
                                                  Email = u.Email,
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
                        .Select(x => new GetUserByPositionResult
                        {
                            Id = x.Id,
                            Fullname = x.Fullname,
                            BinusianID = x.BinusianID,
                            Username = x.Username,
                            Email = x.Email,
                        })
                        .Distinct().ToList();

                        var count = param.CanCountWithoutFetchDb(dataUserPagination.Count)
                        ? dataUserPagination.Count
                        : dataUser.Count;

                        return Request.CreateApiResult2(dataUserPagination as object, param.CreatePaginationProperty(count));
                    }
                    else
                        return Request.CreateApiResult2();
                }
                else
                    return Request.CreateApiResult2();
            }

        }
    }
}
