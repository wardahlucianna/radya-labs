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

    public class CreateSchoolEventInvolvementHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public CreateSchoolEventInvolvementHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CreateSchoolEventInvolvementRequest, CreateSchoolEventInvolvementValidator>();

            var ay = await _dbContext.Entity<MsEventType>()
                .Where(x => x.Id == body.IdEventType)
                .Select(x => new { x.IdAcademicYear, x.AcademicYear.IdSchool })
                .FirstOrDefaultAsync(CancellationToken);

            if (ay is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["EventType"], "Id", body.IdEventType));

            var idHomeroom = "";

            #region Save Event
            var newEvent = new TrEvent
            {
                Id = Guid.NewGuid().ToString(),
                IdAcademicYear = ay.IdAcademicYear,
                IdEventType = body.IdEventType,
                Name = body.EventName,
                IsShowOnCalendarAcademic = false,
                IsShowOnSchedule = false,
                EventLevel = EventLevel.Internal,
                StatusEvent = "On Review (1)",
                StatusEventAward = "On Review (1)",
                IsStudentInvolvement = true
            };
            _dbContext.Entity<TrEvent>().Add(newEvent);
            #endregion

            #region Event Detail
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
                //                 && (body.IntendedFor.Any(e=>e.Role=="ALL") || body.IntendedFor.Any(e => e.Role.Contains(x.Event.EventIntendedFor.IntendedFor)))
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
                    IdEvent = newEvent.Id,
                    StartDate = date.Start,
                    EndDate = date.End
                };
                eventDetails.Add(newEventDetail);
            }
            _dbContext.Entity<TrEventDetail>().AddRange(eventDetails);
            #endregion

            #region Event Activity
            List<TrEventActivity> EventActivity = new List<TrEventActivity>();
            List<TrEventActivityAward> EventActivityAward = new List<TrEventActivityAward>();
            foreach (var ItemEventActivity in body.Activity)
            {
                var newEventActivity = new TrEventActivity
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newEvent.Id,
                    IdActivity = ItemEventActivity.IdActivity,
                };
                EventActivity.Add(newEventActivity);

                #region Activity Award
                foreach (var ItemInvolvmentAward in ItemEventActivity.EventActivityAwardIdUser)
                {
                    var cekUser = _dbContext.Entity<MsUser>().Where(x => x.Id == ItemInvolvmentAward.IdStudent).FirstOrDefault();
                    if(cekUser == null)
                        throw new BadRequestException("Data user not found");
                    var dataHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>().Include(x => x.Homeroom).Where(x => x.IdStudent == ItemInvolvmentAward.IdStudent && x.Homeroom.IdAcademicYear == body.IdAcadyear).FirstOrDefault();
                    if(dataHomeroomStudent == null)
                        throw new BadRequestException("Data homeroom student not found, please contact admin");
                    idHomeroom = dataHomeroomStudent.IdHomeroom;
                    string[] fileAward = {".pdf",".jpg",".png",".docx", ".jpeg"};
                    foreach (var ItemAward in ItemInvolvmentAward.IdAward)
                    {
                        if(!string.IsNullOrEmpty(ItemInvolvmentAward.Filetype) && !fileAward.Any(x => x == ItemInvolvmentAward.Filetype))
                        {
                            throw new BadRequestException("Invalid file type. Allowed types: .pdf, .jpg, .png, .docx, .jpeg");
                        }
                        else
                        {
                            var newEventActivityAward = new TrEventActivityAward
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEventActivity = newEventActivity.Id,
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

            #region Get data homeroom teacher
            var cekHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>().Where(x => x.IdHomeroom == idHomeroom).FirstOrDefault();
                if(cekHomeroomTeacher == null)
                    throw new BadRequestException("Data homeroom teacher not found, please contact admin");
            #endregion

            #region History Event Approval
            List<HTrEventApproval> EventAproval = new List<HTrEventApproval>();
            var newEventApproval = new HTrEventApproval
            {
                Id = Guid.NewGuid().ToString(),
                IdEvent = newEvent.Id,
                Section = "Event",
                State = 1,
                IdUser = cekHomeroomTeacher.IdBinusian
            };
            EventAproval.Add(newEventApproval);
            #endregion

            #region History Event Award Approval
            var EventAwardAproval = new List<HTrEventApproval>();
            var EventAprover = new List<TrEventApprover>();
            var AwardAprover = new List<TrEventAwardApprover>();
            var newEventAwardApproval = new HTrEventApproval();
            if (cekHomeroomTeacher.IdBinusian != "")
            {
                newEventAwardApproval = new HTrEventApproval
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newEvent.Id,
                    Section = "Award",
                    State = 1,
                    IdUser = cekHomeroomTeacher.IdBinusian,
                };
                EventAwardAproval.Add(newEventAwardApproval);
            }

            _dbContext.Entity<HTrEventApproval>().AddRange(EventAwardAproval);
            #endregion

            #region Event Approval
            var EventApprover = new List<TrEventApprover>();
            var ListApprover = EventAwardAproval.Where(e => e.Section == "Event").ToList();

            foreach (var Approver in ListApprover)
            {
                var NewEventApprover = new TrEventApprover
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = Approver.IdEvent,
                    IdUser = Approver.IdUser,
                };

                EventApprover.Add(NewEventApprover);
            }
            _dbContext.Entity<TrEventApprover>().AddRange(EventApprover);
            #endregion

            #region Award Approval
            var AwardApprover = new List<TrEventAwardApprover>();
            ListApprover = EventAwardAproval.Where(e => e.Section == "Award").ToList();

            foreach (var Approver in ListApprover)
            {
                var NewAwardApprover = new TrEventAwardApprover
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = Approver.IdEvent,
                    IdUser = Approver.IdUser,
                };

                AwardApprover.Add(NewAwardApprover);
            }
            _dbContext.Entity<TrEventAwardApprover>().AddRange(AwardApprover);
            #endregion

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
