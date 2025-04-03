using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.SendEmail;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly.Helper;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Helper;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Validator;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.SendEmail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class SaveVenueReservationBookingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly SaveEquipmentReservationHandler _saveEquipment;
        private readonly SendEmailBulkVenueAndEquipmentReservationForUser _sendEmailForUser;
        private readonly SendEmailBulkVenueAndEquipmentReservationForPIC _sendEmailForPIC;
        private readonly SendEmailBulkVenueReservationApprovalForApprover _sendEmailForApprover;
        private IDbContextTransaction _transaction;

        public SaveVenueReservationBookingHandler(ISchedulingDbContext context, SaveEquipmentReservationHandler saveEquipment, SendEmailBulkVenueAndEquipmentReservationForUser sendEmailForUser, SendEmailBulkVenueAndEquipmentReservationForPIC sendEmailForPIC, SendEmailBulkVenueReservationApprovalForApprover sendEmailForApprover)
        {
            _context = context;
            _saveEquipment = saveEquipment;
            _sendEmailForUser = sendEmailForUser;
            _sendEmailForPIC = sendEmailForPIC;
            _sendEmailForApprover = sendEmailForApprover;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<List<SaveVenueReservationBookingRequest>, SaveVenueReservationBookingValidator>();
            var response = new SaveVenueReservationBookingResponse();
            int flag = 0;

            var idLoggedUser = AuthInfo.UserId;
            var idRepeatGroup = "";
            var sendUserAction = "";
            var sendPICAction = "";
            var needApproval = new NeedApproval();
            var idBooking = new List<string>();
            var recurrence = new List<DateTime>();
            var topBookId = "";
            var notAvailEquipmentForUser = new List<NotAvailableEquipmentForUser>();
            var notAvailEquipmentForPIC = new List<NotAvailableEquipmentForPIC>();

            List<int> visibleVenueStatuses = new List<VenueApprovalStatus>
                    {
                        VenueApprovalStatus.Approved,
                        VenueApprovalStatus.WaitingForApproval,
                        VenueApprovalStatus.NoNeedApproval,
                    }.Cast<int>().ToList();

            if (request.Count > 1)
                idRepeatGroup = Guid.NewGuid().ToString();

            // create
            if (request.Any(a => a.IdVenueReservation == null))
            {
                foreach (var item in request)
                {
                    var saveEquipment = new SaveEquipmentReservationRequest();

                    // insert venue reserve
                    using (_transaction = await _context.BeginTransactionAsync(CancellationToken, IsolationLevel.Serializable))
                    {
                        try
                        {
                            TimeSpan StartTime = item.StartTime.Subtract(TimeSpan.FromMinutes(item.PreparationTime ?? 0));
                            TimeSpan EndTime = item.EndTime.Add(TimeSpan.FromMinutes(item.CleanUpTime ?? 0));

                            if (item.EndTime <= item.StartTime)
                                throw new BadRequestException("End Time must be greater than Start Time and cannot be the same as Start Time");

                            if (item.StartTime - TimeSpan.FromMinutes(item.PreparationTime ?? 0) < TimeSpan.Zero)
                                throw new BadRequestException("The start time with preparation time cannot be pushed back to the previous day.");

                            if (item.EndTime + TimeSpan.FromMinutes(item.CleanUpTime ?? 0) > new TimeSpan(23, 59, 59))
                                throw new BadRequestException("The end time with clean up time cannot be advanced to the next day.");

                            var getVenueReserve = _context.Entity<TrVenueReservation>()
                                .Where(a => a.ScheduleDate.Date == item.ScheduleDate.Date
                                    && a.VenueMapping.IdVenue == item.IdVenue
                                    && visibleVenueStatuses.Contains(a.Status))
                                .AsEnumerable()
                                .Where(a => StartTime < a.EndTime.Add(TimeSpan.FromMinutes(a.CleanUpTime ?? 0))
                                    && EndTime > a.StartTime.Subtract(TimeSpan.FromMinutes(a.PreparationTime ?? 0)))
                                .Count();

                            if (getVenueReserve != 0)
                                throw new BadRequestException("Cannot create. Booking time has been overlap.");

                            var idSchool = await _context.Entity<MsUserSchool>()
                                .Where(a => a.IdUser == item.Requester)
                                .FirstOrDefaultAsync(CancellationToken);

                            #region Get Academic Year
                            // check period
                            var checkActivePeriod = await _context.Entity<MsPeriod>()
                               .Include(x => x.Grade)
                                   .ThenInclude(x => x.Level)
                                       .ThenInclude(x => x.AcademicYear)
                               .Where(x => x.Grade.Level.AcademicYear.IdSchool == idSchool.IdSchool)
                               .Where(x => item.ScheduleDate.Date >= x.StartDate.Date)
                               .Where(x => item.ScheduleDate.Date <= x.EndDate.Date)
                               .Select(x => x.Grade.Level.AcademicYear.Id).FirstOrDefaultAsync();

                            var getLatestAcademicYear = await _context.Entity<MsPeriod>()
                               .Include(x => x.Grade)
                                   .ThenInclude(x => x.Level)
                                       .ThenInclude(x => x.AcademicYear)
                               .Where(x => x.Grade.Level.AcademicYear.IdSchool == idSchool.IdSchool)
                               .Where(x => item.ScheduleDate.Date < x.EndDate.Date)
                               .Select(x => x.Grade.Level.AcademicYear.Id).FirstOrDefaultAsync();
                            #endregion

                            var venueReservationId = Guid.NewGuid().ToString();

                            var getIdVenueMapping = await _context.Entity<MsVenueMapping>()
                                .Include(a => a.Floor)
                                .Include(a => a.Venue)
                                .Where(a => a.IdVenue == item.IdVenue
                                    && (checkActivePeriod.Count() != 0 ? a.IdAcademicYear == checkActivePeriod : a.IdAcademicYear == getLatestAcademicYear))
                                .FirstOrDefaultAsync(CancellationToken);

                            #region check with restrict and inactive venue
                            // check with restrict venue

                            bool checkWithRestrictVenue = _context.Entity<MsRestrictionBookingVenue>()
                                .Where(a => item.ScheduleDate.Add(StartTime) <= a.EndRestrictionDate
                                    && item.ScheduleDate.Add(EndTime) >= a.StartRestrictionDate
                                    && ((a.IdBuilding == getIdVenueMapping.Floor.IdBuilding && string.IsNullOrEmpty(a.IdVenue))
                                    || (a.IdVenue == item.IdVenue && a.IdBuilding == getIdVenueMapping.Floor.IdBuilding)))
                                .Any();

                            // check is venue inactive

                            bool checkInactiveVenue = getIdVenueMapping.IsVenueActive;

                            if (checkWithRestrictVenue || !checkInactiveVenue)
                                throw new BadRequestException("Cannot create venue booking in Restrict/Inactive venue");
                            #endregion

                            var insertVenueReservation = new TrVenueReservation()
                            {
                                Id = venueReservationId,
                                IdVenueMapping = getIdVenueMapping.Id,
                                ScheduleDate = item.ScheduleDate,
                                StartTime = item.StartTime,
                                EndTime = item.EndTime,
                                IdUser = item.Requester,
                                EventDescription = item.EventDescription,
                                IdRepeatGroup = idRepeatGroup,
                                URL = item.FileUpload?.Url ?? null,
                                FileName = item.FileUpload?.FileName ?? null,
                                FileType = ("." + item.FileUpload?.FileType) ?? null,
                                FileSize = item.FileUpload?.FileSize ?? 0,
                                Notes = item.Note,
                                PreparationTime = item.PreparationTime,
                                CleanUpTime = item.CleanUpTime,
                                IsOverlapping = false,
                                Status = getIdVenueMapping.IsNeedApproval == true ? 3 : 4,
                            };

                            needApproval = new NeedApproval
                            {
                                IsNeedApproval = getIdVenueMapping.IsNeedApproval
                            };

                            _context.Entity<TrVenueReservation>().Add(insertVenueReservation);

                            idBooking.Add(venueReservationId);
                            var insertEquipmentData = new List<SaveEquipmentReservationRequest_Equipment>();

                            #region Not Avail Equipment
                            var getEquipmentReservation = await _context.Entity<TrEquipmentReservation>()
                                    .Include(a => a.MappingEquipmentReservation)
                                    .Where(a => item.ScheduleDate.Add(StartTime) < a.MappingEquipmentReservation.ScheduleEndDate
                                        && item.ScheduleDate.Add(EndTime) > a.MappingEquipmentReservation.ScheduleStartDate)
                                    .ToListAsync(CancellationToken);

                            foreach (var equipment in item.AdditionalEquipments)
                            {
                                var getEquipment = await _context.Entity<MsEquipment>()
                                    .Where(a => a.Id == equipment.IdEquipment)
                                    .FirstOrDefaultAsync(CancellationToken);

                                var totalEquipmentReserve = getEquipmentReservation
                                    .Where(a => a.IdEquipment == equipment.IdEquipment)
                                    .Select(a => a.EquipmentBorrowingQty)
                                    .Sum();

                                if ((getEquipment.TotalStockQty - totalEquipmentReserve) < equipment.EquipmentBorrowingQty)
                                {
                                    var insertEquipmentForUser = new NotAvailableEquipmentForUser
                                    {
                                        ScheduleDate = item.ScheduleDate.ToString("dd MMMM yyyy"),
                                        EquipmentName = getEquipment.EquipmentName,
                                        BorrowingQty = equipment.EquipmentBorrowingQty.ToString(),
                                        StockQty = (getEquipment.TotalStockQty - totalEquipmentReserve).ToString()
                                    };

                                    notAvailEquipmentForUser.Add(insertEquipmentForUser);

                                    var insertEquipmentForPIC = new NotAvailableEquipmentForPIC
                                    {
                                        ScheduleDate = item.ScheduleDate.ToString("dd MMMM yyyy"),
                                        EquipmentName = getEquipment.EquipmentName,
                                        BorrowingQty = equipment.EquipmentBorrowingQty.ToString(),
                                        StockQty = (getEquipment.TotalStockQty - totalEquipmentReserve).ToString()
                                    };

                                    notAvailEquipmentForPIC.Add(insertEquipmentForPIC);

                                    flag++;
                                }

                                if ((getEquipment.TotalStockQty - totalEquipmentReserve) != 0)
                                {
                                    var equipmentData = new SaveEquipmentReservationRequest_Equipment
                                    {
                                        IdEquipment = equipment.IdEquipment,
                                        EquipmentBorrowingQty = (getEquipment.TotalStockQty - totalEquipmentReserve) < equipment.EquipmentBorrowingQty
                                                          ? (getEquipment.TotalStockQty - totalEquipmentReserve)
                                                          : equipment.EquipmentBorrowingQty,
                                    };

                                    insertEquipmentData.Add(equipmentData);
                                }
                            }
                            #endregion

                            saveEquipment = new SaveEquipmentReservationRequest
                            {
                                IdSchool = idSchool.IdSchool,
                                IdUserLogin = idLoggedUser,
                                EquipmentReservationMapping = new List<SaveEquipmentReservationRequest_Mapping>
                                {
                                    new SaveEquipmentReservationRequest_Mapping
                                    {
                                        IdMappingEquipmentReservation = null,
                                        ScheduleStartDate = item.ScheduleDate.Add(StartTime),
                                        ScheduleEndDate = item.ScheduleDate.Add(EndTime),
                                        IdUser = item.Requester,
                                        IdVenue = item.IdVenue,
                                        EventDescription = item.EventDescription,
                                        IdVenueReservation = venueReservationId,
                                        Notes = item.Note,
                                        ListEquipment = insertEquipmentData
                                    }
                                }
                            };

                            await _context.SaveChangesAsync(CancellationToken);
                            await _transaction.CommitAsync(CancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _transaction?.Rollback();
                            throw new Exception(ex.Message.ToString());
                        }
                    }

                    // insert equipment reserve
                    if (item.AdditionalEquipments.Any())
                    {
                        var insertEquipment = await _saveEquipment.SaveEquipmentReservation(saveEquipment);
                    }
                }

                sendUserAction = "CREATED";
                sendPICAction = "RESERVED";

                if (request.Count > 1)
                    recurrence = request
                        .Select(a => a.ScheduleDate)
                        .OrderBy(a => a)
                        .ToList();

                topBookId = idBooking.FirstOrDefault();
            }
            // edit
            else
            {
                foreach (var item in request)
                {
                    var saveEquipment = new SaveEquipmentReservationRequest();
                    // update venue reserve
                    using (_transaction = await _context.BeginTransactionAsync(CancellationToken, IsolationLevel.Serializable))
                    {
                        try
                        {
                            TimeSpan StartTime = item.StartTime.Subtract(TimeSpan.FromMinutes(item.PreparationTime ?? 0));
                            TimeSpan EndTime = item.EndTime.Add(TimeSpan.FromMinutes(item.CleanUpTime ?? 0));

                            if (item.EndTime <= item.StartTime)
                                throw new BadRequestException("End Time must be greater than Start Time and cannot be the same as Start Time");

                            if (item.StartTime - TimeSpan.FromMinutes(item.PreparationTime ?? 0) < TimeSpan.Zero)
                                throw new BadRequestException("The start time with preparation time cannot be pushed back to the previous day.");

                            if (item.EndTime + TimeSpan.FromMinutes(item.CleanUpTime ?? 0) > new TimeSpan(23, 59, 59))
                                throw new BadRequestException("The end time with clean up time cannot be advanced to the next day.");

                            var getVenueReserve = await _context.Entity<TrVenueReservation>()
                                .Include(a => a.VenueMapping.AcademicYear)
                                .Include(a => a.VenueMapping.Venue)
                                .Where(a => a.Id == item.IdVenueReservation)
                                .FirstOrDefaultAsync(CancellationToken);

                            var checkVenueReserve = _context.Entity<TrVenueReservation>()
                                .Where(a => a.ScheduleDate.Date == item.ScheduleDate.Date
                                    && a.VenueMapping.IdVenue == item.IdVenue
                                    && visibleVenueStatuses.Contains(a.Status))
                                .AsEnumerable()
                                .Where(a => StartTime < a.EndTime.Add(TimeSpan.FromMinutes(a.CleanUpTime ?? 0))
                                    && EndTime > a.StartTime.Subtract(TimeSpan.FromMinutes(a.PreparationTime ?? 0))
                                    && a.Id != item.IdVenueReservation)
                                .Count();

                            var getMappingEquipmentReservation = await _context.Entity<TrMappingEquipmentReservation>()
                                .Where(a => a.IdVenueReservation == item.IdVenueReservation)
                                .FirstOrDefaultAsync(CancellationToken);

                            if (checkVenueReserve != 0)
                                throw new BadRequestException("Cannot edit. Booking time has been overlap.");

                            var insertHTrVenueReservation = new HTrVenueReservation()
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdVenueReservation = getVenueReserve.Id,
                                IdVenueMapping = getVenueReserve.IdVenueMapping,
                                ScheduleDate = getVenueReserve.ScheduleDate,
                                StartTime = getVenueReserve.StartTime,
                                EndTime = getVenueReserve.EndTime,
                                IdUser = getVenueReserve.IdUser,
                                EventDescription = getVenueReserve.EventDescription,
                                IdRepeatGroup = getVenueReserve.IdRepeatGroup,
                                URL = getVenueReserve.URL ?? null,
                                FileName = getVenueReserve.FileName ?? null,
                                FileType = ("." + getVenueReserve.FileType) ?? null,
                                FileSize = getVenueReserve.FileSize,
                                Notes = getVenueReserve.Notes,
                                PreparationTime = getVenueReserve.PreparationTime,
                                CleanUpTime = getVenueReserve.CleanUpTime,
                                IsOverlapping = getVenueReserve.IsOverlapping,
                                Status = getVenueReserve.Status,
                                RejectionReason = getVenueReserve.RejectionReason,
                                IdUserAction = getVenueReserve.IdUserAction
                            };

                            _context.Entity<HTrVenueReservation>().Add(insertHTrVenueReservation);

                            getVenueReserve.EventDescription = item.EventDescription;
                            getVenueReserve.StartTime = item.StartTime;
                            getVenueReserve.EndTime = item.EndTime;
                            getVenueReserve.PreparationTime = item.PreparationTime;
                            getVenueReserve.CleanUpTime = item.CleanUpTime;
                            getVenueReserve.URL = item.FileUpload?.Url ?? null;
                            getVenueReserve.FileName = item.FileUpload?.FileName ?? null;
                            getVenueReserve.FileType = item.FileUpload?.FileType ?? null;
                            getVenueReserve.FileSize = item.FileUpload?.FileSize ?? 0;
                            getVenueReserve.Notes = item.Note;

                            _context.Entity<TrVenueReservation>().Update(getVenueReserve);

                            idBooking.Add(item.IdVenueReservation);
                            needApproval.IsNeedApproval = false;
                            var insertEquipmentData = new List<SaveEquipmentReservationRequest_Equipment>();

                            #region Not Avail Equipment
                            var getEquipmentReservation = await _context.Entity<TrEquipmentReservation>()
                                    .Include(a => a.MappingEquipmentReservation)
                                    .Where(a => getVenueReserve.ScheduleDate.Add(StartTime) < a.MappingEquipmentReservation.ScheduleEndDate
                                        && getVenueReserve.ScheduleDate.Add(EndTime) > a.MappingEquipmentReservation.ScheduleStartDate)
                                    .ToListAsync(CancellationToken);

                            foreach (var equipment in item.AdditionalEquipments)
                            {
                                var getEquipment = await _context.Entity<MsEquipment>()
                                    .Where(a => a.Id == equipment.IdEquipment)
                                    .FirstOrDefaultAsync(CancellationToken);

                                var totalEquipmentReserve = getEquipmentReservation
                                    .Where(a => a.IdEquipment == equipment.IdEquipment
                                        && a.MappingEquipmentReservation.IdVenueReservation != getVenueReserve.Id)
                                    .Select(a => a.EquipmentBorrowingQty)
                                    .Sum();

                                if ((getEquipment.TotalStockQty - totalEquipmentReserve) < equipment.EquipmentBorrowingQty)
                                {
                                    var insertEquipmentForUser = new NotAvailableEquipmentForUser
                                    {
                                        ScheduleDate = item.ScheduleDate.ToString("dd MMMM yyyy"),
                                        EquipmentName = getEquipment.EquipmentName,
                                        BorrowingQty = equipment.EquipmentBorrowingQty.ToString(),
                                        StockQty = (getEquipment.TotalStockQty - totalEquipmentReserve).ToString()
                                    };

                                    notAvailEquipmentForUser.Add(insertEquipmentForUser);

                                    var insertEquipmentForPIC = new NotAvailableEquipmentForPIC
                                    {
                                        ScheduleDate = item.ScheduleDate.ToString("dd MMMM yyyy"),
                                        EquipmentName = getEquipment.EquipmentName,
                                        BorrowingQty = equipment.EquipmentBorrowingQty.ToString(),
                                        StockQty = (getEquipment.TotalStockQty - totalEquipmentReserve).ToString()
                                    };

                                    notAvailEquipmentForPIC.Add(insertEquipmentForPIC);

                                    flag++;
                                }

                                if ((getEquipment.TotalStockQty - totalEquipmentReserve) != 0)
                                {
                                    var equipmentData = new SaveEquipmentReservationRequest_Equipment
                                    {
                                        IdEquipment = equipment.IdEquipment,
                                        EquipmentBorrowingQty = (getEquipment.TotalStockQty - totalEquipmentReserve) < equipment.EquipmentBorrowingQty
                                                          ? (getEquipment.TotalStockQty - totalEquipmentReserve)
                                                          : equipment.EquipmentBorrowingQty,
                                    };

                                    insertEquipmentData.Add(equipmentData);
                                }
                            }
                            #endregion
                            saveEquipment = new SaveEquipmentReservationRequest
                            {
                                IdSchool = getVenueReserve.VenueMapping.AcademicYear.IdSchool,
                                IdUserLogin = idLoggedUser,
                                EquipmentReservationMapping = new List<SaveEquipmentReservationRequest_Mapping>
                                {
                                    new SaveEquipmentReservationRequest_Mapping
                                    {
                                        IdMappingEquipmentReservation = getMappingEquipmentReservation?.Id ?? null,
                                        ScheduleStartDate = item.ScheduleDate.Add(StartTime),
                                        ScheduleEndDate = item.ScheduleDate.Add(EndTime),
                                        IdUser = item.Requester,
                                        IdVenue = item.IdVenue,
                                        EventDescription = item.EventDescription,
                                        IdVenueReservation = getVenueReserve.Id,
                                        Notes = item.Note,
                                        ListEquipment = insertEquipmentData
                                    }
                                }
                            };

                            await _context.SaveChangesAsync(CancellationToken);
                            await _transaction.CommitAsync(CancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _transaction?.Rollback();
                            throw new Exception(ex.Message.ToString());
                        }
                    }

                    // update equipment reserve
                    var updateEquipment = await _saveEquipment.SaveEquipmentReservation(saveEquipment);
                }

                sendUserAction = "UPDATED";
                sendPICAction = "UPDATED";
                topBookId = idBooking.FirstOrDefault();
            }

            #region Send Email
            await _sendEmailForUser.SendEmail(new SendEmailBulkVenueAndEquipmentReservationForUserRequest
            {
                IdBooking = new List<string> { topBookId },
                Action = sendUserAction,
                Recurrence = recurrence,
                ApprovalStatus = needApproval.IsNeedApproval == true ? VenueApprovalStatus.WaitingForApproval : VenueApprovalStatus.NoNeedApproval,
                NotAvailableEquipments = notAvailEquipmentForUser,
                CanceledEquipments = new List<CanceledEquipmentForUser>()
            });

            if (needApproval.IsNeedApproval == true)
            {
                await _sendEmailForApprover.SendEmail(new SendEmailBulkVenueReservationApprovalForApproverRequest
                {
                    IdBooking = new List<string> { topBookId },
                    Recurrence = recurrence,
                });
            }
            else
            {
                await _sendEmailForPIC.SendEmail(new SendEmailBulkVenueAndEquipmentReservationForPICRequest
                {
                    IdBooking = new List<string> { topBookId },
                    Action = sendPICAction,
                    Recurrence = recurrence,
                    NotAvailableEquipments = notAvailEquipmentForPIC,
                    CanceledEquipments = new List<CanceledEquipmentForPIC>()
                });
            }
            #endregion

            if (flag > 0)
                response.IsShowEquipmentExpectationMessage = true;
            else
                response.IsShowEquipmentExpectationMessage = false;

            return Request.CreateApiResult2(response as object, code: HttpStatusCode.Created);
        }

        private class NeedApproval
        {
            public bool IsNeedApproval { get; set; }
        }

        private class NotAvailableEquipment
        {
            public string ScheduleDate { get; set; }
            public string EquipmentName { get; set; }
            public string BorrowingQty { get; set; }
            public string StockQty { get; set; }
        }
    }
}
