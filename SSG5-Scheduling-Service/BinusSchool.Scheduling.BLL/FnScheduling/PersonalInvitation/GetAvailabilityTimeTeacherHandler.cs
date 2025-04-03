using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.PersonalInvitation
{
    public class GetAvailabilityTimeTeacherHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetAvailabilityTimeTeacherHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAvailabilityTimeTeacherRequest>();

            var Day = param.DateInvitation.ToString("dddd");

            var GetAvailabilityTime = await _dbContext.Entity<TrAvailabilitySetting>()
                    .Where(e=>e.IdUserTeacher==param.IdUserTeacher && e.Day==Day)
                    .ToListAsync(CancellationToken);

            List<AvailabilityTime> ListAvailabilityTime = new List<AvailabilityTime>();
            TimeSpan Interval = TimeSpan.FromMinutes(30);
            foreach (var item in GetAvailabilityTime)
            {
                var StartTime = item.StartTime;
                var EndTime = item.StartTime;
                while (EndTime <= item.EndTime)
                {
                    StartTime = EndTime;
                    EndTime = EndTime.Add(Interval);

                    ListAvailabilityTime.Add(new AvailabilityTime
                    {
                        Id = Guid.NewGuid().ToString(),
                        StartTime = StartTime,
                        EndTime = EndTime,
                        IsUse = false
                    });
                };
            }

            ListAvailabilityTime = ListAvailabilityTime.Distinct().ToList();

            var GetSchedule = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                    .Where(e => e.IdUser == param.IdUserTeacher && e.ScheduleDate.Date == param.DateInvitation.Date && e.IsGenerated)
                    .ToListAsync(CancellationToken);


            foreach (var item in GetSchedule)
            {
                var UpdateAvailabilityTimeBySchedule = ListAvailabilityTime.Where(e => (e.StartTime >= item.StartTime && e.StartTime <= item.EndTime)
                                                                                || (e.EndTime > item.StartTime && e.EndTime < item.EndTime)
                                                                    )
                                                                .ToList();

                ListAvailabilityTime
                    .Where(e => UpdateAvailabilityTimeBySchedule.Select(f => f.Id).ToList().Contains(e.Id))
                    .ToList()
                    .ForEach(e => e.IsUse = true);
            }


            var GetEventSchool = await _dbContext.Entity<TrUserEvent>()
                    .Include(e => e.EventDetail).ThenInclude(e => e.Event)
                    .Where(e => e.IdUser == param.IdUserTeacher && e.EventDetail.Event.StatusEvent == "Approved"
                    && (param.DateInvitation.Date >= e.EventDetail.StartDate.Date && param.DateInvitation.Date <= e.EventDetail.EndDate)
                    )
                    .Select(e => new
                    {
                        Startdate = e.EventDetail.StartDate,
                        EndDate = e.EventDetail.EndDate,
                    })
                    .ToListAsync(CancellationToken);


            foreach (var item in GetEventSchool)
            {
                var UpdateAvailabilityTimeByEvent = ListAvailabilityTime.Where(e => (e.StartTime >= item.Startdate.TimeOfDay && e.StartTime <= item.EndDate.TimeOfDay)
                                                                                || (e.EndTime > item.Startdate.TimeOfDay && e.EndTime < item.EndDate.TimeOfDay)
                                                                    )
                                                                .ToList();

                ListAvailabilityTime
                    .Where(e => UpdateAvailabilityTimeByEvent.Select(f => f.Id).ToList().Contains(e.Id))
                    .ToList()
                    .ForEach(e => e.IsUse = true);
            }

            var GetInvitationBooking = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                    .Where(e => e.IdUserTeacher == param.IdUserTeacher
                        && e.DateInvitation.Date == param.DateInvitation.Date)
                    .Select(e => new
                    {
                        Startdate = e.StartTime,
                        EndDate = e.EndTime,
                    })
                    .ToListAsync(CancellationToken);

            foreach (var item in GetInvitationBooking)
            {
                var UpdateAvailabilityTimeByEvent = ListAvailabilityTime.Where(e => (e.StartTime >= item.Startdate && e.StartTime <= item.EndDate)
                                                                                || (e.EndTime > item.Startdate && e.EndTime < item.EndDate)
                                                                    )
                                                                .ToList();

                ListAvailabilityTime
                    .Where(e => UpdateAvailabilityTimeByEvent.Select(f => f.Id).ToList().Contains(e.Id))
                    .ToList()
                    .ForEach(e => e.IsUse = true);
            }

            var GetPersonalInvitation = await _dbContext.Entity<TrPersonalInvitation>()
                    .Where(e => e.IdUserTeacher == param.IdUserTeacher
                        && e.InvitationDate == param.DateInvitation.Date && (e.Status== PersonalInvitationStatus.Approved || e.Status == PersonalInvitationStatus.OnRequest))
                    .Select(e => new
                    {
                        Startdate = e.InvitationStartTime,
                        EndDate = e.InvitationEndTime,
                    })
                    .ToListAsync(CancellationToken);

            foreach (var item in GetPersonalInvitation)
            {
                var UpdateAvailabilityTimeByEvent = ListAvailabilityTime.Where(e => (e.StartTime >= item.Startdate && e.StartTime <= item.EndDate)
                                                                                || (e.EndTime > item.Startdate && e.EndTime < item.EndDate)
                                                                    )
                                                                .ToList();

                ListAvailabilityTime
                    .Where(e => UpdateAvailabilityTimeByEvent.Select(f => f.Id).ToList().Contains(e.Id))
                    .ToList()
                    .ForEach(e => e.IsUse = true);
            }


            List<string> Items = new List<string>();
            TimeSpan startTimeItems = default;
            TimeSpan EndTimeItems = default;
            var NewAvailabilityTime = ListAvailabilityTime.Where(e => e.IsUse == false).Select(e => new { StartTime = e.StartTime, EndTime = e.EndTime }).Distinct().ToList();
            var Index = 0;
            foreach (var item in NewAvailabilityTime)
            {
                if (Index==0)
                {
                    startTimeItems = item.StartTime;
                    EndTimeItems = item.StartTime;
                }
                Index++;
                if (EndTimeItems != item.StartTime)
                {
                    Items.Add(startTimeItems.ToString(@"hh\:mm") + "-" + EndTimeItems.ToString(@"hh\:mm"));
                    Index = 0;
                }
                else if (NewAvailabilityTime.IndexOf(item)+1 == NewAvailabilityTime.Count())
                {
                    Items.Add(startTimeItems.ToString(@"hh\:mm") + "-" + item.EndTime.ToString(@"hh\:mm"));
                    Index = 0;
                }
                
                EndTimeItems = item.EndTime;

            }

            return Request.CreateApiResult2(Items as object);
        }
    }
}
