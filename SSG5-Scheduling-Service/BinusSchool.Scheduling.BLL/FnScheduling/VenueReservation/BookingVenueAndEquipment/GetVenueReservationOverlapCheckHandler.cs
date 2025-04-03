using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class GetVenueReservationOverlapCheckHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly GetVenueReservationUserLoginSpecialRoleHandler _userLoginSpecialRoleHandler;

        public GetVenueReservationOverlapCheckHandler(ISchedulingDbContext context, GetVenueReservationUserLoginSpecialRoleHandler userLoginSpecialRoleHandler)
        {
            _context = context;
            _userLoginSpecialRoleHandler = userLoginSpecialRoleHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<GetVenueReservationOverlapCheckRequest, GetVenueReservationOverlapCheckValidator>();

            var response = await GetVenueReservationOverlapCheck(new GetVenueReservationOverlapCheckRequest
            {
                IdVenueReservation = request.IdVenueReservation,
                IdVenue = request.IdVenue,
                Requester = request.Requester,
                EventDescription = request.EventDescription,
                ScheduleStartDate = request.ScheduleStartDate,
                ScheduleEndDate = request.ScheduleEndDate,
                Recurrence = request.Recurrence,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PreparationTime = request.PreparationTime,
                CleanUpTime = request.CleanUpTime,
                Note = request.Note,
                FileUpload = request.FileUpload,
                Equipments = request.Equipments
            });

            return Request.CreateApiResult2(response as object);
        }

        public async Task<GetVenueReservationOverlapCheckResponse> GetVenueReservationOverlapCheck(GetVenueReservationOverlapCheckRequest request)
        {
            var response = new GetVenueReservationOverlapCheckResponse();
            var equipmentData = new List<GetVenueReservationOverlapCheckResponse_Equipment>();
            var bookingData = new List<GetVenueReservationOverlapCheckResponse_Booking>();

            if (request.EndTime <= request.StartTime)
                throw new BadRequestException("End Time must be greater than Start Time and cannot be the same as Start Time");

            if (request.StartTime - TimeSpan.FromMinutes(request.PreparationTime ?? 0) < TimeSpan.Zero)
                throw new BadRequestException("The start time with preparation time cannot be pushed back to the previous day.");

            if (request.EndTime + TimeSpan.FromMinutes(request.CleanUpTime ?? 0) > new TimeSpan(23, 59, 59))
                throw new BadRequestException("The end time with clean up time cannot be advanced to the next day.");

            var checkUserSpecialRole = await _userLoginSpecialRoleHandler.GetVenueReservationUserLoginSpecialRole(new GetVenueReservationUserLoginSpecialRoleRequest
            {
                IdUser = request.Requester
            });

            TimeSpan StartTime = request.StartTime.Subtract(TimeSpan.FromMinutes(request.PreparationTime ?? 0));
            TimeSpan EndTime = request.EndTime.Add(TimeSpan.FromMinutes(request.CleanUpTime ?? 0));

            var idSchool = await _context.Entity<MsUserSchool>()
                .Where(a => a.IdUser == request.Requester)
                .FirstOrDefaultAsync(CancellationToken);

            var getVenueReservationRule = await _context.Entity<MsVenueReservationRule>()
                .Where(a => a.IdSchool == idSchool.IdSchool)
                .FirstOrDefaultAsync(CancellationToken);

            var isNeedApproval = await _context.Entity<MsVenueMapping>()
                .Where(a => a.IdVenue == request.IdVenue)
                .OrderByDescending(a => a.DateIn)
                .FirstOrDefaultAsync(CancellationToken);

            var getEquipmentReservation = await _context.Entity<TrEquipmentReservation>()
                .Include(a => a.MappingEquipmentReservation)
                .Where(a => request.ScheduleStartDate <= a.MappingEquipmentReservation.ScheduleEndDate.Date
                    && request.ScheduleEndDate >= a.MappingEquipmentReservation.ScheduleStartDate.Date)
                .ToListAsync(CancellationToken);

            // create
            if (string.IsNullOrEmpty(request.IdVenueReservation))
            {
                // range date
                if (request.ScheduleStartDate != request.ScheduleEndDate)
                {
                    TimeSpan difference = request.ScheduleEndDate - request.ScheduleStartDate;
                    int totalDays = difference.Days + 1;
                    DateTime startDate = request.ScheduleStartDate;

                    if (checkUserSpecialRole == null)
                    {
                        bool isValid = totalDays > getVenueReservationRule.MaxDayDurationBookingVenue;

                        if (isValid)
                            throw new BadRequestException($"You reach booking rule limit, max {getVenueReservationRule.MaxDayBookingVenue} day(s).");
                    }
                    else
                    {
                        bool isValid = totalDays > checkUserSpecialRole.SpecialDurationBookingTotalDay;

                        if (isValid)
                            throw new BadRequestException($"You reach booking rule limit, max {checkUserSpecialRole.SpecialDurationBookingTotalDay} day(s).");
                    }

                    // no recurrence
                    if (request.Recurrence == null)
                    {
                        for (int i = 1; i <= totalDays; i++)
                        {
                            var additionalEquipmentData = new List<GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment>();

                            var filterEquipmentByDate = getEquipmentReservation
                                .Where(a => startDate.Add(StartTime) < a.MappingEquipmentReservation.ScheduleEndDate
                                    && startDate.Add(EndTime) > a.MappingEquipmentReservation.ScheduleStartDate)
                                .ToList();

                            foreach (var equipment in request.Equipments)
                            {
                                var getEquipment = await _context.Entity<MsEquipment>()
                                    .Where(a => a.Id == equipment.IdEquipment)
                                    .FirstOrDefaultAsync(CancellationToken);

                                var totalEquipmentReserve = filterEquipmentByDate
                                    .Where(a => a.IdEquipment == equipment.IdEquipment)
                                    .Select(a => a.EquipmentBorrowingQty)
                                    .Sum();

                                if ((getEquipment.TotalStockQty - totalEquipmentReserve) < equipment.EquipmentBorrowingQty)
                                {
                                    var insertEquipment = new GetVenueReservationOverlapCheckResponse_Equipment
                                    {
                                        ScheduleDate = startDate,
                                        StartTime = StartTime,
                                        EndTime = EndTime,
                                        EquipmentName = getEquipment.EquipmentName,
                                        IdEquipment = getEquipment.Id,
                                        CurrentStockAvailable = (getEquipment.TotalStockQty - totalEquipmentReserve),
                                        BorrowingQty = equipment.EquipmentBorrowingQty
                                    };

                                    equipmentData.Add(insertEquipment);
                                }

                                var insertAdditionalEquipment = new GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment
                                {
                                    IdEquipment = getEquipment.Id,
                                    EquipmentBorrowingQty = equipment.EquipmentBorrowingQty,
                                };

                                additionalEquipmentData.Add(insertAdditionalEquipment);
                            }

                            var insertBooking = new GetVenueReservationOverlapCheckResponse_Booking
                            {
                                IdVenueReservation = null,
                                IdVenue = request.IdVenue,
                                Requester = request.Requester,
                                EventDescription = request.EventDescription,
                                ScheduleDate = startDate,
                                StartTime = request.StartTime,
                                EndTime = request.EndTime,
                                PreparationTime = request.PreparationTime,
                                CleanUpTime = request.CleanUpTime,
                                Note = request.Note,
                                Overlapping = CheckOverlapSchedule(idSchool.IdSchool, null, request.IdVenue, startDate, StartTime, EndTime),
                                NeedApproval = isNeedApproval.IsNeedApproval,
                                FileUpload = request.FileUpload == null ? null
                                        : new GetVenueReservationOverlapCheckResponse_Booking_File()
                                        {
                                            FileName = request.FileUpload.FileName,
                                            FileType = request.FileUpload.FileType,
                                            FileSize = request.FileUpload.FileSize,
                                            Url = request.FileUpload.Url,
                                        },
                                AdditionalEquipments = additionalEquipmentData,
                            };

                            bookingData.Add(insertBooking);

                            startDate = startDate.AddDays(1);
                        }

                        response.Equipments = equipmentData;
                        response.Bookings = bookingData;
                    }

                    // recurrence
                    else
                    {
                        var recurrenceDays = request.Recurrence.Select(int.Parse).ToArray();

                        for (int i = 1; i <= totalDays; i++)
                        {
                            var additionalEquipmentData = new List<GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment>();

                            if (recurrenceDays.Contains((int)startDate.DayOfWeek))
                            {
                                var filterEquipmentByDate = getEquipmentReservation
                                .Where(a => startDate.Add(StartTime) < a.MappingEquipmentReservation.ScheduleEndDate
                                    && startDate.Add(EndTime) > a.MappingEquipmentReservation.ScheduleStartDate)
                                .ToList();

                                foreach (var equipment in request.Equipments)
                                {
                                    var getEquipment = await _context.Entity<MsEquipment>()
                                        .Where(a => a.Id == equipment.IdEquipment)
                                        .FirstOrDefaultAsync(CancellationToken);

                                    var totalEquipmentReserve = filterEquipmentByDate
                                        .Where(a => a.IdEquipment == equipment.IdEquipment)
                                        .Select(a => a.EquipmentBorrowingQty)
                                        .Sum();

                                    if ((getEquipment.TotalStockQty - totalEquipmentReserve) < equipment.EquipmentBorrowingQty)
                                    {
                                        var insertEquipment = new GetVenueReservationOverlapCheckResponse_Equipment
                                        {
                                            ScheduleDate = startDate,
                                            StartTime = StartTime,
                                            EndTime = EndTime,
                                            EquipmentName = getEquipment.EquipmentName,
                                            IdEquipment = getEquipment.Id,
                                            CurrentStockAvailable = (getEquipment.TotalStockQty - totalEquipmentReserve),
                                            BorrowingQty = equipment.EquipmentBorrowingQty
                                        };

                                        equipmentData.Add(insertEquipment);
                                    }

                                    var insertAdditionalEquipment = new GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment
                                    {
                                        IdEquipment = getEquipment.Id,
                                        EquipmentBorrowingQty = equipment.EquipmentBorrowingQty,
                                    };

                                    additionalEquipmentData.Add(insertAdditionalEquipment);
                                }

                                var insertBooking = new GetVenueReservationOverlapCheckResponse_Booking
                                {
                                    IdVenueReservation = null,
                                    IdVenue = request.IdVenue,
                                    Requester = request.Requester,
                                    EventDescription = request.EventDescription,
                                    ScheduleDate = startDate,
                                    StartTime = request.StartTime,
                                    EndTime = request.EndTime,
                                    PreparationTime = request.PreparationTime,
                                    CleanUpTime = request.CleanUpTime,
                                    Note = request.Note,
                                    Overlapping = CheckOverlapSchedule(idSchool.IdSchool, null, request.IdVenue, startDate, StartTime, EndTime),
                                    NeedApproval = isNeedApproval.IsNeedApproval,
                                    FileUpload = request.FileUpload == null ? null
                                            : new GetVenueReservationOverlapCheckResponse_Booking_File()
                                            {
                                                FileName = request.FileUpload.FileName,
                                                FileType = request.FileUpload.FileType,
                                                FileSize = request.FileUpload.FileSize,
                                                Url = request.FileUpload.Url,
                                            },
                                    AdditionalEquipments = additionalEquipmentData,
                                };

                                bookingData.Add(insertBooking);
                            }
                            startDate = startDate.AddDays(1);
                        }

                        response.Equipments = equipmentData;
                        response.Bookings = bookingData;
                    }
                }
                // same date, recurrence
                else if (request.ScheduleStartDate == request.ScheduleEndDate && request.Recurrence != null)
                {
                    var recurrenceDays = request.Recurrence.Select(int.Parse).ToArray();

                    if (recurrenceDays.Contains((int)request.ScheduleStartDate.DayOfWeek))
                    {
                        var additionalEquipmentData = new List<GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment>();

                        var filterEquipmentByDate = getEquipmentReservation
                                .Where(a => request.ScheduleStartDate.Add(StartTime) < a.MappingEquipmentReservation.ScheduleEndDate
                                    && request.ScheduleStartDate.Add(EndTime) > a.MappingEquipmentReservation.ScheduleStartDate)
                                .ToList();

                        foreach (var equipment in request.Equipments)
                        {
                            var getEquipment = await _context.Entity<MsEquipment>()
                                .Where(a => a.Id == equipment.IdEquipment)
                                .FirstOrDefaultAsync(CancellationToken);

                            var totalEquipmentReserve = filterEquipmentByDate
                                .Where(a => a.IdEquipment == equipment.IdEquipment)
                                .Select(a => a.EquipmentBorrowingQty)
                                .Sum();

                            if ((getEquipment.TotalStockQty - totalEquipmentReserve) < equipment.EquipmentBorrowingQty)
                            {
                                var insertEquipment = new GetVenueReservationOverlapCheckResponse_Equipment
                                {
                                    ScheduleDate = request.ScheduleStartDate,
                                    StartTime = StartTime,
                                    EndTime = EndTime,
                                    EquipmentName = getEquipment.EquipmentName,
                                    IdEquipment = getEquipment.Id,
                                    CurrentStockAvailable = (getEquipment.TotalStockQty - totalEquipmentReserve),
                                    BorrowingQty = equipment.EquipmentBorrowingQty
                                };

                                equipmentData.Add(insertEquipment);
                            }

                            var insertAdditionalEquipment = new GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment
                            {
                                IdEquipment = getEquipment.Id,
                                EquipmentBorrowingQty = equipment.EquipmentBorrowingQty,
                            };

                            additionalEquipmentData.Add(insertAdditionalEquipment);
                        }

                        var insertBooking = new GetVenueReservationOverlapCheckResponse_Booking
                        {
                            IdVenueReservation = null,
                            IdVenue = request.IdVenue,
                            Requester = request.Requester,
                            EventDescription = request.EventDescription,
                            ScheduleDate = request.ScheduleStartDate,
                            StartTime = request.StartTime,
                            EndTime = request.EndTime,
                            PreparationTime = request.PreparationTime,
                            CleanUpTime = request.CleanUpTime,
                            Note = request.Note,
                            Overlapping = CheckOverlapSchedule(idSchool.IdSchool, null, request.IdVenue, request.ScheduleStartDate, StartTime, EndTime),
                            NeedApproval = isNeedApproval.IsNeedApproval,
                            FileUpload = request.FileUpload == null ? null
                                    : new GetVenueReservationOverlapCheckResponse_Booking_File()
                                    {
                                        FileName = request.FileUpload.FileName,
                                        FileType = request.FileUpload.FileType,
                                        FileSize = request.FileUpload.FileSize,
                                        Url = request.FileUpload.Url,
                                    },
                            AdditionalEquipments = additionalEquipmentData,
                        };

                        bookingData.Add(insertBooking);
                    }

                    response.Equipments = equipmentData;
                    response.Bookings = bookingData;
                }

                // no recurrence
                else
                {
                    var additionalEquipmentData = new List<GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment>();

                    var filterEquipmentByDate = getEquipmentReservation
                                .Where(a => request.ScheduleStartDate.Add(StartTime) < a.MappingEquipmentReservation.ScheduleEndDate
                                    && request.ScheduleStartDate.Add(EndTime) > a.MappingEquipmentReservation.ScheduleStartDate)
                                .ToList();

                    foreach (var equipment in request.Equipments)
                    {
                        var getEquipment = await _context.Entity<MsEquipment>()
                            .Where(a => a.Id == equipment.IdEquipment)
                            .FirstOrDefaultAsync(CancellationToken);

                        var totalEquipmentReserve = filterEquipmentByDate
                            .Where(a => a.IdEquipment == equipment.IdEquipment)
                            .Select(a => a.EquipmentBorrowingQty)
                            .Sum();

                        if ((getEquipment.TotalStockQty - totalEquipmentReserve) < equipment.EquipmentBorrowingQty)
                        {
                            var insertEquipment = new GetVenueReservationOverlapCheckResponse_Equipment
                            {
                                ScheduleDate = request.ScheduleStartDate,
                                StartTime = StartTime,
                                EndTime = EndTime,
                                EquipmentName = getEquipment.EquipmentName,
                                IdEquipment = getEquipment.Id,
                                CurrentStockAvailable = (getEquipment.TotalStockQty - totalEquipmentReserve),
                                BorrowingQty = equipment.EquipmentBorrowingQty
                            };

                            equipmentData.Add(insertEquipment);
                        }

                        var insertAdditionalEquipment = new GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment
                        {
                            IdEquipment = getEquipment.Id,
                            EquipmentBorrowingQty = equipment.EquipmentBorrowingQty,
                        };

                        additionalEquipmentData.Add(insertAdditionalEquipment);
                    }

                    var insertBooking = new GetVenueReservationOverlapCheckResponse_Booking
                    {
                        IdVenueReservation = null,
                        IdVenue = request.IdVenue,
                        Requester = request.Requester,
                        EventDescription = request.EventDescription,
                        ScheduleDate = request.ScheduleStartDate,
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                        PreparationTime = request.PreparationTime,
                        CleanUpTime = request.CleanUpTime,
                        Note = request.Note,
                        Overlapping = CheckOverlapSchedule(idSchool.IdSchool, null, request.IdVenue, request.ScheduleStartDate, StartTime, EndTime),
                        NeedApproval = isNeedApproval.IsNeedApproval,
                        FileUpload = request.FileUpload == null ? null
                                : new GetVenueReservationOverlapCheckResponse_Booking_File()
                                {
                                    FileName = request.FileUpload.FileName,
                                    FileType = request.FileUpload.FileType,
                                    FileSize = request.FileUpload.FileSize,
                                    Url = request.FileUpload.Url,
                                },
                        AdditionalEquipments = additionalEquipmentData,
                    };

                    bookingData.Add(insertBooking);

                    response.Equipments = equipmentData;
                    response.Bookings = bookingData;
                }
            }
            // edit
            else
            {
                var additionalEquipmentData = new List<GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment>();

                var filterEquipmentByDate = getEquipmentReservation
                                .Where(a => request.ScheduleStartDate.Add(StartTime) < a.MappingEquipmentReservation.ScheduleEndDate
                                    && request.ScheduleStartDate.Add(EndTime) > a.MappingEquipmentReservation.ScheduleStartDate)
                                .ToList();

                foreach (var equipment in request.Equipments)
                {
                    var getEquipment = await _context.Entity<MsEquipment>()
                        .Where(a => a.Id == equipment.IdEquipment)
                        .FirstOrDefaultAsync(CancellationToken);

                    var totalEquipmentReserve = filterEquipmentByDate
                        .Where(a => a.IdEquipment == equipment.IdEquipment
                            && a.MappingEquipmentReservation.IdVenueReservation != request.IdVenueReservation)
                        .Select(a => a.EquipmentBorrowingQty)
                        .Sum();

                    if ((getEquipment.TotalStockQty - totalEquipmentReserve) < equipment.EquipmentBorrowingQty)
                    {
                        var insertEquipment = new GetVenueReservationOverlapCheckResponse_Equipment
                        {
                            ScheduleDate = request.ScheduleStartDate,
                            StartTime = StartTime,
                            EndTime = EndTime,
                            EquipmentName = getEquipment.EquipmentName,
                            IdEquipment = getEquipment.Id,
                            CurrentStockAvailable = (getEquipment.TotalStockQty - totalEquipmentReserve),
                            BorrowingQty = equipment.EquipmentBorrowingQty
                        };

                        equipmentData.Add(insertEquipment);
                    }

                    var insertAdditionalEquipment = new GetVenueReservationOverlapCheckResponse_Booking_AdditionalEquipment
                    {
                        IdEquipment = getEquipment.Id,
                        EquipmentBorrowingQty = equipment.EquipmentBorrowingQty,
                    };

                    additionalEquipmentData.Add(insertAdditionalEquipment);
                }

                var insertBooking = new GetVenueReservationOverlapCheckResponse_Booking
                {
                    IdVenueReservation = request.IdVenueReservation,
                    IdVenue = request.IdVenue,
                    Requester = request.Requester,
                    EventDescription = request.EventDescription,
                    ScheduleDate = request.ScheduleStartDate,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    PreparationTime = request.PreparationTime,
                    CleanUpTime = request.CleanUpTime,
                    Note = request.Note,
                    Overlapping = CheckOverlapSchedule(idSchool.IdSchool, request.IdVenueReservation, request.IdVenue, request.ScheduleStartDate, StartTime, EndTime),
                    NeedApproval = isNeedApproval.IsNeedApproval,
                    FileUpload = request.FileUpload == null ? null
                            : new GetVenueReservationOverlapCheckResponse_Booking_File()
                            {
                                FileName = request.FileUpload.FileName,
                                FileType = request.FileUpload.FileType,
                                FileSize = request.FileUpload.FileSize,
                                Url = request.FileUpload.Url,
                            },
                    AdditionalEquipments = additionalEquipmentData,
                };

                bookingData.Add(insertBooking);

                response.Equipments = equipmentData;
                response.Bookings = bookingData;
            }

            return response;
        }

        private bool CheckOverlapSchedule(string idSchool, string idVenueReservation, string idVenue, DateTime scheduleDate, TimeSpan startTime, TimeSpan endTime)
        {
            List<int> visibleVenueStatuses = new List<VenueApprovalStatus>
            {
                VenueApprovalStatus.Approved,
                VenueApprovalStatus.WaitingForApproval,
                VenueApprovalStatus.NoNeedApproval,
            }.Cast<int>().ToList();

            #region get active academic year
            var checkActivePeriod = _context.Entity<MsPeriod>()
                .Include(a => a.Grade.Level.AcademicYear)
                .Where(a => a.Grade.Level.AcademicYear.IdSchool == idSchool
                    && scheduleDate.Date >= a.StartDate.Date
                    && scheduleDate.Date <= a.EndDate.Date)
                .Select(a => a.Grade.Level.IdAcademicYear)
                .FirstOrDefault();

            var getLatestAcademicYear = _context.Entity<MsPeriod>()
                .Include(a => a.Grade.Level.AcademicYear)
                .Where(a => a.Grade.Level.AcademicYear.IdSchool == idSchool
                    && scheduleDate.Date < a.EndDate.Date)
                .Select(a => a.Grade.Level.IdAcademicYear)
                .FirstOrDefault();
            #endregion

            var venueMapping = _context.Entity<MsVenueMapping>()
                .Include(a => a.Floor)
                .Where(a => a.IdVenue == idVenue
                    && (checkActivePeriod.Any() ? a.IdAcademicYear == checkActivePeriod : a.IdAcademicYear == getLatestAcademicYear))
                .FirstOrDefault();

            var getScheduleLesson = _context.Entity<MsScheduleLesson>()
                .Include(a => a.Lesson)
                    .ThenInclude(lesson => lesson.LessonTeachers)
                    .ThenInclude(lessonTeacher => lessonTeacher.Staff)
                .Include(a => a.Venue.VenueMappings)
                .Where(a => a.ScheduleDate.Date == scheduleDate)
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToList();

            var getScheduleRealization = _context.Entity<TrScheduleRealization2>()
                .Include(a => a.Venue.VenueMappings)
                .Where(a => a.ScheduleDate.Date == scheduleDate)
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToList();

            var joinSchedule = from scheduleLesson in getScheduleLesson
                               join scheduleRealization in getScheduleRealization
                               on new { scheduleLesson.ScheduleDate, scheduleLesson.IdLesson, scheduleLesson.SessionID }
                               equals new { scheduleRealization.ScheduleDate, scheduleRealization.IdLesson, scheduleRealization.SessionID }
                               select new
                               {
                                   ScheduleLesson = scheduleLesson,
                                   ScheduleRealization = scheduleRealization
                               };

            var scheduleLessonIds = new List<string>();

            if (joinSchedule.Any())
                scheduleLessonIds = joinSchedule.Select(sl => sl.ScheduleLesson.Id).ToList();

            var getScheduleLeesonRequestFloor = _context.Entity<MsScheduleLesson>()
                .Include(a => a.Lesson)
                    .ThenInclude(lesson => lesson.LessonTeachers)
                    .ThenInclude(lessonTeacher => lessonTeacher.Staff)
                .Include(a => a.Venue.VenueMappings)
                .Where(a => a.ScheduleDate.Date == scheduleDate
                    && a.IdVenue == idVenue
                    && !scheduleLessonIds.Contains(a.Id))
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToList();

            // check with schedule lesson

            bool checkWithScheduleLesson = getScheduleLeesonRequestFloor
                .Distinct()
                .Where(a => scheduleDate.Date == a.ScheduleDate.Date
                    && startTime < a.EndTime
                    && endTime > a.StartTime)
                .Any();

            // check with schedule realization

            bool checkWithScheduleRealization = joinSchedule
                .Distinct()
                .Where(a => scheduleDate.Date == a.ScheduleRealization.ScheduleDate.Date
                    && startTime < a.ScheduleRealization.EndTime
                    && endTime > a.ScheduleRealization.StartTime
                    && a.ScheduleRealization.IdVenueChange == idVenue
                    && a.ScheduleRealization.IsCancel == false)
                .Any();

            // check with other venue reserve

            bool checkWithOtherVenueReservation = _context.Entity<TrVenueReservation>()
                .Where(a => a.ScheduleDate.Date == scheduleDate.Date
                    && a.VenueMapping.IdVenue == idVenue
                    && visibleVenueStatuses.Contains(a.Status))
                .AsEnumerable()
                .Where(a => startTime < a.EndTime.Add(TimeSpan.FromMinutes(a.CleanUpTime ?? 0))
                    && endTime > a.StartTime.Subtract(TimeSpan.FromMinutes(a.PreparationTime ?? 0))
                    && (idVenueReservation == null ? true : a.Id != idVenueReservation))
                .Any();

            // check with restrict venue

            var checkWithRestrictVenue = _context.Entity<MsRestrictionBookingVenue>()
                .Where(a => scheduleDate.Add(startTime) <= a.EndRestrictionDate
                    && scheduleDate.Add(endTime) >= a.StartRestrictionDate
                    && ((a.IdBuilding == venueMapping.Floor.IdBuilding && string.IsNullOrEmpty(a.IdVenue))
                    || (a.IdVenue == idVenue && a.IdBuilding == venueMapping.Floor.IdBuilding)))
                .Any();

            // check is venue inactive

            bool checkInactiveVenue = venueMapping.IsVenueActive;

            return checkWithScheduleLesson || checkWithScheduleRealization || checkWithOtherVenueReservation || checkWithRestrictVenue || !checkInactiveVenue;
        }
    }
}
