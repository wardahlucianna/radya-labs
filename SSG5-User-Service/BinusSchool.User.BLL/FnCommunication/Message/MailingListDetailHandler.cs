using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Utils;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using System.Text.Encodings.Web;
using System.Net;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.Persistence.UserDb.Entities.Student;
using NPOI.Util;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MailingListDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public MailingListDetailHandler(
            IUserDbContext userDbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = userDbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMailingListDetailRequest>(nameof(GetMailingListDetailRequest.IdMailingList));

            var currentAcademicYear = await _dbContext.Entity<MsPeriod>()
              .Include(x => x.Grade)
                  .ThenInclude(x => x.MsLevel)
                      .ThenInclude(x => x.MsAcademicYear)
              .Where(x => x.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
              .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
              .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
              .Select(x => x.Grade.MsLevel.IdAcademicYear).FirstOrDefaultAsync();

            if (currentAcademicYear == null)
                throw new Exception("this date not contains in list academic year");

            var data = await _dbContext.Entity<MsGroupMailingList>()
                .Include(x => x.User)
                .Select(x => new GetGroupMailingListDetailsResult
                {
                    Id = x.Id,
                    GroupName = x.GroupName,
                    GroupDescripction = x.Description,
                    Description = x.Description,
                    IdUser = x.IdUser,
                    OwnerGroup = x.User.DisplayName,
                    UserName = x.User.Username,
                    CreateBy = x.UserIn,
                    CreateDate = x.DateIn.GetValueOrDefault(),
                    GroupMembers = new List<GroupMember>()
                })
                .FirstOrDefaultAsync(x => x.Id == param.IdMailingList);

            if (data != null)
            {
                var dataMember = await (from a in _dbContext.Entity<MsGroupMailingListMember>()
                                        join u in _dbContext.Entity<MsUser>() on a.IdUser equals u.Id
                                        join r in _dbContext.Entity<MsUserRole>() on u.Id equals r.IdUser
                                        join lr in _dbContext.Entity<LtRole>() on r.IdRole equals lr.Id
                                        join lrg in _dbContext.Entity<LtRoleGroup>() on lr.IdRoleGroup equals lrg.Id
                                        join hs in _dbContext.Entity<MsHomeroomStudent>() on u.Id equals hs.IdStudent
                                        join h in _dbContext.Entity<MsHomeroom>() on hs.IdHomeroom equals h.Id
                                        join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on h.IdGradePathwayClassRoom equals gpc.Id
                                        join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
                                        join gp in _dbContext.Entity<MsGradePathway>() on gpc.IdGradePathway equals gp.Id
                                        join g in _dbContext.Entity<MsGrade>() on gp.IdGrade equals g.Id
                                        join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
                                        join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id
                                        join std in _dbContext.Entity<MsStudent>() on u.Id equals std.Id
                                        where a.IdGroupMailingList == param.IdMailingList
                                        && h.IdAcademicYear == currentAcademicYear
                                        select new GroupMember
                                        {
                                            Id = a.Id,
                                            IdUser = u.Id,
                                            Name = lrg.Id == "STD" ? std.FirstName + " " + std.LastName : u.DisplayName,
                                            UserName = u.Username,
                                            Role = lr.Description,
                                            Level = lrg.Id == "STD" ? l.Description : "-",
                                            Grade = lrg.Id == "STD" ? g.Description : "-",
                                            Homeroom = lrg.Id == "STD" ? g.Code + c.Code : "-",
                                            BinusianID = lrg.Id == "STD" ? std.Id : u.Id,
                                            CreateMessage = a.IsCreateMessage
                                        }).Distinct().ToListAsync(CancellationToken);

                var dataMemberNonStudent = await (from a in _dbContext.Entity<MsGroupMailingListMember>()
                                                     join u in _dbContext.Entity<MsUser>() on a.IdUser equals u.Id
                                                     join r in _dbContext.Entity<MsUserRole>() on u.Id equals r.IdUser
                                                     join lr in _dbContext.Entity<LtRole>() on r.IdRole equals lr.Id
                                                     join lrg in _dbContext.Entity<LtRoleGroup>() on lr.IdRoleGroup equals lrg.Id
                                                     where a.IdGroupMailingList == param.IdMailingList
                                                     select new GroupMember
                                                     {
                                                         Id = a.Id,
                                                         IdUser = u.Id,
                                                         Name = u.DisplayName,
                                                         UserName = u.Username,
                                                         Role = lr.Description,
                                                         Level = "-",
                                                         Grade = "-",
                                                         Homeroom = "-",
                                                         BinusianID = u.Id,                                                         
                                                         CreateMessage = a.IsCreateMessage                                                         
                                                     }).Distinct().ToListAsync(CancellationToken);

                var dataMemberFilter = dataMember.Union(dataMemberNonStudent).AsQueryable();

                dataMemberFilter = dataMemberFilter.GroupBy(x => x.BinusianID).Select(x => x.First()).AsQueryable();
                
                data.GroupMembers = new List<GroupMember>();
                data.GroupMembers = dataMemberFilter
                .Select(x => new GroupMember
                {
                    Id = x.Id,
                    IdUser = x.IdUser,
                    Name = x.Name,
                    UserName = x.UserName,
                    Role = x.Role,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    BinusianID = x.BinusianID,                    
                    CreateMessage = x.CreateMessage
                }).ToList();

                //.Include(x => x.User)
                //.ThenInclude(x => x.UserRoles)
                //.ThenInclude(x => x.Role)
                //.ThenInclude(x => x.RoleGroup)
                //.Where(x => x.IdGroupMailingList == param.IdMailingList)
                //.ToListAsync();

                
                //foreach (var item in dataMember)
                //{
                //    var GroupMember = new GroupMember();
                //    GroupMember.Id = item.Id;
                //    GroupMember.IdUser = item.IdUser;
                //    GroupMember.Name = item.User.DisplayName;
                //    GroupMember.UserName = item.User.Username;
                //    GroupMember.Role = item.User.UserRoles.First().Role.RoleGroup.Description;
                //    GroupMember.CreateMessage = item.IsCreateMessage;

                //    data.GroupMembers.Add(GroupMember);
                //}
            }

            return Request.CreateApiResult2(data as object);
        }
    }
}
