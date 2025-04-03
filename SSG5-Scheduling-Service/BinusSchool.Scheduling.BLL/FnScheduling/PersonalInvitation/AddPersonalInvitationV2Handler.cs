using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnSchedule.PersonalInvitation.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Scheduling.FnSchedule.PersonalInvitation
{
    public class AddPersonalInvitationV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IServiceProvider _provider;

        public AddPersonalInvitationV2Handler(ISchedulingDbContext dbContext, IServiceProvider provider)
        {
            _dbContext = dbContext;
            _provider = provider;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddPersonalInvitationRequest, AddPersonalInvitationValidator>();

            var GetInvitationBookingSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                    .Include(e => e.InvitationBookingSetting)
                    .Where(e => e.InvitationBookingSetting.IdAcademicYear == body.IdAcademicYear
                                && e.IdUserTeacher == body.IdUserTeacher
                                && (e.IsPriority == true || e.IsFlexibleBreak == true || e.IsFixedBreak == true)
                                && e.DateInvitation.Date == body.InvitationDate.Date)
                    .ToListAsync(CancellationToken);

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

            if (RoleConstant.Parent == body.Role)
            {
                var interval = TimeSpan.Parse(body.InvitationEndTime) - TimeSpan.Parse(body.InvitationStartTime);
                if (interval.TotalMinutes < 30)
                    throw new BadRequestException("Interval minimum is 30 minutes");

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
                    Status = body.SendInvitationIsBothParent || body.SendInvitationIsMother || body.SendInvitationIsFather
                                    ? PersonalInvitationStatus.OnRequest : PersonalInvitationStatus.NoApproval,
                    IsFather = body.SendInvitationIsBothParent == true ? true : body.SendInvitationIsFather,
                    IsMother = body.SendInvitationIsBothParent == true ? true : body.SendInvitationIsMother,
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
                    Status = body.SendInvitationIsBothParent || body.SendInvitationIsMother || body.SendInvitationIsFather
                                    ? PersonalInvitationStatus.OnRequest : PersonalInvitationStatus.NoApproval,
                    IsFather = body.SendInvitationIsBothParent == true ? true : body.SendInvitationIsFather,
                    IsMother = body.SendInvitationIsBothParent == true ? true : body.SendInvitationIsMother,
                    IsStudent = body.SendInvitationIsStudent,
                    IsAvailable = GetInvitationBookingSchedule
                        .Where(e => (e.StartTime >= TimeSpan.Parse(body.InvitationStartTime) && e.StartTime <= TimeSpan.Parse(body.InvitationEndTime))
                                    || (e.EndTime >= TimeSpan.Parse(body.InvitationStartTime) && e.EndTime <= TimeSpan.Parse(body.InvitationEndTime)))
                    .Any(),
                    Description = body.Description,
                };

                _dbContext.Entity<TrPersonalInvitation>().Add(AddPersonalInvitation);
            }

            var invitationStartTime = TimeSpan.Parse(body.InvitationStartTime);
            var invitationEndTime = TimeSpan.Parse(body.InvitationEndTime);

            var idSibling = await _dbContext.Entity<MsSiblingGroup>()
                                .Where(e => e.IdStudent == body.IdStudent)
                                .Select(e => e.Id)
                                .FirstOrDefaultAsync(CancellationToken);

            var listIdStudent = await _dbContext.Entity<MsSiblingGroup>()
                                .Where(e => e.Id == idSibling)
                                .Select(e => e.IdStudent)
                                .ToListAsync(CancellationToken);

            var exsisUserInvitationSameTime = await _dbContext.Entity<TrPersonalInvitation>()
                       .Where(e => e.IdAcademicYear == body.IdAcademicYear
                                   && listIdStudent.Contains(e.IdStudent)
                                   && e.InvitationDate == body.InvitationDate
                                   && (e.Status == PersonalInvitationStatus.Approved || e.Status == PersonalInvitationStatus.OnRequest || e.Status == PersonalInvitationStatus.NoApproval)
                                   && ((e.InvitationStartTime <= invitationStartTime && e.InvitationEndTime >= invitationStartTime)
                                        || (e.InvitationStartTime <= invitationEndTime && e.InvitationEndTime >= invitationEndTime))
                                )
                       .AnyAsync(CancellationToken);

            if (exsisUserInvitationSameTime)
                throw new BadRequestException("You have another personal invitation at the same time");

            if (body.Role == RoleConstant.Parent)
            {
                var exsisTeacherUserInvitationSameTime = await _dbContext.Entity<TrPersonalInvitation>()
                                                            .Where(e => e.IdAcademicYear == body.IdAcademicYear
                                                            && e.IdUserTeacher == body.IdUserTeacher
                                                            && e.InvitationDate.Date == body.InvitationDate.Date
                                                            && (e.Status == PersonalInvitationStatus.Approved || e.Status == PersonalInvitationStatus.OnRequest || e.Status == PersonalInvitationStatus.NoApproval)
                                                            && ((e.InvitationStartTime >= invitationStartTime && e.InvitationStartTime <= invitationEndTime)
                                                                || (e.InvitationEndTime > invitationStartTime && e.InvitationEndTime < invitationEndTime))
                                                                    )
                                                            .AnyAsync(CancellationToken);

                if (exsisTeacherUserInvitationSameTime)
                    throw new BadRequestException("the teacher already booking");
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
