using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.PersonalInvitation.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.PersonalInvitation
{
    public class PersonalInvitationApprovalHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public PersonalInvitationApprovalHandler(ISchedulingDbContext schedulingDbContext, IMachineDateTime dateTime)
        {
            _dbContext = schedulingDbContext;
            _dateTime = dateTime;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {


            return Request.CreateApiResult2(); ;
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetPersonalInvitationApprovalRequest>();
            var predicate = PredicateBuilder.Create<TrPersonalInvitation>(x => x.IsActive);
            List<string> _columns = new List<string>();

            if (!string.IsNullOrEmpty(param.DateInvitation.ToString()))
                predicate = predicate.And(x => x.InvitationDate.Date == Convert.ToDateTime(param.DateInvitation).Date);

            if (RoleConstant.Parent == param.Role)
            {
                _columns.Add("TeacherName");
                _columns.Add("InvitationDate");
                _columns.Add("StartTime");
                _columns.Add("EndTime");
                _columns.Add("Description");
                _columns.Add("Status");
                predicate = predicate.And(x => x.IdStudent == param.IdStudent && x.IdUserInvitation!=param.IdUser);
            }
            else if (RoleConstant.Teacher == param.Role)
            {
                _columns.Add("StudentName");
                _columns.Add("BinusianId");
                _columns.Add("InvitationDate");
                _columns.Add("StartTime");
                _columns.Add("EndTime");
                _columns.Add("Description");
                _columns.Add("Status");
                predicate = predicate.And(x => x.IdUserTeacher == param.IdUser && x.IdUserInvitation != param.IdUser);
            }

            var query = _dbContext.Entity<TrPersonalInvitation>()
                        .Include(e => e.Student)
                        .Include(e=>e.UserTeacher)
                        .Where(predicate)
                        .Select(x => new
                        {
                            Id = x.Id,
                            StudentName = x.Student.FirstName + (x.Student.MiddleName == null ? "" : " " + x.Student.MiddleName) + (x.Student.LastName == null ? "" : " " + x.Student.LastName),
                            BinusianId = x.Student.Id,
                            InvitationDate = x.InvitationDate,
                            StartTime = x.InvitationStartTime,
                            EndTime = x.InvitationEndTime,
                            Description = x.Description,
                            Status = x.Status.GetDescription(),
                            TeacherName = x.UserTeacher.DisplayName
                        }); ;

            //Search

            if (RoleConstant.Parent == param.Role)
            {
                if (!string.IsNullOrEmpty(param.Search))
                    query = query.Where(x => x.TeacherName.ToLower().Contains(param.Search.ToLower()));
            }
            else if (RoleConstant.Teacher == param.Role)
            {
                if (!string.IsNullOrEmpty(param.Search))
                    query = query.Where(x => x.StudentName.ToLower().Contains(param.Search.ToLower()));
            }
            

            //ordering
            switch (param.OrderBy)
            {
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;
                case "BinusanId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BinusianId)
                        : query.OrderBy(x => x.BinusianId);
                    break;
                case "InvitationDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.InvitationDate)
                        : query.OrderBy(x => x.InvitationDate);
                    break;
                case "StartTime":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StartTime)
                        : query.OrderBy(x => x.StartTime);
                    break;
                case "EndTime":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EndTime)
                        : query.OrderBy(x => x.EndTime);
                    break;
                case "Description":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Description)
                        : query.OrderBy(x => x.Description);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
                case "TeacherName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.TeacherName)
                        : query.OrderBy(x => x.TeacherName);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetPersonalInvitationApprovalResult
                {
                    Id = x.Id,
                    StudentName = x.StudentName,
                    InvitationDate = x.InvitationDate.ToString("dd MMMM yyyy"),
                    BinusanId = x.BinusianId,
                    StartTime = x.StartTime.ToString(@"hh\:mm"),
                    EndTime = x.EndTime.ToString(@"hh\:mm"),
                    Description = x.Description,
                    Status = x.Status,
                    TeacherName = x.TeacherName,
                    DisableButton = x.Status== "On Request"?false:true,
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetPersonalInvitationApprovalResult
                {
                    Id = x.Id,
                    StudentName = x.StudentName,
                    InvitationDate = x.InvitationDate.ToString("dd MMMM yyyy"),
                    BinusanId = x.BinusianId,
                    StartTime = x.StartTime.ToString(@"hh\:mm"),
                    EndTime = x.EndTime.ToString(@"hh\:mm"),
                    Description = x.Description,
                    Status = x.Status,
                    TeacherName = x.TeacherName,
                    DisableButton = x.Status == "On Request" ? false : true,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<UpdatePersonalInvitationApprovalRequest, UpdatePersonalInvitationApprovalValidator>();

            var GetPersonalInvitation = await _dbContext.Entity<TrPersonalInvitation>()
                        .Where(e => e.Id == body.Id)
                        .FirstOrDefaultAsync(CancellationToken);

            if (GetPersonalInvitation==null)
            {
                throw new BadRequestException($"IdPersonalInvitation : {body.Id} is not found");
            }

            if (body.IsApproval)
            {
                if (RoleConstant.Teacher == body.Role)
                {
                    GetPersonalInvitation.IdVenue = body.IdVanue;
                    GetPersonalInvitation.Status = PersonalInvitationStatus.Approved;
                    GetPersonalInvitation.DateApproval = _dateTime.ServerTime;
                }
                else if (RoleConstant.Parent == body.Role)
                {
                    GetPersonalInvitation.Status = PersonalInvitationStatus.Approved;
                    GetPersonalInvitation.DateApproval = _dateTime.ServerTime;
                }
            }
            else
            {
                if (RoleConstant.Teacher == body.Role)
                {
                    GetPersonalInvitation.Status = PersonalInvitationStatus.Declined;
                    GetPersonalInvitation.DateApproval = _dateTime.ServerTime;
                    GetPersonalInvitation.DeclineReason = body.Note;
                    GetPersonalInvitation.AvailabilityDate = Convert.ToDateTime(body.StartDateAvailability).Date;
                    GetPersonalInvitation.AvailabilityStartTime = Convert.ToDateTime(body.StartDateAvailability).TimeOfDay;
                    GetPersonalInvitation.AvailabilityEndTime = Convert.ToDateTime(body.EndDateAvailability).TimeOfDay;
                }
                else if (RoleConstant.Parent == body.Role)
                {
                    GetPersonalInvitation.Status = PersonalInvitationStatus.Declined;
                    GetPersonalInvitation.DateApproval = _dateTime.ServerTime;
                    GetPersonalInvitation.DeclineReason = body.Note;
                }
            }

            _dbContext.Entity<TrPersonalInvitation>().Update(GetPersonalInvitation);

            var cekStatus = await _dbContext.Entity<TrPersonalInvitation>()
                        .Where(e => e.Id == body.Id)
                        .Select(e=>e.Status)
                        .FirstOrDefaultAsync(CancellationToken);

            if(cekStatus== PersonalInvitationStatus.Declined || cekStatus == PersonalInvitationStatus.Approved)
                throw new BadRequestException($"the personal invitation already approved/declined");

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
