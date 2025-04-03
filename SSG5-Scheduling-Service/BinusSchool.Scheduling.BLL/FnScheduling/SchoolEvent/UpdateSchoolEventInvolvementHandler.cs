using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.Award.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{

    public class UpdateSchoolEventInvolvementHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public UpdateSchoolEventInvolvementHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateSchoolEventInvolvementRequest, UpdateSchoolEventInvolvementValidator>();

            var ay = await _dbContext.Entity<MsEventType>()
                .Where(x => x.Id == body.IdEventType)
                .Select(x => new { x.IdAcademicYear, x.AcademicYear.IdSchool })
                .FirstOrDefaultAsync(CancellationToken);

            if (ay is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["EventType"], "Id", body.IdEventType));

            var existEvent = await _dbContext.Entity<TrEvent>()
                .FirstOrDefaultAsync(x => x.Id == body.IdEvent, CancellationToken);

            if (existEvent is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Event"], "Id", body.IdEvent));

            #region Event Change
            var DisplayName = _dbContext.Entity<MsUser>()
                .SingleOrDefault(e => e.Id == body.IdUser).DisplayName;
            var idEventChange = Guid.NewGuid().ToString();
            var newEventChange = new TrEventChange
            {
                Id = idEventChange,
                IdEvent = existEvent.Id,
                ChangeNotes = "Event Edited by " + DisplayName
            };
            _dbContext.Entity<TrEventChange>().Add(newEventChange);
            #endregion

            #region History Event
            var newHTrEvent = new HTrEvent
            {
                Id = idEventChange,
                IdEventType = existEvent.IdEventType,
                IdAcademicYear = existEvent.IdAcademicYear,
                Name = existEvent.Name,
                IsShowOnCalendarAcademic = existEvent.IsShowOnCalendarAcademic,
                IsShowOnSchedule = existEvent.IsShowOnSchedule,
                Objective = existEvent.Objective,
                Place = existEvent.Place,
                EventLevel = existEvent.EventLevel,
                StatusEvent = existEvent.StatusEvent,
                DescriptionEvent = existEvent.DescriptionEvent,
                StatusEventAward = existEvent.StatusEventAward,
                DescriptionEventAward = existEvent.DescriptionEventAward,
                IdCertificateTemplate = existEvent.IdCertificateTemplate,
            };
            _dbContext.Entity<HTrEvent>().Add(newHTrEvent);
            #endregion


            #region History Event Detail
            var UpdateEventDetail = await _dbContext.Entity<TrEventDetail>().Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            foreach (var ItemDetail in UpdateEventDetail)
            {
                var newHTrEventDetail = new HTrEventDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    StartDate = ItemDetail.StartDate,
                    EndDate = ItemDetail.EndDate,
                    //StartTime = ItemDetail.StartTime,
                    //EndTime = ItemDetail.EndTime,
                };
                _dbContext.Entity<HTrEventDetail>().Add(newHTrEventDetail);
            }
            #endregion

            #region Event
            if (existEvent.StatusEvent == "Declined")
            {
                #region update Event
                existEvent.IdAcademicYear = ay.IdAcademicYear;
                existEvent.IdEventType = body.IdEventType;
                existEvent.Name = body.EventName;
                existEvent.IsShowOnCalendarAcademic = false;
                existEvent.IsShowOnSchedule = false;
                existEvent.EventLevel = EventLevel.Internal;
                existEvent.StatusEvent = "On Review (1)";
                _dbContext.Entity<TrEvent>().Update(existEvent);
                #endregion

                #region update Event Detail
                UpdateEventDetail.ForEach(x => x.IsActive = false);
                _dbContext.Entity<TrEventDetail>().UpdateRange(UpdateEventDetail);
                #endregion

                #region Create Event Detail
                var eventDetails = new List<TrEventDetail>(body.Dates.Count());

                foreach (var date in body.Dates)
                {
                    // var intersectEvents = await _dbContext.Entity<TrEventDetail>()
                    //    .Include(x => x.Event).ThenInclude(x => x.EventIntendedFor)
                    //    .Where(x
                    //        => x.Event.EventType.AcademicYear.IdSchool == ay.IdSchool
                    //        && (x.StartDate == date.Start || x.EndDate == date.End
                    //        || (x.StartDate < date.Start
                    //            ? (x.EndDate > date.Start && x.EndDate < date.End) || x.EndDate > date.End
                    //            : (date.End > x.StartDate && date.End < x.EndDate) || date.End > x.EndDate)))
                    //    .ToListAsync(CancellationToken);

                    // check date & time conflict with existing intersect event
                    // var conflictEvents = Enumerable.Empty<string>();
                    // if (intersectEvents.Count != 0)
                    // {
                    //     // get each date of new event
                    //     var eachDate = DateTimeUtil.ToEachDay(date.Start, date.End);

                    //     foreach (var (start, end) in eachDate)
                    //     {
                    //         // select event that intersect date & time with day
                    //         var dayOfEvents = intersectEvents.Where(x => DateTimeUtil.IsIntersect(x.StartDate, x.EndDate, start, end));
                    //         // select event that intersect time with day
                    //         var intersectDayOfEvents = dayOfEvents
                    //             .Where(x
                    //                 => TimeSpanUtil.IsIntersect(x.StartDate.TimeOfDay, x.EndDate.TimeOfDay, start.TimeOfDay, end.TimeOfDay)
                    //                 && (body.IntendedFor.Any(e => e.Role == "ALL") || body.IntendedFor.Any(e => e.Role.Contains(x.Event.EventIntendedFor.IntendedFor)))
                    //                 && x.Event.Name == body.EventName);

                    //         if (intersectDayOfEvents.Any())
                    //             conflictEvents = conflictEvents.Concat(intersectEvents.Select(x => x.Event.Name));
                    //     }
                    // }

                    // if (conflictEvents.Any())
                    //     throw new BadRequestException("There is another event with same name, intended for, date and time.");

                    var newEventDetail = new TrEventDetail
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = existEvent.Id,
                        StartDate = date.Start,
                        EndDate = date.End
                    };
                    eventDetails.Add(newEventDetail);
                }
                _dbContext.Entity<TrEventDetail>().AddRange(eventDetails);
                #endregion
            }
            else
            {
                #region update Event
                existEvent.EventLevel = EventLevel.Internal;
                existEvent.StatusEvent = "On Review (1)";
                _dbContext.Entity<TrEvent>().Update(existEvent);
                #endregion
            }
            #endregion

            #region Event Activity
            var GetActivity = await _dbContext.Entity<TrEventActivity>()
                .Include(e => e.EventActivityRegistrants)
                .Include(e => e.EventActivityPICs)
                .Include(e => e.EventActivityAwards)
                .Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            foreach (var ItemActivity in GetActivity)
            {
                #region History Activity
                var newHTrEventActivity = new HTrEventActivity
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IdActivity = ItemActivity.IdActivity,
                };
                _dbContext.Entity<HTrEventActivity>().Add(newHTrEventActivity);
                #endregion

                #region History Award
                foreach (var ItemActivityAward in ItemActivity.EventActivityAwards)
                {
                    var newHTrEventActivityAward = new HTrEventActivityAward
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newHTrEventActivity.Id,
                        IdAward = ItemActivityAward.IdAward,
                        IdHomeroomStudent = ItemActivityAward.IdHomeroomStudent,
                        Url = ItemActivityAward.Url,
                        Filename = ItemActivityAward.Filename,
                        Filetype = ItemActivityAward.Filetype,
                        Filesize = ItemActivityAward.Filesize,
                        OriginalFilename = ItemActivityAward.OriginalFilename,
                    };

                    _dbContext.Entity<HTrEventActivityAward>().Add(newHTrEventActivityAward);
                    await _dbContext.SaveChangesAsync(CancellationToken);
                }
                #endregion

                #region Update Activity
                var ExsisBodyActivity = body.Activity.Any(e => e.Id == ItemActivity.Id);
                if (!ExsisBodyActivity)
                {
                    ItemActivity.IsActive = false;
                    _dbContext.Entity<TrEventActivity>().Update(ItemActivity);

                    var GetActivityAward = ItemActivity.EventActivityAwards.ToList();
                    GetActivityAward.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrEventActivityAward>().UpdateRange(GetActivityAward);
                }
                else
                {
                    foreach (var itemAcyivityAward in ItemActivity.EventActivityAwards)
                    {
                        itemAcyivityAward.IsActive = false;
                        _dbContext.Entity<TrEventActivityAward>().Update(itemAcyivityAward);
                    }

                }
                #endregion
            }

            #region Create Event Activity
            List<TrEventActivity> EventActivity = new List<TrEventActivity>();
            List<TrEventActivityAward> EventActivityAward = new List<TrEventActivityAward>();

            foreach (var ItemEventActivity in body.Activity)
            {
                var GetEventActivity = GetActivity.SingleOrDefault(e => e.Id == ItemEventActivity.Id);
                if (GetEventActivity == null)
                {
                    var newEventActivity = new TrEventActivity
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = existEvent.Id,
                        IdActivity = ItemEventActivity.IdActivity,
                    };
                    EventActivity.Add(newEventActivity);
                    ItemEventActivity.Id = newEventActivity.Id;
                }
                else
                {
                    GetEventActivity.IdActivity = ItemEventActivity.IdActivity;
                    _dbContext.Entity<TrEventActivity>().Update(GetEventActivity);
                }

                #region Activity Award
                string[] fileAward = {".pdf",".jpg",".png",".docx", ".jpeg"};
                foreach (var ItemInvolvmentAward in ItemEventActivity.EventActivityAwardIdUser)
                {
                    foreach (var ItemAward in ItemInvolvmentAward.IdAward)
                    {
                        var cekUser = _dbContext.Entity<MsUser>().Where(x => x.Id == ItemInvolvmentAward.IdStudent).FirstOrDefault();
                        if(cekUser == null)
                            throw new BadRequestException("Data user not found");
                        var dataHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>().Where(x => x.IdStudent == ItemInvolvmentAward.IdStudent).FirstOrDefault();
                        if(dataHomeroomStudent == null)
                            throw new BadRequestException("Data homeroom student not found, please contact admin");
                        if(!string.IsNullOrEmpty(ItemInvolvmentAward.Filetype) && !fileAward.Any(x => x == ItemInvolvmentAward.Filetype))
                        {
                            throw new BadRequestException("Invalid file type. Allowed types: .pdf, .jpg, .png, .docx, .jpeg");
                        }
                        else
                        {
                            var newEventActivityAward = new TrEventActivityAward
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEventActivity = ItemEventActivity.Id,
                                IdAward = ItemAward,
                                IdHomeroomStudent = dataHomeroomStudent.Id,
                                Url = ItemInvolvmentAward.Url,
                                Filename = ItemInvolvmentAward.Filename,
                                Filetype = ItemInvolvmentAward.Filetype,
                                Filesize = ItemInvolvmentAward.Filesize,
                                OriginalFilename = ItemInvolvmentAward.OriginalFilename
                            };
                            EventActivityAward.Add(newEventActivityAward);
                        }
                    }
                }
                #endregion
            }
            _dbContext.Entity<TrEventActivity>().AddRange(EventActivity);
            _dbContext.Entity<TrEventActivityAward>().AddRange(EventActivityAward);
            #endregion
            #endregion
            
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
