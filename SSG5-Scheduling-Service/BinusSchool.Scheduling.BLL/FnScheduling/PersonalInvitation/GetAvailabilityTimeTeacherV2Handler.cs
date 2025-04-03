using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Scheduling.FnSchedule.PersonalInvitation
{
    public class GetAvailabilityTimeTeacherV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IServiceProvider _provider;

        public GetAvailabilityTimeTeacherV2Handler(ISchedulingDbContext dbContext, IServiceProvider provider)
        {
            _dbContext = dbContext;
            _provider = provider;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAvailabilityTimeTeacherRequest>();

            List<AvailabilityTime> AvailabilityTeacher = new List<AvailabilityTime>();
            using (var scope = _provider.CreateScope())
            {
                AvailabilityTeacher = await GetAvailabilityTimeTeacherV2Handler.GetAvailabilityTimeTeacher(param, _dbContext, CancellationToken);
            }

            List<string> Items = new List<string>();
            foreach (var item in AvailabilityTeacher)
            {
                Items.Add(item.StartTime.ToString(@"hh\:mm") + "-" + item.EndTime.ToString(@"hh\:mm"));
            }

            return Request.CreateApiResult2(Items as object);
        }

        public static async Task<List<AvailabilityTime>> GetAvailabilityTimeTeacher(GetAvailabilityTimeTeacherRequest param, ISchedulingDbContext _dbContext, CancellationToken CancellationToken, string IdPersonalInvitation = null)
        {
            var Day = param.DateInvitation.ToString("dddd");
            #region Availability Setting
            var GetAvailabilityTime = await _dbContext.Entity<TrAvailabilitySetting>()
                    .Where(e => e.IdUserTeacher == param.IdUserTeacher && e.Day == Day)
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

                    if (EndTime <= item.EndTime)
                        ListAvailabilityTime.Add(new AvailabilityTime
                        {
                            Id = Guid.NewGuid().ToString(),
                            StartTime = StartTime.Add(TimeSpan.FromMinutes(1)),
                            StartTimeTittle = StartTime,
                            EndTime = EndTime,
                            IsUse = false
                        });
                };
            }

            ListAvailabilityTime = ListAvailabilityTime.Distinct().OrderBy(e => e.StartTime).ToList();
            #endregion

            #region Schedule
            var GetMsSchedule = await _dbContext.Entity<MsSchedule>()
                    .Where(e => e.IdUser == param.IdUserTeacher && e.Lesson.IdAcademicYear == param.IdAcademicYear)
                    .Select(e => new
                    {
                        e.IdLesson,
                        e.IdSession,
                        e.IdDay,
                        e.IdWeek
                    })
                    .ToListAsync(CancellationToken);

            var listIdLesson = GetMsSchedule.Select(e => e.IdLesson).Distinct().ToList();
            var listIdSession = GetMsSchedule.Select(e => e.IdSession).Distinct().ToList();
            var listIdDay = GetMsSchedule.Select(e => e.IdDay).Distinct().ToList();
            var listIdWeek = GetMsSchedule.Select(e => e.IdWeek).Distinct().ToList();
            #endregion

            #region Schedule Lesson
            var GetSchedule = await _dbContext.Entity<MsScheduleLesson>()
                    .Where(e =>
                            listIdLesson.Contains(e.IdLesson)
                            && listIdSession.Contains(e.IdSession)
                            && listIdDay.Contains(e.IdDay)
                            && listIdWeek.Contains(e.IdWeek)
                            && e.ScheduleDate.Date == param.DateInvitation.Date
                            && e.IsGenerated)
                    .Select(e => new AvailabilityTime
                    {
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                    })
                    .ToListAsync(CancellationToken);
            #endregion

            #region User Event
            var GetEventSchool = await _dbContext.Entity<TrUserEvent>()
                    .Include(e => e.EventDetail).ThenInclude(e => e.Event)
                    .Where(e => e.IdUser == param.IdUserTeacher && e.EventDetail.Event.StatusEvent == "Approved"
                    && (param.DateInvitation.Date >= e.EventDetail.StartDate.Date && param.DateInvitation.Date <= e.EventDetail.EndDate)
                    )
                    .Select(e => new AvailabilityTime
                    {
                        StartTime = e.EventDetail.StartDate.TimeOfDay,
                        EndTime = e.EventDetail.EndDate.TimeOfDay,
                    })
                    .ToListAsync(CancellationToken);
            #endregion

            #region Invitation Booking Setting Schedule
            var GetInvitationBooking = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                    .Where(e => e.IdUserTeacher == param.IdUserTeacher
                        && e.DateInvitation.Date == param.DateInvitation.Date)
                    .Select(e => new AvailabilityTime
                    {
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                    })
                    .ToListAsync(CancellationToken);
            #endregion

            #region Personal Invitation
            var predicate = PredicateBuilder.Create<TrPersonalInvitation>(e=> (e.Status == PersonalInvitationStatus.Approved || e.Status == PersonalInvitationStatus.OnRequest));
            if (!string.IsNullOrEmpty(IdPersonalInvitation))
                predicate = predicate.And(e => e.Id != IdPersonalInvitation);
            if (!string.IsNullOrEmpty(param.IdUserTeacher))
                predicate = predicate.And(e => e.IdUserTeacher == param.IdUserTeacher);
            if (!string.IsNullOrEmpty(param.DateInvitation.Date.ToString()))
                predicate = predicate.And(e => e.InvitationDate.Date == param.DateInvitation.Date);

            var GetPersonalInvitation = await _dbContext.Entity<TrPersonalInvitation>()
                    .Where(predicate)
                    .Select(e => new AvailabilityTime
                    {
                        StartTime = e.InvitationStartTime,
                        EndTime = e.InvitationEndTime,
                    })
                    .ToListAsync(CancellationToken);
            #endregion

            var unionTimeUsing = GetSchedule.Union(GetEventSchool).Union(GetInvitationBooking).Union(GetPersonalInvitation)
                                    .OrderBy(e=>e.StartTime)
                                    .ToList();

            foreach (var item in unionTimeUsing)
            {
                var UpdateAvailabilityTimeBySchedule = ListAvailabilityTime.Where(e => (e.StartTime >= item.StartTime && e.StartTime <= item.EndTime)
                                                                                || (e.EndTime > item.StartTime && e.EndTime < item.EndTime)
                                                                                || (e.StartTime <= item.StartTime && e.EndTime > item.EndTime)
                                                                    )
                                                                .ToList();

                ListAvailabilityTime
                    .Where(e => UpdateAvailabilityTimeBySchedule.Select(f => f.Id).ToList().Contains(e.Id))
                    .ToList()
                    .ForEach(e => e.IsUse = true);
            }

            var NewAvailabilityTime = ListAvailabilityTime.Where(e => e.IsUse == false).Select(e => new { StartTime = e.StartTimeTittle, EndTime = e.EndTime }).Distinct().ToList();

            if (!NewAvailabilityTime.Any())
                throw new BadRequestException("Availability time is not exsis");

            List<AvailabilityTime> Items = new List<AvailabilityTime>();
            TimeSpan startTimeItems = default;
            TimeSpan EndTimeItems = default;
            var Index = 0;
            foreach (var item in NewAvailabilityTime)
            {
                if (Index == 0)
                {
                    startTimeItems = item.StartTime;
                    EndTimeItems = item.StartTime;
                }
                Index++;
                if (EndTimeItems != item.StartTime)
                {
                    Items.Add(new AvailabilityTime
                    {
                        StartTime = startTimeItems,
                        EndTime = EndTimeItems
                    });
                    startTimeItems = item.StartTime;
                }
                else if (NewAvailabilityTime.IndexOf(item) + 1 == NewAvailabilityTime.Count())
                {
                    EndTimeItems = item.EndTime;
                    Items.Add(new AvailabilityTime
                    {
                        StartTime = startTimeItems,
                        EndTime = EndTimeItems
                    });
                    Index = 0;
                }
                EndTimeItems = item.EndTime;
            }

            return Items;
        }
    }
}
