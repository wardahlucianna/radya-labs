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
// using BinusSchool.Persistence.UserDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.User.FnCommunication.Message
{
    public class GetListSentToHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetListSentToHandler(
            IUserDbContext schoolDbContext,
            IMachineDateTime dateTime
        )
        {
            _dbContext = schoolDbContext;
            _dateTime = dateTime;
        }

        private string GetHomeroom(MsHomeroom msHomeroom)
        {

            return msHomeroom.GradePathwayClassroom.MsClassroom.Code;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var result = new GetListSentToResult();

            var param = Request.ValidateParams<GetListSentToRequest>(nameof(GetListSentToRequest.IdMessage));

            result.Id = param.IdMessage;

            var currentAcademicYear = await _dbContext.Entity<MsPeriod>()
              .Include(x => x.Grade)
                  .ThenInclude(x => x.MsLevel)
                      .ThenInclude(x => x.MsAcademicYear)
              .Where(x => x.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
              .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
              .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
              .Select(x => x.Grade.MsLevel.IdAcademicYear).FirstOrDefaultAsync();
            if (currentAcademicYear == null)
                throw new Exception("The master data period is not available. Please contact your system administrator");

            var getMessage = await _dbContext
                                        .Entity<TrMessage>()
                                        .Include(e=>e.MessageFors)
                                        .Include(e=>e.MessageGroupMembers)
                                        .Where(e => e.Id == param.IdMessage)
                                        .FirstOrDefaultAsync(CancellationToken);

            var idUserCreateMessage = getMessage.UserIn;

            var dataRecepient = await (from a in _dbContext.Entity<TrMessageRecepient>()
                                       join u in _dbContext.Entity<MsUser>() on a.IdRecepient equals u.Id
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
                                       where a.IdMessage == param.IdMessage
                                   && h.IdAcademicYear == currentAcademicYear
                                   //&& a.IdRecepient!= idUserCreateMessage
                                       select new ListUserSentTo
                                       {
                                           Id = a.Id,
                                           Role = lr.Description,
                                           Level = lrg.Id == "STD" ? l.Description : "-",
                                           Grade = lrg.Id == "STD" ? g.Description : "-",
                                           Homeroom = lrg.Id == "STD" ? g.Code + c.Code : "-",
                                           BinusianID = lrg.Id == "STD" ? std.Id : u.Id,
                                           Username = u.Username,
                                           FullName = lrg.Id == "STD" ? std.FirstName + " " + std.LastName : u.DisplayName,
                                       }).Distinct().ToListAsync(CancellationToken);

            var dataRecepientNonStudent = await (from a in _dbContext.Entity<TrMessageRecepient>()
                                                 join u in _dbContext.Entity<MsUser>() on a.IdRecepient equals u.Id
                                                 join r in _dbContext.Entity<MsUserRole>() on u.Id equals r.IdUser
                                                 join lr in _dbContext.Entity<LtRole>() on r.IdRole equals lr.Id
                                                 join lrg in _dbContext.Entity<LtRoleGroup>() on lr.IdRoleGroup equals lrg.Id
                                                 where a.IdMessage == param.IdMessage
                                   //&& a.IdRecepient!= idUserCreateMessage
                                                 select new ListUserSentTo
                                                 {
                                                     Id = a.Id,
                                                     Role = lr.Description,
                                                     Level = "-",
                                                     Grade = "-",
                                                     Homeroom = "-",
                                                     BinusianID = u.Id,
                                                     Username = u.Username,
                                                     FullName = u.DisplayName,
                                                 }).Distinct().ToListAsync(CancellationToken);

            if(!getMessage.MessageFors.Any() && !getMessage.MessageGroupMembers.Any())
            {
                dataRecepient = dataRecepient.Where(e=>e.BinusianID==idUserCreateMessage).ToList();
                dataRecepientNonStudent = dataRecepientNonStudent.Where(e=>e.BinusianID==idUserCreateMessage).ToList();
            }

            var dataRecepientFilter = dataRecepient.Union(dataRecepientNonStudent).AsQueryable();

            dataRecepientFilter = dataRecepientFilter.GroupBy(x => x.BinusianID).Select(x => x.First()).AsQueryable();

            switch (param.OrderBy)
            {
                case "role":
                    dataRecepientFilter = param.OrderType == OrderType.Desc
                        ? dataRecepientFilter.OrderByDescending(x => x.Role)
                        : dataRecepientFilter.OrderBy(x => x.Role);
                    break;
                case "level":
                    dataRecepientFilter = param.OrderType == OrderType.Desc
                        ? dataRecepientFilter.OrderByDescending(x => x.Level)
                        : dataRecepientFilter.OrderBy(x => x.Level);
                    break;
                case "grade":
                    dataRecepientFilter = param.OrderType == OrderType.Desc
                        ? dataRecepientFilter.OrderByDescending(x => x.Grade)
                        : dataRecepientFilter.OrderBy(x => x.Grade);
                    break;
                case "homeroom":
                    dataRecepientFilter = param.OrderType == OrderType.Desc
                        ? dataRecepientFilter.OrderByDescending(x => x.Homeroom)
                        : dataRecepientFilter.OrderBy(x => x.Homeroom);
                    break;
                case "binusianid":
                    dataRecepientFilter = param.OrderType == OrderType.Desc
                        ? dataRecepientFilter.OrderByDescending(x => x.BinusianID)
                        : dataRecepientFilter.OrderBy(x => x.BinusianID);
                    break;
                case "username":
                    dataRecepientFilter = param.OrderType == OrderType.Desc
                        ? dataRecepientFilter.OrderByDescending(x => x.Username)
                        : dataRecepientFilter.OrderBy(x => x.Username);
                    break;
                case "fullname":
                    dataRecepientFilter = param.OrderType == OrderType.Desc
                        ? dataRecepientFilter.OrderByDescending(x => x.FullName)
                        : dataRecepientFilter.OrderBy(x => x.FullName);
                    break;
            };

            var dataRecepientFix = dataRecepientFilter
                .Select(x => new ListUserSentTo
                {
                    Id = x.Id,
                    Role = x.Role,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    BinusianID = x.BinusianID,
                    Username = x.Username,
                    FullName = x.FullName,
                }).ToList();

            result.ListUserSentTos = new List<ListUserSentTo>();
            result.ListMemberSentTos = new List<ListMemberSentTo>();

            result.ListUserSentTos.AddRange(dataRecepientFix);

            var getDataGroupMember = _dbContext.Entity<TrMessageGroupMember>()
                .Include(e=>e.GroupMailingList)
                .Where(x => x.IdMessage == param.IdMessage).ToList();

            if (getDataGroupMember.Any())
            {
                //find dataUser From dataRecepientFix
                var listUser = getDataGroupMember.Select(x => x.GroupMailingList.IdUser).ToList();

                result.ListUserSentTos.RemoveAll(p => listUser.Any(y => y == p.Username));
                //End

                var resultGroupIds = getDataGroupMember.Select(o => o.IdGroupMailingList).Distinct().ToList();

                var dataGroupMember = _dbContext.Entity<MsGroupMailingList>()
                    .Where(x => resultGroupIds.Any(y => y == x.Id))
                    .Select(x => new ListMemberSentTo
                    {
                        Id = x.Id,
                        GroupName = x.GroupName,
                        Description = x.Description
                    })
                    .ToList();

                result.ListMemberSentTos.AddRange(dataGroupMember);

            }

            foreach (var item in result.ListMemberSentTos)
            {
                var GroupMember = getDataGroupMember.Where(x => x.IdGroupMailingList == item.Id).Select(x => x.GroupMailingList.IdUser).ToList();
                item.ListGroupMembers = dataRecepientFix.Where(x => GroupMember.Any(y => y == x.Username)).ToList();
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
