using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.PersonalInvitation.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace BinusSchool.Scheduling.FnSchedule.PersonalInvitation
{
    public class PersonalInvitationHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IServiceProvider _provider;

        public PersonalInvitationHandler(ISchedulingDbContext schedulingDbContext, IMachineDateTime dateTime, IServiceProvider provider)
        {
            _dbContext = schedulingDbContext;
            _dateTime = dateTime;
            _provider = provider;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetPersonalInvitation = await _dbContext.Entity<TrPersonalInvitation>()
               .Where(x => ids.Contains(x.Id))
               .ToListAsync(CancellationToken);

            GetPersonalInvitation.ForEach(x => x.Status = PersonalInvitationStatus.Cancelled);
            _dbContext.Entity<TrPersonalInvitation>().UpdateRange(GetPersonalInvitation);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var items = await _dbContext.Entity<TrPersonalInvitation>()
                .Include(e => e.Student)
                .Include(e => e.UserTeacher)
               .Where(e => e.Id == id)
              .Select(x => new DetailPersonalInvitationResult
              {
                  Id = x.Id,
                  IdAcademicYear = x.IdAcademicYear,
                  IdStudent = x.IdStudent,
                  IdUserTeacher = x.IdUserTeacher,
                  StudentName = x.Student.Id + " - " + x.Student.FirstName + (x.Student.MiddleName == null ? "" : " " + x.Student.MiddleName) + (x.Student.LastName == null ? "" : " " + x.Student.LastName),
                  TeacherName = x.UserTeacher.DisplayName,
                  Description = x.Description,
                  InvitationType = x.InvitationType.HasValue ? x.InvitationType.Value.GetDescription() : null,
                  MakeAppointmentWithIsStudent = x.IsStudent,
                  MakeAppointmentWithIsFather = x.IsFather,
                  MakeAppointmentWithIsMother = x.IsMother,
                  MakeAppointmentWithIsBothParent = x.IsFather && x.IsMother ? true : false,
                  SendInvitationTo = GetSendInvitationTo(x.IsFather, x.IsMother, x.IsStudent),
                  IsNotifParent = x.IsNotifParent,
                  InvitationStartDate = (x.InvitationDate + x.InvitationStartTime),
                  InvitationEndDate = (x.InvitationDate + x.InvitationEndTime),
                  IsDisabledCancel = x.Status == PersonalInvitationStatus.Approved ? false : true,
                  IsDisabledReschedule = x.Status == PersonalInvitationStatus.Approved ? false : true,
                  Status = x.Status.GetDescription(),
                  Reason = x.DeclineReason,
                  AvailabilityDate = x.AvailabilityDate,
                  AvailabilityStartTime = x.AvailabilityStartTime,
                  AvailabilityEndTime = x.AvailabilityEndTime
              }).SingleOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetPersonalInvitationRequest>();
            List<string> _columns = new List<string>();
            var predicate = PredicateBuilder.Create<TrPersonalInvitation>(x => x.IsActive == true);

            if (!string.IsNullOrEmpty(param.DateInvitation.ToString()))
                predicate = predicate.And(x => x.InvitationDate.Date == Convert.ToDateTime(param.DateInvitation).Date);

            var UpdatePersonalInvitationParent = await _dbContext.Entity<TrPersonalInvitation>()
                  .Where(e => _dateTime.ServerTime >= e.InvitationDate && e.Status == PersonalInvitationStatus.OnRequest)
                .ToListAsync(CancellationToken);

            if (UpdatePersonalInvitationParent.Any())
            {
                UpdatePersonalInvitationParent.ForEach(e => e.Status = PersonalInvitationStatus.NoResponse);
                _dbContext.Entity<TrPersonalInvitation>().UpdateRange(UpdatePersonalInvitationParent);
                await _dbContext.SaveChangesAsync(CancellationToken);
            }

            if (RoleConstant.Parent == param.Role)
            {
                _columns.Add("InvitationDate");
                _columns.Add("TeacherName");
                _columns.Add("BinusianId");
                _columns.Add("StartTime");
                _columns.Add("EndTime");
                _columns.Add("Description");
                _columns.Add("Status");
                predicate = predicate.And(x => x.IdStudent == param.IdStudent && x.IdUserInvitation == param.IdUser);

                if (!string.IsNullOrEmpty(param.Search))
                    predicate = predicate.And(x => x.UserTeacher.Id.ToLower().Contains(param.Search.ToLower()) || x.UserTeacher.DisplayName.ToLower().Contains(param.Search.ToLower()));
            }
            else if (RoleConstant.Staff == param.Role || RoleConstant.Teacher == param.Role)
            {

                _columns.Add("StudentName");
                _columns.Add("SendInvitationTo");
                _columns.Add("InvitationType");
                _columns.Add("InvitationDate");
                _columns.Add("StartTime");
                _columns.Add("EndTime");
                _columns.Add("Description");
                _columns.Add("Status");
                predicate = predicate.And(x => x.IdUserInvitation == param.IdUser);
            }
            else if (RoleConstant.Student == param.Role)
            {

                _columns.Add("TeacherName");
                _columns.Add("InvitationDate");
                _columns.Add("StartTime");
                _columns.Add("EndTime");
                _columns.Add("Description");

                predicate = predicate.And(x => x.IdStudent == param.IdUser && x.IsStudent);
            }

            var GetPersonalInvitation = await _dbContext.Entity<TrPersonalInvitation>()
                    .Include(e => e.UserInvitation)
                    .Include(e => e.Student)
                  .Where(predicate)
                  .Select(x => new
                  {
                      Id = x.Id,
                      InvitationDate = x.InvitationDate,
                      TeacherName = x.UserTeacher.DisplayName,
                      BinusianId = x.UserTeacher.Id,
                      StartTime = x.InvitationStartTime,
                      EndTime = x.InvitationEndTime,
                      Description = x.Description,
                      Status = x.Status.GetDescription(),
                      StudentName = x.Student.FirstName + (x.Student.MiddleName == null ? "" : " " + x.Student.MiddleName) + (x.Student.LastName == null ? "" : " " + x.Student.LastName),
                      SendInvitationTo = GetSendInvitationTo(x.IsFather, x.IsMother, x.IsStudent),
                      InvitationType = x.InvitationType.HasValue ? x.InvitationType.Value.GetDescription() : null,
                  })
                .ToListAsync(CancellationToken);

            var query = GetPersonalInvitation.Distinct();

            if (!string.IsNullOrEmpty(param.Status))
            {
                string statusDescription = param.Status;
                if (param.Status == PersonalInvitationStatus.OnRequest.ToString()
                    || param.Status == PersonalInvitationStatus.NoResponse.ToString()
                    || param.Status == PersonalInvitationStatus.NoApproval.ToString())
                {
                    Enum.TryParse(param.Status, out PersonalInvitationStatus myStatus);
                    statusDescription = myStatus.GetDescription();
                }

                query = query.Where(x => x.Status == statusDescription);
            }

            if (!string.IsNullOrEmpty(param.TypeInvitation))
                query = query.Where(x => x.InvitationType == param.TypeInvitation);

            if (!string.IsNullOrEmpty(param.Search))
            {
                if (RoleConstant.Staff == param.Role || RoleConstant.Teacher == param.Role)
                    query = query.Where(x => x.StudentName.ToLower().Contains(param.Search.ToLower()) || x.BinusianId.Contains(param.Search.ToLower()));
                else
                    query = query.Where(x => x.TeacherName.ToLower().Contains(param.Search.ToLower()) || x.BinusianId.Contains(param.Search.ToLower()));
            }

            //ordering
            switch (param.OrderBy)
            {
                case "InvitationDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.InvitationDate)
                        : query.OrderBy(x => x.InvitationDate);
                    break;
                case "TeacherName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.TeacherName)
                        : query.OrderBy(x => x.TeacherName);
                    break;
                case "BinusianId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BinusianId)
                        : query.OrderBy(x => x.BinusianId);
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
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;
                case "SendInvitationTo":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.SendInvitationTo)
                        : query.OrderBy(x => x.SendInvitationTo);
                    break;
                case "InvitationType":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.InvitationType)
                        : query.OrderBy(x => x.InvitationType);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();

                items = result.Select(x => new GetPersonalInvitationResult
                {
                    Id = x.Id,
                    InvitationDate = x.InvitationDate.ToString("dd MMMM yyyy"),
                    TeacherName = x.TeacherName,
                    BinusianId = x.BinusianId,
                    StartTime = x.StartTime.ToString(@"hh\:mm"),
                    EndTime = x.EndTime.ToString(@"hh\:mm"),
                    Description = x.Description,
                    Status = x.Status,
                    StudentName = x.StudentName,
                    SendInvitationTo = x.SendInvitationTo,
                    InvitationType = x.InvitationType,
                }).ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.Select(x => new GetPersonalInvitationResult
                {
                    Id = x.Id,
                    InvitationDate = x.InvitationDate.ToString("dd MMMM yyyy"),
                    TeacherName = x.TeacherName,
                    BinusianId = x.BinusianId,
                    StartTime = x.StartTime.ToString(@"hh\:mm"),
                    EndTime = x.EndTime.ToString(@"hh\:mm"),
                    Description = x.Description,
                    Status = x.Status,
                    StudentName = x.StudentName,
                    SendInvitationTo = x.SendInvitationTo,
                    InvitationType = x.InvitationType,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        public static string GetSendInvitationTo(bool isFather, bool isMother, bool isStudent)
        {
            List<string> listSendInvitationTo = new List<string>();

            if (isFather)
                listSendInvitationTo.Add("Father");
            if (isMother)
                listSendInvitationTo.Add("Mother");
            if (isStudent)
                listSendInvitationTo.Add("Student");

            return string.Join(",", listSendInvitationTo);
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddPersonalInvitationRequest, AddPersonalInvitationValidator>();

            var GetInvitationBookingSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                        .Include(e => e.InvitationBookingSetting)
                        .Where(e => e.InvitationBookingSetting.IdAcademicYear == body.IdAcademicYear
                                    && e.IdUserTeacher == body.IdUserTeacher
                                    && (e.IsPriority == true || e.IsFlexibleBreak == true || e.IsFixedBreak == true)
                                    && e.DateInvitation.Date == body.InvitationDate.Date)
                        .ToListAsync(CancellationToken);

            var invitationStartTime = TimeSpan.Parse(body.InvitationStartTime);
            var invitationEndTime = TimeSpan.Parse(body.InvitationEndTime);

            if (RoleConstant.Parent == body.Role)
            {
                var interval = TimeSpan.Parse(body.InvitationEndTime) - TimeSpan.Parse(body.InvitationStartTime);
                if (interval.TotalMinutes < 30)
                    throw new BadRequestException("Interval minimum is 30 minutes");

                #region avalible time
                var param = new GetAvailabilityTimeTeacherRequest
                {
                    IdAcademicYear = body.IdAcademicYear,
                    DateInvitation = body.InvitationDate,
                    IdUserTeacher = body.IdUserTeacher
                };

                List<AvailabilityTime> AvailabilityTeacher = new List<AvailabilityTime>();
                using (var scope = _provider.CreateScope())
                {
                    AvailabilityTeacher = await GetAvailabilityTimeTeacherV2Handler.GetAvailabilityTimeTeacher(param, _dbContext, CancellationToken);
                }

                TimeSpan invStartTime = TimeSpan.Parse(body.InvitationStartTime);
                TimeSpan invEndTime = TimeSpan.Parse(body.InvitationEndTime);
                var Exsis = AvailabilityTeacher.Where(e => ((invStartTime >= e.StartTime && invStartTime < e.EndTime) && (invEndTime > e.StartTime && invEndTime <= e.EndTime))).Any();
                if (!Exsis)
                    throw new BadRequestException("You have another invitation at the same time");
                #endregion

                var AddPersonalInvitation = new TrPersonalInvitation
                {
                    Id = Guid.NewGuid().ToString(),
                    IdAcademicYear = body.IdAcademicYear,
                    IdUserInvitation = body.IdUser,
                    IdStudent = body.IdStudent,
                    IdUserTeacher = body.IdUserTeacher,
                    InvitationType = body.InvitationType,
                    InvitationDate = body.InvitationDate.Date,
                    InvitationStartTime = TimeSpan.Parse(body.InvitationStartTime),
                    InvitationEndTime = TimeSpan.Parse(body.InvitationEndTime),
                    Status = body.SendInvitationIsStudent ? PersonalInvitationStatus.NoApproval : PersonalInvitationStatus.OnRequest,
                    IsFather = false,
                    IsMother = false,
                    IsStudent = false,
                    IsAvailable = GetInvitationBookingSchedule
                        .Where(e => (e.StartTime >= TimeSpan.Parse(body.InvitationStartTime) && e.StartTime <= TimeSpan.Parse(body.InvitationEndTime))
                                    || (e.EndTime >= TimeSpan.Parse(body.InvitationStartTime) && e.EndTime <= TimeSpan.Parse(body.InvitationEndTime)))
                    .Any(),
                    Description = body.Description,
                };

                _dbContext.Entity<TrPersonalInvitation>().Add(AddPersonalInvitation);
            }
            else if (RoleConstant.Staff == body.Role)
            {
                var AddPersonalInvitation = new TrPersonalInvitation
                {
                    Id = Guid.NewGuid().ToString(),
                    IdAcademicYear = body.IdAcademicYear,
                    IdUserInvitation = body.IdUser,
                    IdStudent = body.IdStudent,
                    IdUserTeacher = body.IdUserTeacher,
                    InvitationType = body.InvitationType,
                    InvitationDate = body.InvitationDate.Date,
                    InvitationStartTime = TimeSpan.Parse(body.InvitationStartTime),
                    InvitationEndTime = TimeSpan.Parse(body.InvitationEndTime),
                    Status = body.SendInvitationIsStudent ? PersonalInvitationStatus.NoApproval : PersonalInvitationStatus.OnRequest,
                    IsFather = body.SendInvitationIsBothParent == true ? true : body.SendInvitationIsFather,
                    IsMother = body.SendInvitationIsMother == true ? true : body.SendInvitationIsMother,
                    IsStudent = body.SendInvitationIsStudent,
                    IsAvailable = GetInvitationBookingSchedule
                        .Where(e => (e.StartTime >= TimeSpan.Parse(body.InvitationStartTime) && e.StartTime <= TimeSpan.Parse(body.InvitationEndTime))
                                    || (e.EndTime >= TimeSpan.Parse(body.InvitationStartTime) && e.EndTime <= TimeSpan.Parse(body.InvitationEndTime)))
                    .Any(),
                    Description = body.Description,
                };

                _dbContext.Entity<TrPersonalInvitation>().Add(AddPersonalInvitation);
            }
            else
            {
                var AddPersonalInvitation = new TrPersonalInvitation
                {
                    Id = Guid.NewGuid().ToString(),
                    IdAcademicYear = body.IdAcademicYear,
                    IdUserInvitation = body.IdUser,
                    IdStudent = body.IdStudent,
                    IdUserTeacher = body.IdUser,
                    InvitationType = body.InvitationType,
                    InvitationDate = body.InvitationDate.Date,
                    InvitationStartTime = TimeSpan.Parse(body.InvitationStartTime),
                    InvitationEndTime = TimeSpan.Parse(body.InvitationEndTime),
                    Status = body.SendInvitationIsStudent ? PersonalInvitationStatus.NoApproval : PersonalInvitationStatus.OnRequest,
                    IsFather = body.SendInvitationIsBothParent == true ? true : body.SendInvitationIsFather,
                    IsMother = body.SendInvitationIsMother == true ? true : body.SendInvitationIsMother,
                    IsStudent = body.SendInvitationIsStudent,
                    IsAvailable = GetInvitationBookingSchedule
                        .Where(e => (e.StartTime >= TimeSpan.Parse(body.InvitationStartTime) && e.StartTime <= TimeSpan.Parse(body.InvitationEndTime))
                                    || (e.EndTime >= TimeSpan.Parse(body.InvitationStartTime) && e.EndTime <= TimeSpan.Parse(body.InvitationEndTime)))
                    .Any(),
                    Description = body.Description,
                };

                _dbContext.Entity<TrPersonalInvitation>().Add(AddPersonalInvitation);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdatePersonalInvitationRequest, UpdatePersonalInvitationValidator>();

            var GetInvitationBooking = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                        .Include(e => e.InvitationBookingSetting)
                        .Where(e => e.InvitationBookingSetting.IdAcademicYear == body.IdAcademicYear
                                    && e.IdUserTeacher == body.IdUserTeacher
                                    && (e.IsPriority == true || e.IsFlexibleBreak == true || e.IsFixedBreak == true)
                                    && e.DateInvitation.Date == body.InvitationDate.Date)
                        .ToListAsync(CancellationToken);

            var UpdatePersonalInvitation = await _dbContext.Entity<TrPersonalInvitation>()
                        .Where(e => e.Id == body.Id)
                        .FirstOrDefaultAsync(CancellationToken);

            #region avalible time
            var param = new GetAvailabilityTimeTeacherRequest
            {
                IdAcademicYear = body.IdAcademicYear,
                DateInvitation = body.InvitationDate,
                IdUserTeacher = body.IdUserTeacher
            };

            List<AvailabilityTime> AvailabilityTeacher = new List<AvailabilityTime>();
            using (var scope = _provider.CreateScope())
            {
                AvailabilityTeacher = await GetAvailabilityTimeTeacherV2Handler.GetAvailabilityTimeTeacher(param, _dbContext, CancellationToken, body.Id);
            }

            TimeSpan invStartTime = TimeSpan.Parse(body.InvitationStartTime);
            TimeSpan invEndTime = TimeSpan.Parse(body.InvitationEndTime);
            var Exsis = AvailabilityTeacher.Where(e => ((invStartTime >= e.StartTime && invStartTime < e.EndTime) && (invEndTime > e.StartTime && invEndTime <= e.EndTime))).Any();
            if (!Exsis)
                throw new BadRequestException("You have another invitation at the same time");
            #endregion

            if (RoleConstant.Parent == body.Role)
            {
                UpdatePersonalInvitation.InvitationType = body.InvitationType;
                UpdatePersonalInvitation.IsNotifParent = body.IsNotifParent;
                UpdatePersonalInvitation.InvitationDate = body.InvitationDate;
                UpdatePersonalInvitation.IsStudent = body.SendInvitationIsStudent;
                UpdatePersonalInvitation.IsFather = body.SendInvitationIsFather;
                UpdatePersonalInvitation.IsMother = body.SendInvitationIsMother;
                UpdatePersonalInvitation.InvitationStartTime = TimeSpan.Parse(body.InvitationStartTime);
                UpdatePersonalInvitation.InvitationEndTime = TimeSpan.Parse(body.InvitationEndTime);
                UpdatePersonalInvitation.Description = body.Description;
                UpdatePersonalInvitation.IsAvailable = GetInvitationBooking
                        .Where(e => (e.StartTime >= TimeSpan.Parse(body.InvitationStartTime) && e.StartTime <= TimeSpan.Parse(body.InvitationEndTime))
                                    || (e.EndTime >= TimeSpan.Parse(body.InvitationStartTime) && e.EndTime <= TimeSpan.Parse(body.InvitationEndTime)))
                    .Any();
                UpdatePersonalInvitation.Status = PersonalInvitationStatus.OnRequest;
                UpdatePersonalInvitation.Description = body.Description;
                _dbContext.Entity<TrPersonalInvitation>().Update(UpdatePersonalInvitation);
            }
            else if (RoleConstant.Staff == body.Role || RoleConstant.Teacher == body.Role)
            {
                UpdatePersonalInvitation.InvitationType = body.InvitationType;
                UpdatePersonalInvitation.IsNotifParent = body.IsNotifParent;
                UpdatePersonalInvitation.InvitationDate = body.InvitationDate;
                UpdatePersonalInvitation.IsStudent = body.SendInvitationIsStudent;
                UpdatePersonalInvitation.IsFather = body.SendInvitationIsBothParent == true ? true : body.SendInvitationIsFather;
                UpdatePersonalInvitation.IsMother = body.SendInvitationIsBothParent == true ? true : body.SendInvitationIsMother;
                UpdatePersonalInvitation.InvitationStartTime = TimeSpan.Parse(body.InvitationStartTime);
                UpdatePersonalInvitation.InvitationEndTime = TimeSpan.Parse(body.InvitationEndTime);
                UpdatePersonalInvitation.IsAvailable = GetInvitationBooking
                       .Where(e => (e.StartTime >= TimeSpan.Parse(body.InvitationStartTime) && e.StartTime <= TimeSpan.Parse(body.InvitationEndTime))
                                   || (e.EndTime >= TimeSpan.Parse(body.InvitationStartTime) && e.EndTime <= TimeSpan.Parse(body.InvitationEndTime)))
                   .Any();
                UpdatePersonalInvitation.Status = PersonalInvitationStatus.OnRequest;
                UpdatePersonalInvitation.Description = body.Description;
                _dbContext.Entity<TrPersonalInvitation>().Update(UpdatePersonalInvitation);
            }

            var GetInvitationBookingSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                    .Include(e => e.InvitationBookingSetting)
                    .Where(e => e.InvitationBookingSetting.IdAcademicYear == body.IdAcademicYear
                                && e.IdUserTeacher == body.IdUserTeacher
                                && (e.IsPriority == true || e.IsFlexibleBreak == true || e.IsFixedBreak == true)
                                && e.DateInvitation.Date == body.InvitationDate.Date)
                    .ToListAsync(CancellationToken);

            var invitationStartTime = TimeSpan.Parse(body.InvitationStartTime);
            var invitationEndTime = TimeSpan.Parse(body.InvitationEndTime);

            var existStudentInvitationSameTime = await _dbContext.Entity<TrPersonalInvitation>()
                .Where(e => e.IdAcademicYear == body.IdAcademicYear
                       && e.IdStudent == body.IdStudent
                       && e.InvitationDate == body.InvitationDate
                       && (e.Status == PersonalInvitationStatus.Approved || e.Status == PersonalInvitationStatus.OnRequest || e.Status == PersonalInvitationStatus.NoApproval)
                       && ((e.InvitationStartTime >= invitationStartTime && e.InvitationStartTime <= invitationEndTime) || (e.InvitationEndTime > invitationStartTime && e.InvitationEndTime <= invitationEndTime))
                       && e.Id != body.Id)
                .AnyAsync(CancellationToken);

            if (existStudentInvitationSameTime)
            {
                if (body.Role == RoleConstant.Parent)
                    throw new BadRequestException("You have another personal invitation at the same time");
                else
                    throw new BadRequestException("This student have another personal invitation at the same time");
            }

            var exsisUserInvitationSameTime = await _dbContext.Entity<TrPersonalInvitation>()
                       .Where(e => e.IdAcademicYear == body.IdAcademicYear
                                   && e.IdUserInvitation == body.IdUser
                                   && e.InvitationDate == body.InvitationDate
                                   && (e.Status == PersonalInvitationStatus.Approved || e.Status == PersonalInvitationStatus.OnRequest || e.Status == PersonalInvitationStatus.NoApproval)
                                   && ((e.InvitationStartTime >= invitationStartTime && e.InvitationStartTime <= invitationEndTime) || (e.InvitationEndTime > invitationStartTime && e.InvitationEndTime <= invitationEndTime))
                                   && e.Id != body.Id)
                       .AnyAsync(CancellationToken);

            if (exsisUserInvitationSameTime)
                throw new BadRequestException("You have another personal invitation at the same time");

            if (body.Role == RoleConstant.Parent)
            {
                var exsisTeacherUserInvitationSameTime = await _dbContext.Entity<TrPersonalInvitation>()
                                                    .Where(e => e.IdAcademicYear == body.IdAcademicYear
                                                        && e.IdUserTeacher == body.IdUserTeacher
                                                        && e.InvitationDate == body.InvitationDate
                                                        && (e.Status == PersonalInvitationStatus.Approved || e.Status == PersonalInvitationStatus.OnRequest || e.Status == PersonalInvitationStatus.NoApproval)
                                                        && ((e.InvitationStartTime >= invitationStartTime && e.InvitationStartTime <= invitationEndTime) || (e.InvitationEndTime > invitationStartTime && e.InvitationEndTime <= invitationEndTime))
                                                        && e.Id != body.Id)
                                                            .AnyAsync(CancellationToken);

                if (exsisTeacherUserInvitationSameTime)
                    throw new BadRequestException("the teacher already booking");
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
