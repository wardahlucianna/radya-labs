using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.BreakSetting;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.BreakSetting
{
    public class GetBreakSettingV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetBreakSettingV2Handler(ISchedulingDbContext BreakSettingDbContext, IMachineDateTime dateTime)
        {
            _dbContext = BreakSettingDbContext;
            _dateTime = dateTime;

        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetBreakSettingRequest>();
            List<GetBreakSettingResult> Items = new List<GetBreakSettingResult>();

            #region Invitation Booking
            var predicateSchedule = PredicateBuilder.Create<TrInvitationBookingSettingSchedule>(e => e.IdUserTeacher == param.IdUserTeacher && e.DateInvitation.Date == param.DateCalender.Date);

            if (!string.IsNullOrEmpty(param.DateInvitation.ToString()))
                predicateSchedule = predicateSchedule.And(x => x.DateInvitation.Date == Convert.ToDateTime(param.DateInvitation).Date);

            //if (!string.IsNullOrEmpty(param.IdInvitationBookingSetting))
            //    predicateSchedule = predicateSchedule.And(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting);

            var GetInvitationBookingSettingSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                                                          .Include(e => e.InvitationBookingSetting)
                                                          .Include(e => e.UserTeacher)
                                                          .Where(predicateSchedule)
                                                          .Where(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting)
                                                          .OrderBy(e => e.StartTime).ToListAsync(CancellationToken);

            var GetInvitationBookingSettingSchedulePriority = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                                                          .Include(e => e.InvitationBookingSetting)
                                                          .Include(e => e.UserTeacher)
                                                          .Where(predicateSchedule)
                                                          .Where(e => e.IsPriority == true)
                                                          .OrderBy(e => e.StartTime).ToListAsync(CancellationToken);

            var predicateBooking = PredicateBuilder.Create<TrInvitationBookingDetail>(e => e.InvitationBooking.IdUserTeacher == param.IdUserTeacher && e.InvitationBooking.StartDateInvitation.Date == param.DateCalender.Date);
            if (!string.IsNullOrEmpty(param.DateInvitation.ToString()))
                predicateBooking = predicateBooking.And(x => x.InvitationBooking.StartDateInvitation.Date == Convert.ToDateTime(param.DateInvitation).Date);

            if (!string.IsNullOrEmpty(param.IdInvitationBookingSetting))
                predicateBooking = predicateBooking.And(x => x.InvitationBooking.IdInvitationBookingSetting == param.IdInvitationBookingSetting);

            var GetInvitationBooking = await _dbContext.Entity<TrInvitationBookingDetail>()
                                                          .Include(e => e.InvitationBooking).ThenInclude(e => e.Venue)
                                                          .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                                          .Where(predicateBooking)
                                                          .ToListAsync(CancellationToken);


            var GetInvitationBookingSetting = await _dbContext.Entity<TrInvitationBookingSetting>()
                                                          .Where(e => e.IdAcademicYear == param.IdAcademicYear && e.InvitationEndDate.Date > _dateTime.ServerTime.Date)
                                                          .ToListAsync(CancellationToken);

            var GetInvitationBookingSettingBreak = await _dbContext.Entity<TrInvitationBookingSettingBreak>()
                                                          .Where(e => e.DateInvitation.Date == param.DateCalender.Date && e.BreakType == BreakType.Flexible)
                                                          .ToListAsync(CancellationToken);


            var ItemsSchedule = GetInvitationBookingSettingSchedule
                .Select(e => new GetBreakSettingResult
                {
                    IdInvitationBookingSettingSchedule = e.Id,
                    IdInvitationBookingSetting = e.IdInvitationBookingSetting,
                    InvitationName = e.InvitationBookingSetting.InvitationName,
                    IsAvailable = e.IsAvailable,
                    DisabledAvailable = e.IsDisabledAvailable,
                    StartDate = e.DateInvitation.Date + e.StartTime,
                    EndDate = e.DateInvitation.Date + e.EndTime,
                    Description = e.Description,
                    Type = e.IsFlexibleBreak == true ? "FlexibleBreak" : e.IsFixedBreak == true ? "FixedBreak" : e.Description == "Break Between Session" ? "FixedBreak" : "InvitatinBooking",
                    IsFullBook = e.Description == "Break Between Session" || e.IsFixedBreak == true || e.IsFixedBreak == true
                                ? false
                                : GetInvitationBooking
                                    .Where(x => x.InvitationBooking.IdInvitationBookingSetting == e.IdInvitationBookingSetting && x.InvitationBooking.StartDateInvitation == e.DateInvitation.Date + e.StartTime)
                                    .Count() == e.QuotaSlot
                                        ? true
                                        : false,
                    Quota = e.QuotaSlot,
                    Students = !e.IsAvailable ? null
                                :
                                GetInvitationBooking
                                    .Where(x => x.InvitationBooking.IdInvitationBookingSetting == e.IdInvitationBookingSetting && x.InvitationBooking.StartDateInvitation == e.DateInvitation.Date + e.StartTime)
                                    .Select(x => new Student
                                    {
                                        IdInvitationBooking = x.IdInvitationBooking,
                                        IdStudent = x.HomeroomStudent.IdStudent,
                                        BinusianID = x.HomeroomStudent.IdStudent,
                                        IdHomeroomStudent = x.IdHomeroomStudent,
                                        StudentName = x.HomeroomStudent.Student.FirstName + (x.HomeroomStudent.Student.MiddleName == null ? "" : " " + x.HomeroomStudent.Student.MiddleName) + (x.HomeroomStudent.Student.LastName == null ? "" : " " + x.HomeroomStudent.Student.LastName),
                                        TeacherName = x.InvitationBooking.UserTeacher.DisplayName,
                                        IdUserTeacher = x.InvitationBooking.UserTeacher.Id,
                                        Venue = x.InvitationBooking.Venue.Description,
                                        Date = x.InvitationBooking.StartDateInvitation.Date,
                                        Time = x.InvitationBooking.StartDateInvitation.TimeOfDay,
                                        Status = x.InvitationBooking.Status.ToString(),
                                        Note = x.InvitationBooking.Note,
                                        InitiateBy = x.InvitationBooking.InitiateBy.ToString(),
                                        DisableButton = false
                                    }).ToList(),
                    IsPriority = e.IsPriority == true ? true : false,
                    DisabledPriority = e.IsPriority == true
                                        ? GetInvitationBooking
                                            .Where(x => x.InvitationBooking.IdInvitationBookingSetting == e.IdInvitationBookingSetting 
                                                        && x.InvitationBooking.StartDateInvitation == e.DateInvitation.Date + e.StartTime)
                                            .Any()
                                        : GetInvitationBookingSettingSchedulePriority
                                            .Where(f => f.Id != e.Id)
                                            .Where(x => (e.StartTime >= x.StartTime && e.StartTime <= x.EndTime) || (e.EndTime >= x.StartTime && e.EndTime <= x.EndTime))
                                            .Any(),
                    BreakSettings = GetInvitationBookingSettingBreak
                                    .Where(x => (e.DateInvitation.Date + e.StartTime >= x.DateInvitation.Date + x.StartTime && e.DateInvitation.Date + e.StartTime <= x.DateInvitation.Date + x.EndTime) || (e.DateInvitation.Date + e.EndTime >= x.DateInvitation.Date + x.StartTime && e.DateInvitation.Date + e.EndTime <= x.DateInvitation.Date + x.EndTime) & x.IdInvitationBookingSetting == e.IdInvitationBookingSetting)
                                    .Select(x => new BreakSettings
                                    {
                                        IdInvitationBookingSettingBreak = x.Id,
                                        Description = x.BreakName,
                                        IsChecked = e.IsFlexibleBreak == true ? true : false,
                                        Disabledcheckbox = e.IsDisabledFlexible,
                                    })
                                    .ToList(),
                })
                .ToList();

            Items.AddRange(ItemsSchedule);
            #endregion

            #region Personal Invitation
            var ItemsPersonalInvitation = await _dbContext.Entity<TrPersonalInvitation>()
                                                        .Include(e => e.Student)
                                                        .Include(e => e.Venue)
                                                         .Where(e => e.InvitationDate.Date == param.DateCalender.Date
                                                            && e.IdUserTeacher == param.IdUserTeacher
                                                            && e.IdAcademicYear == param.IdAcademicYear
                                                            && (e.Status == PersonalInvitationStatus.OnRequest || e.Status == PersonalInvitationStatus.Approved))
                                                         .ToListAsync(CancellationToken);

            Items.AddRange(
                    ItemsPersonalInvitation
                      .Select(e => new GetBreakSettingResult
                      {
                          IdInvitationBookingSettingSchedule = null,
                          IdInvitationBookingSetting = null,
                          InvitationName = "Personal Invitation",
                          IsAvailable = false,
                          DisabledAvailable = false,
                          StartDate = e.InvitationDate + e.InvitationStartTime,
                          EndDate = e.InvitationDate + e.InvitationEndTime,
                          Description = e.Description,
                          Type = "PersonalInvitation",
                          IsFullBook = false,
                          Quota = 0,
                          Students = ItemsPersonalInvitation.Where(x => x.Id == e.Id).Select(x => new Student
                          {
                              IdStudent = x.Student.Id,
                              BinusianID = x.Student.Id,
                              IdHomeroomStudent = null,
                              StudentName = x.Student.FirstName + (x.Student.MiddleName == null ? "" : " " + x.Student.MiddleName) + (x.Student.LastName == null ? "" : " " + x.Student.LastName),
                              TeacherName = x.UserTeacher.DisplayName,
                              IdUserTeacher = x.UserTeacher.Id,
                              Venue = x.Venue?.Description,
                              Date = x.InvitationDate.Date,
                              Time = x.InvitationStartTime,
                              Status = null,
                              Note = null,
                              InitiateBy = null,
                              DisableButton = true
                          }).ToList(),
                          IsPriority = false,
                          DisabledPriority = false,
                          BreakSettings = default,
                      }).ToList()
                );

            #endregion


            #region Availability booking
            var Day = param.DateCalender.ToString("dddd");

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

            var GetMsSchedule = await _dbContext.Entity<MsSchedule>()
                    .Where(e => e.IdUser == param.IdUserTeacher && e.Lesson.IdAcademicYear==param.IdAcademicYear)
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

            var GetSchedule = await _dbContext.Entity<MsScheduleLesson>()
                    .Where(e =>
                            listIdLesson.Contains(e.IdLesson)
                            && listIdSession.Contains(e.IdSession)
                            && listIdDay.Contains(e.IdDay)
                            && listIdWeek.Contains(e.IdWeek)
                            && e.ScheduleDate.Date == param.DateCalender.Date 
                            && e.IsGenerated)
                    .Select(e => new
                    {
                        StartTime = e.StartTime,
                        EndTime = e.EndTime,
                    })
                    .ToListAsync(CancellationToken);

            var GetEventSchool = await _dbContext.Entity<TrUserEvent>()
                    .Include(e => e.EventDetail).ThenInclude(e => e.Event)
                    .Where(e => e.IdUser == param.IdUserTeacher && e.EventDetail.Event.StatusEvent == "Approved"
                    && (param.DateCalender.Date >= e.EventDetail.StartDate.Date && param.DateCalender.Date <= e.EventDetail.EndDate)
                    )
                    .Select(e => new
                    {
                        StartTime = e.EventDetail.StartDate.TimeOfDay,
                        EndTime = e.EventDetail.EndDate.TimeOfDay,
                    })
                    .ToListAsync(CancellationToken);

            var GetInvitationBookingAvailability = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                  .Where(e => e.IdUserTeacher == param.IdUserTeacher
                      && e.DateInvitation.Date == param.DateCalender.Date)
                  .Select(e => new
                  {
                      StartTime = e.StartTime,
                      EndTime = e.EndTime,
                  })
                  .ToListAsync(CancellationToken);

            var GetPersonalInvitation = await _dbContext.Entity<TrPersonalInvitation>()
                    .Where(e => e.IdUserTeacher == param.IdUserTeacher
                        && e.InvitationDate == param.DateCalender.Date && (e.Status == PersonalInvitationStatus.Approved || e.Status == PersonalInvitationStatus.OnRequest))
                    .Select(e => new
                    {
                        StartTime = e.InvitationStartTime,
                        EndTime = e.InvitationEndTime,
                    })
                    .ToListAsync(CancellationToken);

            var ListInvitatin = GetSchedule.Union(GetEventSchool).Union(GetInvitationBookingAvailability).Union(GetPersonalInvitation).ToList();


            foreach (var item in ListInvitatin)
            {
                var UpdateAvailabilityTimeBySchedule = ListAvailabilityTime.Where(e => (e.StartTime >= item.StartTime && e.StartTime < item.EndTime)
                                                                                || (e.EndTime > item.StartTime && e.EndTime < item.EndTime)
                                                                    )
                                                                .ToList();

                ListAvailabilityTime
                    .Where(e => UpdateAvailabilityTimeBySchedule.Select(f => f.Id).ToList().Contains(e.Id))
                    .ToList()
                    .ForEach(e => e.IsUse = true);
            }



            TimeSpan startTimeItems = default;
            TimeSpan EndTimeItems = default;
            var NewAvailabilityTime = ListAvailabilityTime.Where(e => e.IsUse == false).Select(e => new { StartTime = e.StartTime, EndTime = e.EndTime }).Distinct().ToList();
            var Index = 0;
            foreach (var item in NewAvailabilityTime)
            {
                if (Index == 0)
                {
                    startTimeItems = item.StartTime;
                    EndTimeItems = item.StartTime;
                }

                if (EndTimeItems != item.StartTime)
                {
                    Items.Add(new GetBreakSettingResult
                    {
                        IdInvitationBookingSettingSchedule = null,
                        IdInvitationBookingSetting = null,
                        InvitationName = "Available to Book",
                        IsAvailable = false,
                        DisabledAvailable = false,
                        StartDate = param.DateCalender.Date + startTimeItems,
                        EndDate = param.DateCalender.Date + EndTimeItems,
                        Description = "Available to Book for Parent Event",
                        Type = "AvailabilityBooking",
                        IsFullBook = false,
                        Quota = 0,
                        Students = default,
                        IsPriority = false,
                        DisabledPriority = false,
                        BreakSettings = default,
                    });
                    startTimeItems = item.StartTime;
                }
                else if (NewAvailabilityTime.IndexOf(item) + 1 == NewAvailabilityTime.Count())
                {
                    Items.Add(new GetBreakSettingResult
                    {
                        IdInvitationBookingSettingSchedule = null,
                        IdInvitationBookingSetting = null,
                        InvitationName = "Available to Book",
                        IsAvailable = false,
                        DisabledAvailable = false,
                        StartDate = param.DateCalender.Date + startTimeItems,
                        EndDate = param.DateCalender.Date + EndTimeItems,
                        Description = "Available to Book for Parent Event",
                        Type = "AvailabilityBooking",
                        IsFullBook = false,
                        Quota = 0,
                        Students = default,
                        IsPriority = false,
                        DisabledPriority = false,
                        BreakSettings = default,
                    });
                    startTimeItems = item.StartTime;
                }
                EndTimeItems = item.EndTime;
                Index++;
            }

            #endregion

            return Request.CreateApiResult2(Items as object);
        }
    }
}
