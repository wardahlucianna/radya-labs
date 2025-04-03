using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
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
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class AppointmentBookingParentHendler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public AppointmentBookingParentHendler(ISchedulingDbContext schedulingDbContext, IMachineDateTime dateTime)
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
            var items = await _dbContext.Entity<TrInvitationBooking>()
                .Include(e => e.InvitationBookingSetting)
                .Include(e => e.InvitationBookingDetails).ThenInclude(e => e.HomeroomStudent)
                .ThenInclude(e => e.Student)
                .Include(e => e.InvitationBookingDetails).ThenInclude(e => e.HomeroomStudent)
                .ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                .Include(e => e.InvitationBookingDetails).ThenInclude(e => e.HomeroomStudent)
                .ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.Venue)
                .Include(e => e.UserTeacher)
                .Where(e => e.Id == id)
                .Select(x => new DetailAppointmentBookingParentResult
                {
                    Id = x.Id,
                    IdInvitationBookingSetting = x.IdInvitationBookingSetting,
                    InvitationName = x.InvitationBookingSetting.InvitationName,
                    Venue = new ItemValueVm
                    {
                        Id = x.Venue.Id,
                        Description = x.Venue.Description
                    },
                    Teacher = new ItemValueVm
                    {
                        Id = x.IdUserTeacher,
                        Description = x.UserTeacher.DisplayName
                    },
                    DateInvitation = x.StartDateInvitation.Date,
                    TimeInvitation = x.StartDateInvitation.ToString("HH:mm"),
                    DetailStudents = x.InvitationBookingDetails.Select(x => new DetailStudents
                    {
                        IdHomeroomStudent = x.IdHomeroomStudent,
                        StudentName = x.HomeroomStudent.Student.FirstName == null
                            ? x.HomeroomStudent.Student.LastName
                            : x.HomeroomStudent.Student.FirstName
                              + (x.HomeroomStudent.Student.MiddleName == null
                                  ? ""
                                  : " " + x.HomeroomStudent.Student.MiddleName)
                              + (x.HomeroomStudent.Student.LastName == null
                                  ? ""
                                  : " " + x.HomeroomStudent.Student.LastName),
                        StudentId = x.HomeroomStudent.IdStudent,
                        Level = x.HomeroomStudent.Homeroom.Grade.Level.Description,
                        Grade = x.HomeroomStudent.Homeroom.Grade.Description,
                        Homeroom = x.HomeroomStudent.Homeroom.Grade.Code +
                                   x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    }).ToList(),
                    FootNote = x.InvitationBookingSetting.FootNote
                }).SingleOrDefaultAsync(CancellationToken);

            var userVenue = await _dbContext.Entity<TrInvitationBooking>()
                .Include(x => x.InvitationBookingSetting)
                    .ThenInclude(x => x.InvitationBookingSettingVenueDates)
                .Where(x => x.Id == items.Id)
                .Select(x => new
                {
                    IdInvitationBookingSettingVenueDates = x.InvitationBookingSetting.InvitationBookingSettingVenueDates.Select(x => x.Id).FirstOrDefault()
                })
                .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
        }

        private class InvitationBookingGetResult
        {
            public InvitationBookingGetResult()
            {
                Students = new List<DetailStudents>();
            }

            public string Id { get; set; }
            public string AppoitmentName { get; set; }
            public List<DetailStudents> Students { get; set; }
            public DateTime InvitationDateTime { get; set; }
            public string Venue { get; set; }
            public string IdTeacher { get; set; }
            public string TeacherBooked { get; set; }
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetAppointmentBookingParentRequest>();
            var predicate = PredicateBuilder.Create<TrInvitationBooking>(x => true);

            var userSchool = _dbContext.Entity<MsUserRole>().Include(e => e.Role).Where(e => e.IdUser == param.IdUser)
                .Select(e => e.Role.IdSchool).FirstOrDefault();

            predicate = predicate.And(e =>
                e.InvitationBookingSetting.AcademicYears.IdSchool == userSchool &&
                e.StatusData != InvitatinBookingStatusData.OnProgress);

            if (param.Role == "PARENT")
            {
                predicate = predicate.And(e =>
                    e.UserIn == param.IdUser ||
                    e.InvitationBookingDetails.Any(y => y.HomeroomStudent.IdStudent == param.IdUser.Substring(1)));
            }

            string[] _columns =
            {
                "AppoitmentName", "StudentName", "BinusanId", "InvitationDate", "TimeBooked", "Venue", "TeacherBooked"
            };

            if (!string.IsNullOrEmpty(param.Date.ToString()))
                predicate = predicate.And(x => x.StartDateInvitation.Date == param.Date);

            var DataInvitation = await _dbContext.Entity<TrInvitationBooking>()
                .Include(e => e.InvitationBookingSetting)
                //.Include(e => e.InvitationBookingDetails).ThenInclude(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                .Include(e => e.Venue)
                .Include(e => e.UserTeacher)
                .Where(predicate)
                .Select(x => new InvitationBookingGetResult
                {
                    Id = x.Id,
                    AppoitmentName = x.InvitationBookingSetting.InvitationName,
                    //StudentDetail = x.InvitationBookingDetails.Select(x => new DetailStudents
                    //{
                    //    StudentName = x.HomeroomStudent.Student.FirstName != null
                    //        ? $"{x.HomeroomStudent.Student.FirstName} {x.HomeroomStudent.Student.MiddleName}"
                    //        : $"{x.HomeroomStudent.Student.LastName}",
                    //    StudentId = x.HomeroomStudent.IdStudent,
                    //    Level = x.HomeroomStudent.Homeroom.Grade.Level.Description,
                    //    Grade = x.HomeroomStudent.Homeroom.Grade.Description,
                    //    Homeroom = x.HomeroomStudent.Homeroom.Grade.Code +
                    //               x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    //}).ToList(),
                    InvitationDateTime = x.StartDateInvitation,
                    Venue = x.Venue.Description,
                    IdTeacher = x.UserTeacher.Id,
                    TeacherBooked = x.UserTeacher.DisplayName,
                })
                .ToListAsync(CancellationToken);

            foreach (var item in DataInvitation)
            {
                var result = await _dbContext.Entity<TrInvitationBookingDetail>()
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Venue)
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                    .Where(e => e.IdInvitationBooking == item.Id)
                    .AsNoTracking()
                    .ToListAsync(CancellationToken);

                foreach (var x in result)
                    item.Students.Add(new DetailStudents
                    {
                        StudentName = $"{x.HomeroomStudent.Student?.FirstName ?? ""} {x.HomeroomStudent.Student?.MiddleName ?? ""} {x.HomeroomStudent.Student?.LastName ?? ""}".Trim(),
                        StudentId = x.HomeroomStudent.IdStudent,
                        Level = x.HomeroomStudent.Homeroom.Grade?.Level?.Description,
                        Grade = x.HomeroomStudent.Homeroom.Grade?.Description,
                        Homeroom = $"{x.HomeroomStudent.Homeroom.Grade?.Code ?? ""}{x.HomeroomStudent.Homeroom.GradePathwayClassroom?.Classroom?.Code ?? ""}"
                    });

                var userVenue = await _dbContext.Entity<TrInvitationBooking>()
                    .Include(x => x.InvitationBookingSetting)
                        .ThenInclude(x => x.InvitationBookingSettingVenueDates)
                    .Where(x => x.Id == item.Id)
                    .Select(x => new
                    {
                        IdInvitationBookingSettingVenueDates = x.InvitationBookingSetting.InvitationBookingSettingVenueDates.Select(x => x.Id).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync(CancellationToken);
            }

            var query = DataInvitation
                .Select(x => new
                {
                    Id = x.Id,
                    AppoitmentName = x.AppoitmentName,
                    StudentName = ConvertString(x.Students.Select(e => e.StudentName).ToList()),
                    BinusanId = x.Students.Count == 0
                        ? "-"
                        : ConvertString(x.Students.Select(e => e.StudentId).ToList()),
                    InvitationDate = x.InvitationDateTime.Date,
                    TimeBooked = x.InvitationDateTime.TimeOfDay.ToString(),
                    Venue = x.Venue,
                    TeacherBooked = x.TeacherBooked,
                });

            //search
            if (!string.IsNullOrEmpty(param.Search))
            {
                if (RoleConstant.Staff == param.Role)
                    query = query.Where(x =>
                        x.StudentName.ToLower().Contains(param.Search.ToLower()) ||
                        x.TeacherBooked.ToLower().Contains(param.Search.ToLower()));
                else if (RoleConstant.Parent == param.Role)
                    query = query.Where(x =>
                        x.StudentName.ToLower().Contains(param.Search.ToLower()) ||
                        x.AppoitmentName.ToLower().Contains(param.Search.ToLower()));
            }

            //ordering
            switch (param.OrderBy)
            {
                case "AppoitmentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AppoitmentName)
                        : query.OrderBy(x => x.AppoitmentName);
                    break;
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;
                case "BinusanId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BinusanId)
                        : query.OrderBy(x => x.BinusanId);
                    break;
                case "InvitationDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.InvitationDate)
                        : query.OrderBy(x => x.InvitationDate);
                    break;
                case "TimeBooked":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.TimeBooked)
                        : query.OrderBy(x => x.TimeBooked);
                    break;
                case "Venue":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Venue)
                        : query.OrderBy(x => x.Venue);
                    break;
                case "TeacherBooked":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.TeacherBooked)
                        : query.OrderBy(x => x.TeacherBooked);
                    break;
            }

            ;

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();


                items = result.Select(x => new GetAppointmentBookingParentResult
                {
                    Id = x.Id,
                    AppoitmentName = x.AppoitmentName,
                    StudentName = x.StudentName,
                    BinusanId = x.BinusanId,
                    InvitationDate = x.InvitationDate.Date,
                    TimeBooked = x.TimeBooked.ToString(),
                    Venue = x.Venue,
                    TeacherBooked = x.TeacherBooked,
                }).ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.Select(x => new GetAppointmentBookingParentResult
                {
                    Id = x.Id,
                    AppoitmentName = x.AppoitmentName,
                    StudentName = x.StudentName,
                    BinusanId = x.BinusanId,
                    InvitationDate = x.InvitationDate.Date,
                    TimeBooked = x.TimeBooked.ToString(),
                    Venue = x.Venue,
                    TeacherBooked = x.TeacherBooked,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request
                .ValidateBody<AddAppointmentBookingParentRequest, AddAppointmentBookingParentValidator>();

            try
            {
                if (_dateTime.ServerTime > body.StartDateTimeInvitation)
                    throw new BadRequestException("Cannot choose availability time less than the current time");

                var invitationBookingSettingSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                    .Include(e => e.InvitationBookingSetting)
                    .Where(e => e.IdUserTeacher == body.IdUserTeacher &&
                                e.IdInvitationBookingSetting == body.IdInvitationBookingSetting &&
                                e.DateInvitation.Date == body.StartDateTimeInvitation.Date &&
                                e.StartTime == body.StartDateTimeInvitation.TimeOfDay &&
                                !e.IsFixedBreak &&
                                (e.IsPriority == true || e.IsPriority == null))
                    .FirstOrDefaultAsync(CancellationToken);

                if (invitationBookingSettingSchedule == null)
                    throw new BadRequestException("Booking failed. The invitation schedule has been changed, please rebook the invitation");

                var newInvitationBooking = new TrInvitationBooking
                {
                    Id = Guid.NewGuid().ToString(),
                    IdVenue = body.IdVenue,
                    IdInvitationBookingSetting = body.IdInvitationBookingSetting,
                    IdUserTeacher = body.IdUserTeacher,
                    StartDateInvitation = body.StartDateTimeInvitation,
                    EndDateInvitation = body.EndDateTimeInvitation,
                    UserIn = body.IdUser,
                    InitiateBy = body.InitiateBy,
                    StatusData = InvitatinBookingStatusData.OnProgress,
                    Description = invitationBookingSettingSchedule.Description
                };
                _dbContext.Entity<TrInvitationBooking>().Add(newInvitationBooking);
                await _dbContext.SaveChangesAsync(CancellationToken);

                var totalCurrentInvitationBooking = await _dbContext.Entity<TrInvitationBooking>().IgnoreQueryFilters()
                    .Where(e => e.IdInvitationBookingSetting == body.IdInvitationBookingSetting
                                && e.IdUserTeacher == body.IdUserTeacher
                                && e.IsActive == true
                                && e.StartDateInvitation == body.StartDateTimeInvitation
                                && (e.StatusData == InvitatinBookingStatusData.OnProgress ||
                                    e.StatusData == InvitatinBookingStatusData.Success))
                    .CountAsync(CancellationToken);

                if (invitationBookingSettingSchedule.QuotaSlot < totalCurrentInvitationBooking)
                {
                    newInvitationBooking.IsActive = false;
                    newInvitationBooking.StatusData = InvitatinBookingStatusData.Failed;
                    newInvitationBooking.StatusDataMessage = "Slot is full";
                    await _dbContext.SaveChangesAsync(CancellationToken);

                    throw new BadRequestException("Booking failed. The slot is full, please select another schedule");
                }

                var ids = body.IdHomeroomStudents.Select(f => f.IdHomeroomStudent).ToList();

                var scheduleBookingVip = await _dbContext.Entity<TrInvitationEmail>()
                    .Include(e => e.InvitationBookingSetting)
                    .Where(e =>
                        e.IdInvitationBookingSetting == body.IdInvitationBookingSetting &&
                        ids.Contains(e.IdHomeroomStudent) &&
                        _dateTime.ServerTime >= e.InvitationBookingSetting.StaffBookingStartDate &&
                        _dateTime.ServerTime <= e.InvitationBookingSetting.StaffBookingEndDate
                    )
                    .ToListAsync(CancellationToken);

        var scheduleBooking = await _dbContext.Entity<TrInvitationBookingSetting>()
            .Where(e =>
                e.Id == body.IdInvitationBookingSetting &&
                (_dateTime.ServerTime >= e.ParentBookingStartDate && _dateTime.ServerTime <= e.ParentBookingEndDate)
            )
            .ToListAsync(CancellationToken);

                if (!scheduleBookingVip.Any() && !scheduleBooking.Any())
                {
                    newInvitationBooking.IsActive = false;
                    newInvitationBooking.StatusData = InvitatinBookingStatusData.Failed;
                    newInvitationBooking.StatusDataMessage = "You cant booking invitation booking";
                    await _dbContext.SaveChangesAsync(CancellationToken);

                    throw new BadRequestException("Booking failed. Booking period has been ended");
                }

                var listInvitationBookingDetail = await _dbContext.Entity<TrInvitationBookingDetail>()
                    .Where(e =>
                        e.InvitationBooking.IdInvitationBookingSetting == body.IdInvitationBookingSetting &&
                        ids.Contains(e.IdHomeroomStudent) &&
                        e.InvitationBooking.IdUserTeacher == body.IdUserTeacher)
                    .ToListAsync(CancellationToken);

                if (listInvitationBookingDetail.Any())
                {
                    newInvitationBooking.IsActive = false;
                    newInvitationBooking.StatusData = InvitatinBookingStatusData.Failed;
                    newInvitationBooking.StatusDataMessage = "You already create invitation booking, you can reschedule";
                    await _dbContext.SaveChangesAsync(CancellationToken);
                    throw new BadRequestException("You already create invitation booking, you can reschedule");
                }

                // BEGIN :: When passed all validation
                newInvitationBooking.StatusData = InvitatinBookingStatusData.Success;

                var listNewInvitationBookingDetail = body.IdHomeroomStudents.Select(x => new TrInvitationBookingDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdHomeroomStudent = x.IdHomeroomStudent,
                    IdInvitationBooking = newInvitationBooking.Id,
                    UserIn = body.IdUser
                }).ToList();

                _dbContext.Entity<TrInvitationBookingDetail>().AddRange(listNewInvitationBookingDetail);

                //set priority
                if (invitationBookingSettingSchedule.IsPriority == null)
                {
                    invitationBookingSettingSchedule.IsPriority = true;
                    invitationBookingSettingSchedule.IsFlexibleBreak = false;
                    invitationBookingSettingSchedule.IsDisabledPriority = false;
                    invitationBookingSettingSchedule.IsDisabledFlexible = true;
                    invitationBookingSettingSchedule.IsDisabledAvailable = true;
                    invitationBookingSettingSchedule.IdUserSetPriority =
                        invitationBookingSettingSchedule.IdUserSetPriority == null
                            ? body.IdUser
                            : invitationBookingSettingSchedule.IdUserSetPriority;

                    // entity already tracked
                    //_dbContext.Entity<TrInvitationBookingSettingSchedule>().UpdateRange(invitationBookingSettingSchedule);
                }

                //set not priority yang berisian
                var dt = body.StartDateTimeInvitation.Date;
                var listInvitationBookingSettingSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                    .Where(e => e.IdUserTeacher == body.IdUserTeacher &&
                                e.IdInvitationBookingSetting != body.IdInvitationBookingSetting &&
                                e.DateInvitation.Date == dt &&
                                e.StartTime == body.StartDateTimeInvitation.TimeOfDay &&
                                !e.IsFixedBreak &&
                                (e.IsPriority == null || e.IsPriority == true))
                    .ToListAsync(CancellationToken);

                foreach (var item in listInvitationBookingSettingSchedule)
                {
                    item.IsPriority = false;
                    item.IsFlexibleBreak = false;
                    item.IsDisabledPriority = true;
                    item.IsDisabledFlexible = true;
                    item.IsDisabledAvailable = true;
                }
                // END :: When passed all validation

                var invitationBookingSettingScheduleIds = new List<string>();
                if (listInvitationBookingSettingSchedule.Any())
                    invitationBookingSettingScheduleIds = listInvitationBookingSettingSchedule.Select(x => x.Id).Distinct().ToList();

                //refactor, remove calculation CPU date translation
                var startDate = new DateTime(body.StartDateTimeInvitation.Year, body.StartDateTimeInvitation.Month,
                    body.StartDateTimeInvitation.Day, 0, 0, 0);
                var endDate = new DateTime(body.EndDateTimeInvitation.Year, body.EndDateTimeInvitation.Month,
                    body.EndDateTimeInvitation.Day, 23, 23, 59);

                var sw = Stopwatch.StartNew();

                var invitationBookingSettingSchedules = listInvitationBookingSettingSchedule.Any() ?
                    await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                    .Include(x => x.InvitationBookingSetting)
                    .Where(e => e.DateInvitation >= startDate &&
                                e.DateInvitation <= endDate &&
                                (e.IsPriority == true || e.IsFlexibleBreak == true || e.IsFixedBreak == true) &&
                                !invitationBookingSettingScheduleIds.Contains(e.Id))
                    .AsNoTracking()
                    .ToListAsync(CancellationToken) :
                    await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                    .Include(x => x.InvitationBookingSetting)
                    .Where(e => e.DateInvitation >= startDate &&
                                e.DateInvitation <= endDate &&
                                (e.IsPriority == true || e.IsFlexibleBreak == true || e.IsFixedBreak == true))
                    .AsNoTracking()
                    .ToListAsync(CancellationToken);

                sw.Stop();

                var totalSeconds = Math.Round(sw.Elapsed.TotalSeconds, 2);
                Logger.LogInformation("Get invitation booking setting schedules takes {TotalSeconds}s", totalSeconds);

                //remapping invitationbookings, biar tidak round-trip
                if (invitationBookingSettingSchedules.Any())
                {
                    var invitationBookinSettingIds = invitationBookingSettingSchedules
                        .Select(e => e.InvitationBookingSetting.Id).Distinct().ToList();
                    var invitationBookings = await _dbContext.Entity<TrInvitationBooking>()
                        .Where(e => invitationBookinSettingIds.Contains(e.IdInvitationBookingSetting))
                        .AsNoTracking()
                        .ToListAsync(CancellationToken);

                    foreach (var item in invitationBookingSettingSchedules)
                        item.InvitationBookingSetting.InvitationBookings = invitationBookings
                            .Where(e => e.IdInvitationBookingSetting == item.IdInvitationBookingSetting).ToList();
                }

                //inmemory 
                var isParent = invitationBookingSettingSchedules
                    .Where(e =>
                        e.IdUserTeacher == body.IdUserTeacher &&
                        e.InvitationBookingSetting.InvitationBookings.Any(x =>
                            x.InitiateBy == InvitationBookingInitiateBy.Parent) &&
                        ((body.StartDateTimeInvitation.TimeOfDay >= e.StartTime &&
                          body.StartDateTimeInvitation.TimeOfDay < e.EndTime) ||
                         (body.EndDateTimeInvitation.TimeOfDay > e.StartTime &&
                          body.EndDateTimeInvitation.TimeOfDay < e.EndTime))
                    ).Any();

                foreach (var item in invitationBookingSettingSchedules)
                    item.InvitationBookingSetting.InvitationBookings.Clear();

                //inmemory 
                var accessDisabled = invitationBookingSettingSchedules
                    .Where(e => e.IdUserTeacher == body.IdUserTeacher
                                && ((body.StartDateTimeInvitation.TimeOfDay >= e.StartTime &&
                                     body.StartDateTimeInvitation.TimeOfDay < e.EndTime) ||
                                    (body.EndDateTimeInvitation.TimeOfDay > e.StartTime &&
                                     body.EndDateTimeInvitation.TimeOfDay < e.EndTime))
                    ).ToList();

                foreach (var item in accessDisabled)
                {
                    if (item.IdInvitationBookingSetting == body.IdInvitationBookingSetting)
                    {
                        if (isParent)
                            item.IsPriority = true;
                    }
                    else
                        _dbContext.Entity<TrInvitationBookingSettingSchedule>().Attach(item);

                    item.IsDisabledPriority = isParent;
                }

                var exsisInvitationBookingSetting = await _dbContext.Entity<TrInvitationBookingSetting>()
                                                            .Where(e => e.Id == body.IdInvitationBookingSetting)
                                                            .AnyAsync(CancellationToken);

                if (!exsisInvitationBookingSetting)
                    throw new BadRequestException("The request cannot be processed. The invitation has been deleted by staff");

                await _dbContext.SaveChangesAsync(CancellationToken);

                //result if not initaited by parent
                if (body.InitiateBy != InvitationBookingInitiateBy.Parent)
                    return Request.CreateApiResult2();

                //BEGIN Email if parent
                var invitationBookingEmail = await _dbContext.Entity<TrInvitationBooking>()
                    //comment for integral bugs
                    //.Include(e => e.InvitationBookingDetails).ThenInclude(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                    .Include(e => e.InvitationBookingSetting).ThenInclude(e => e.AcademicYears)
                    .Include(e => e.Venue)
                    .Include(e => e.UserTeacher)
                    .Include(x => x.InvitationBookingSetting).ThenInclude(x => x.InvitationBookingSettingVenueDates)
                    .Where(e => e.Id == newInvitationBooking.Id)
                    .Select(x => new InvitationBookingEmailDto
                    {
                        Id = x.InvitationBookingSetting.Id,
                        IdInvitationBooking = x.Id,
                        AcademicYears = x.InvitationBookingSetting.AcademicYears.Description,
                        InvitationName = x.InvitationBookingSetting.InvitationName,
                        InvitationStartDate = x.InvitationBookingSetting.InvitationStartDate,
                        InvitationEndDate = x.InvitationBookingSetting.InvitationEndDate,
                        //remove this add another flow after, prevent integral bugs in efcore 3.1
                        //StudentDetail = x.InvitationBookingDetails.Select(x => new DetailStudents
                        //{
                        //    StudentName = x.HomeroomStudent.Student.FirstName
                        //                  + (x.HomeroomStudent.Student.MiddleName == null
                        //                      ? ""
                        //                      : " " + x.HomeroomStudent.Student.MiddleName)
                        //                  + (x.HomeroomStudent.Student.LastName == null
                        //                      ? ""
                        //                      : " " + x.HomeroomStudent.Student.LastName),
                        //    StudentId = x.HomeroomStudent.IdStudent,
                        //}).ToList(),
                        Date = x.StartDateInvitation,
                        Venue = x.Venue.Description,
                        Time = x.StartDateInvitation,
                        IdUserTeacher = x.IdUserTeacher,
                        IdUserParent = x.UserIn,
                        IdSchool = x.InvitationBookingSetting.AcademicYears.IdSchool,
                        IdInvitationBookingSettingVenueDates = x.InvitationBookingSetting.InvitationBookingSettingVenueDates.Select(x => x.Id).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync(CancellationToken);

                if (invitationBookingEmail is null)
                {
                    Logger.LogWarning("Initiated by parent but newInvitationBooking is null of ID : {ID}",
                        newInvitationBooking.Id);
                    return Request.CreateApiResult2();
                }

                var invitationBookingDetails = await _dbContext.Entity<TrInvitationBookingDetail>()
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Venue)
                    .Where(e => e.IdInvitationBooking == invitationBookingEmail.IdInvitationBooking)
                    .AsNoTracking()
                    .ToListAsync(CancellationToken);

                foreach (var x in invitationBookingDetails)
                {
                    invitationBookingEmail.StudentDetail.Add(new DetailStudents
                    {
                        StudentName = NameUtil.GenerateFullName(x.HomeroomStudent?.Student?.FirstName ?? "", x.HomeroomStudent?.Student?.LastName ?? "").Trim(),
                        StudentId = x.HomeroomStudent.IdStudent,
                        Venue = x.HomeroomStudent.Homeroom.Venue.Description,
                        IdHomeroomStudent = x.IdHomeroomStudent
                    });
                };

                var listIdHomeroomStudentBody = body.IdHomeroomStudents.Select(e => e.IdHomeroomStudent).ToList();

                var emailInvitationBookingParentResult = new EmailInvitatinBookingParentResult
                {
                    IdInvitationBookingSetting = invitationBookingEmail.Id,
                    AcademicYear = invitationBookingEmail.AcademicYears,
                    InvitationName = invitationBookingEmail.InvitationName,
                    InvitationDate =
                        Convert.ToDateTime(invitationBookingEmail.InvitationStartDate)
                            .ToString("dd MMM yyyy HH:mm") + " - " + Convert
                            .ToDateTime(invitationBookingEmail.InvitationEndDate).ToString("dd MMM yyyy HH:mm"),
                    StudentName =
                       invitationBookingEmail.StudentDetail.Count() > 1
                        ? ConvertString(invitationBookingEmail.StudentDetail.Select(e => e.StudentName).ToList())
                        : invitationBookingEmail.StudentDetail.Where(e=> listIdHomeroomStudentBody.Contains(e.IdHomeroomStudent)).Select(e => e.StudentName).FirstOrDefault(),
                    BinusianId = invitationBookingEmail.StudentDetail.Count() > 1
                        ? ConvertString(invitationBookingEmail.StudentDetail.Select(e => e.StudentId).ToList())
                        : invitationBookingEmail.StudentDetail.Where(e => listIdHomeroomStudentBody.Contains(e.IdHomeroomStudent)).Select(e => e.StudentId).FirstOrDefault(),
                    Venue = invitationBookingEmail.Venue,
                    Date = invitationBookingEmail.Date.ToString("dd MMM yyyy"),
                    Time = invitationBookingEmail.Time.ToString("HH:mm"),
                    IdTeacher = invitationBookingEmail.IdUserTeacher,
                    IdUserParent = invitationBookingEmail.IdUserParent,
                    IdSchool = invitationBookingEmail.IdSchool
                };

                if (KeyValues.ContainsKey("GetInvitationBookingEmail"))
                    KeyValues.Remove("GetInvitationBookingEmail");

                KeyValues.Add("GetInvitationBookingEmail", emailInvitationBookingParentResult);
                var notification = APP19Notification(KeyValues, AuthInfo);

                if (KeyValues.ContainsKey("GetInvitationBookingParentEmail"))
                    KeyValues.Remove("GetInvitationBookingParentEmail");

                KeyValues.Add("GetInvitationBookingParentEmail", emailInvitationBookingParentResult);
                var notificationParent = APP27Notification(KeyValues, AuthInfo);

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    throw new BadRequestException("Booking failed. The invitation has been deleted");
                else
                    throw new BadRequestException(ex.Message);
            }

            return Request.CreateApiResult2();
        }

        private class InvitationBookingEmailDto
        {
            public InvitationBookingEmailDto()
            {
                StudentDetail = new List<DetailStudents>();
            }

            public string Id { get; set; }
            public string IdInvitationBooking { get; set; }
            public string AcademicYears { get; set; }
            public string InvitationName { get; set; }
            public DateTime InvitationStartDate { get; set; }
            public DateTime InvitationEndDate { get; set; }
            public DateTime Date { get; set; }
            public string Venue { get; set; }
            public DateTime Time { get; set; }
            public string IdUserTeacher { get; set; }
            public string UserIn { get; set; }
            public string IdSchool { get; set; }
            public string IdUserParent { get; set; }
            public string IdInvitationBookingSettingVenueDates { get; set; }
            public List<DetailStudents> StudentDetail { get; set; }
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request
                .ValidateBody<ApsentAppointmentBookingParentRequest, ApsentAppointmentBookingParentValidator>();

            var GetInvitationBookingDetail = await _dbContext.Entity<TrInvitationBooking>()
                .Where(e => body.Absents.Select(f => f.IdInvitationBooking).ToList().Contains(e.Id))
                .ToListAsync(CancellationToken);

            foreach (var ItemBody in body.Absents)
            {
                var GetInvitationBookingById = GetInvitationBookingDetail
                    .Where(e => e.Id == ItemBody.IdInvitationBooking)
                    .FirstOrDefault();

                GetInvitationBookingById.Status = (InvitationBookingStatus)ItemBody.Status;
                GetInvitationBookingById.Note = ItemBody.Note;
                _dbContext.Entity<TrInvitationBooking>().Update(GetInvitationBookingById);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        private string ConvertString(List<string> List)
        {
            var ValueStirng = "";

            foreach (var item in List)
            {
                ValueStirng += ValueStirng == "" ? item : ", " + item;
            }

            return ValueStirng;
        }

        public static string APP19Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetInvitationBookingEmail").Value;
            var EmailInvitation =
                JsonConvert.DeserializeObject<EmailInvitatinBookingParentResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP19")
                {
                    IdRecipients = new List<string>()
                    {
                        EmailInvitation.IdTeacher,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }

            return "";
        }

        public static string APP27Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetInvitationBookingParentEmail").Value;
            var EmailInvitation =
                JsonConvert.DeserializeObject<EmailInvitatinBookingParentResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP27")
                {
                    IdRecipients = new List<string>()
                    {
                        EmailInvitation.IdUserParent,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }

            return "";
        }
    }
}
