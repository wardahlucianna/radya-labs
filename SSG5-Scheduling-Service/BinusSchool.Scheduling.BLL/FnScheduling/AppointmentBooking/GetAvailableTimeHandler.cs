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
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetAvailableTimeHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = { "StartTime" };
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _datetime;

        public GetAvailableTimeHandler(ISchedulingDbContext SchedulingDbContext, IMachineDateTime datetime)
        {
            _dbContext = SchedulingDbContext;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAvailableTimeRequest>();
            var predicate = PredicateBuilder.Create<TrInvitationBookingSettingSchedule>(x => x.IsActive==true);
            predicate = predicate.And(x => 
                        x.IdInvitationBookingSetting == param.IdInvitationBookingSetting
                        && x.IdUserTeacher == param.IdUserTeacher 
                        && x.DateInvitation.Date == param.AppointmentDate.Date
                        && (x.IsPriority==true || x.IsPriority==null) 
                        && x.IsFixedBreak == false && x.QuotaSlot>0
                        );



            var IdSibling = await _dbContext.Entity<MsSiblingGroup>()
                            .Where(e=>e.IdStudent==param.IdStudent)
                            .Select(e=>e.Id)
                            .FirstOrDefaultAsync(CancellationToken);

            var GetStudent = await _dbContext.Entity<MsSiblingGroup>()
                            .Where(e => e.Id == IdSibling)
                            .Select(e => e.IdStudent)
                            .ToListAsync(CancellationToken);

            var InvitationBookingByStudent = await _dbContext.Entity<TrInvitationBookingDetail>()
                .Include(e => e.InvitationBooking)
                .Include(e => e.HomeroomStudent)
                .Where(e =>
                            // e.InvitationBooking.IdInvitationBookingSetting==param.IdInvitationBookingSetting
                            GetStudent.Contains(e.HomeroomStudent.IdStudent)
                           && e.InvitationBooking.StartDateInvitation.Date == param.AppointmentDate.Date
                        )
                .Select(e => new
                {
                    e.InvitationBooking,
                    e.IdHomeroomStudent,
                })
                .Distinct().ToListAsync(CancellationToken);

            //var siblingIdHomeroomStudent = InvitationBookingByStudent.Select(e=>e.IdHomeroomStudent).ToList();

            var GetHomeroomStudentVenue = await _dbContext.Entity<TrInvitationBookingSettingUser>()
              .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Venue)
              .Where(e => e.IdInvitationBookingSetting == param.IdInvitationBookingSetting && e.HomeroomStudent.IdStudent == param.IdStudent)
              .Select(e => e.HomeroomStudent.Homeroom.Venue)
              .FirstOrDefaultAsync(CancellationToken);

            var InvitationBooking = await _dbContext.Entity<TrInvitationBooking>()
               .Where(e => e.IdInvitationBookingSetting == param.IdInvitationBookingSetting
                           && e.IdUserTeacher == param.IdUserTeacher
                            && e.StartDateInvitation.Date == param.AppointmentDate.Date
                           )
               .Distinct().ToListAsync(CancellationToken);

            var GetInvitationBookingSettingSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                .Include(x => x.InvitationBookingSetting).ThenInclude(x => x.AcademicYears)
                .Include(x => x.Venue)
                .Where(e=> !InvitationBookingByStudent.Select(f=>f.InvitationBooking.StartDateInvitation.TimeOfDay).ToList().Contains(e.StartTime))
                .Where(predicate)
                .ToListAsync(CancellationToken);

            var ListInvitationBookingSettingSchedule = GetInvitationBookingSettingSchedule.Select(x => new
                {
                    IdInvitationBookingSettingSchedule = x.Id,
                    StartTimeInvitation = x.StartTime,
                    EndTimeInvitation = x.EndTime,
                    IdVenue = x.Venue.Description == "Respective Classroom"? GetHomeroomStudentVenue.Id : x.IdVenue,
                    Venue = x.Venue.Description == "Respective Classroom"? GetHomeroomStudentVenue.Description : x.Venue.Description,
                    IsFull = InvitationBooking.Where(e => e.StartDateInvitation.TimeOfDay == x.StartTime).Count() >= x.QuotaSlot ? true : false,
                })
                .OrderBy(e=>e.StartTimeInvitation).ToList();

            var items = new GetAvailableTimeResult
            {
                IdVenue = ListInvitationBookingSettingSchedule.FirstOrDefault() == null ? "" : ListInvitationBookingSettingSchedule.FirstOrDefault().IdVenue,
                Venue = ListInvitationBookingSettingSchedule.FirstOrDefault() == null ? "" : ListInvitationBookingSettingSchedule.FirstOrDefault().Venue,
                Times = ListInvitationBookingSettingSchedule.Where(e=>!e.IsFull).Select(e => new Time
                {
                    IdInvitationBookingSettingSchedule = e.IdInvitationBookingSettingSchedule,
                    StartTimeInvitation = string.Format("{0:00}:{1:00}", e.StartTimeInvitation.Hours, e.StartTimeInvitation.Minutes),
                    EndTimeInvitation = string.Format("{0:00}:{1:00}", e.EndTimeInvitation.Hours, e.EndTimeInvitation.Minutes),
                    IsDisabled = _datetime.ServerTime>param.AppointmentDate+e.StartTimeInvitation?true:false
                }).ToList()
            };
          
            return Request.CreateApiResult2(items as object);
        }
    }
}
