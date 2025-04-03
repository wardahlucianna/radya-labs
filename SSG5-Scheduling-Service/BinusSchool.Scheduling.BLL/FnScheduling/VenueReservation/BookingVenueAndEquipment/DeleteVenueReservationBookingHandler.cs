using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Helper;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class DeleteVenueReservationBookingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private IDbContextTransaction _transaction;
        private readonly IMachineDateTime _dateTime;
        private readonly GetVenueReservationUserLoginSpecialRoleHandler _getVenueReservationUserLoginSpecialRoleHandler;
        private readonly SendEmailBulkVenueAndEquipmentReservationForUser _sendEmailForUser;
        private readonly SendEmailBulkVenueAndEquipmentReservationForPIC _sendEmailForPIC;

        public DeleteVenueReservationBookingHandler(ISchedulingDbContext context, IMachineDateTime dateTime, GetVenueReservationUserLoginSpecialRoleHandler getVenueReservationUserLoginSpecialRoleHandler, SendEmailBulkVenueAndEquipmentReservationForUser sendEmailForUser, SendEmailBulkVenueAndEquipmentReservationForPIC sendEmailForPIC)
        {
            _context = context;
            _dateTime = dateTime;
            _getVenueReservationUserLoginSpecialRoleHandler = getVenueReservationUserLoginSpecialRoleHandler;
            _sendEmailForUser = sendEmailForUser;
            _sendEmailForPIC = sendEmailForPIC;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<DeleteVenueReservationBookingRequest, DeleteVenueReservationBookingValidator>();

            try
            {
                _transaction = await _context.BeginTransactionAsync(CancellationToken);

                DateTime today = _dateTime.ServerTime;
                //DateTime today = DateTime.Parse("2024-06-25");
                var idLoggedUser = AuthInfo.UserId;
                //var idLoggedUser = "1701309620";
                var idBooking = new List<string>();
                var canceledEquipmentsForUser = new List<CanceledEquipmentForUser>();
                var canceledEquipmentsForPIC = new List<CanceledEquipmentForPIC>();

                var checkUserSpecialRole = await _getVenueReservationUserLoginSpecialRoleHandler.GetVenueReservationUserLoginSpecialRole(new GetVenueReservationUserLoginSpecialRoleRequest
                {
                    IdUser = idLoggedUser
                });

                var idSchool = await _context.Entity<MsUserSchool>()
                    .Where(a => a.IdUser == idLoggedUser)
                    .FirstOrDefaultAsync(CancellationToken);

                var getUserAction = _context.Entity<MsUser>()
                    .Where(a => a.Id == idLoggedUser)
                    .FirstOrDefault();

                var getVenueReservationRule = await _context.Entity<MsVenueReservationRule>()
                    .Where(a => a.IdSchool == idSchool.IdSchool)
                    .FirstOrDefaultAsync(CancellationToken);

                var getVenueReservation = await _context.Entity<TrVenueReservation>()
                    .Include(a => a.VenueMapping.Venue)
                    .Include(a => a.User)
                    .Where(a => a.VenueMapping.AcademicYear.IdSchool == idSchool.IdSchool)
                    .ToListAsync(CancellationToken);

                var getMappingEquipmentReservation = await _context.Entity<TrMappingEquipmentReservation>()
                    .Include(a => a.EquipmentReservations)
                        .ThenInclude(b => b.Equipment)
                    .ToListAsync(CancellationToken);

                if (checkUserSpecialRole == null || (checkUserSpecialRole != null && checkUserSpecialRole.CanOverrideAnotherReservation == false && checkUserSpecialRole.AllSuperAccess == false))
                {
                    foreach (var items in request.IdVenueReservation)
                    {
                        var filterVenueReservation = getVenueReservation
                            .Where(a => a.Id == items)
                            .FirstOrDefault();

                        var filterMappingEquipmentReservation = getMappingEquipmentReservation
                            .Where(a => a.IdVenueReservation == items)
                            .ToList();

                        if (!BookingVenueAndEquipmentValidationHelper.ValidateDelete(new BookingVenueAndEquipmentValidationParams
                        {
                            Today = today,
                            ScheduleDate = filterVenueReservation.ScheduleDate,
                            StartTime = filterVenueReservation.StartTime,
                            MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                            MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                            AllSuperAccess = false,
                            CanOverride = false,
                            IdLoggedUser = idLoggedUser,
                            CreatedBy = filterVenueReservation.UserIn,
                            CreatedFor = filterVenueReservation.IdUser,
                            ApprovalStatus = filterVenueReservation.Status,
                            IsOverlapping = filterVenueReservation.IsOverlapping
                        }))
                            throw new BadRequestException($"Cannot cancel {filterVenueReservation.EventDescription} book");

                        canceledEquipmentsForUser = filterMappingEquipmentReservation
                            .SelectMany(a => a.EquipmentReservations, (a, b) => new CanceledEquipmentForUser
                            {
                                EquipmentName = b.Equipment.EquipmentName,
                                EquipmentQty = b.EquipmentBorrowingQty
                            })
                            .ToList();

                        canceledEquipmentsForPIC = filterMappingEquipmentReservation
                            .SelectMany(a => a.EquipmentReservations, (a, b) => new CanceledEquipmentForPIC
                            {
                                EquipmentName = b.Equipment.EquipmentName,
                                EquipmentQty = b.EquipmentBorrowingQty
                            })
                            .ToList();

                        var insertTrVenueReservationHistory = new HTrVenueReservation
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdVenueReservation = items,
                            ScheduleDate = filterVenueReservation.ScheduleDate,
                            StartTime = filterVenueReservation.StartTime,
                            EndTime = filterVenueReservation.EndTime,
                            IdVenueMapping = filterVenueReservation.IdVenueMapping,
                            IdUser = filterVenueReservation.IdUser,
                            EventDescription = filterVenueReservation.EventDescription,
                            IdRepeatGroup = null,
                            URL = filterVenueReservation.URL ?? null,
                            FileName = filterVenueReservation.FileName ?? null,
                            FileType = filterVenueReservation.FileType ?? null,
                            FileSize = filterVenueReservation.FileSize,
                            CleanUpTime = filterVenueReservation.CleanUpTime ?? 0,
                            PreparationTime = filterVenueReservation.PreparationTime ?? 0,
                            IsOverlapping = filterVenueReservation.IsOverlapping,
                            Status = filterVenueReservation.Status,
                            RejectionReason = filterVenueReservation.RejectionReason ?? null,
                            IdUserAction = filterVenueReservation.IdUserAction ?? null,
                        };

                        _context.Entity<HTrVenueReservation>().Add(insertTrVenueReservationHistory);

                        filterVenueReservation.Status = 5;
                        filterVenueReservation.IdUserAction = AuthInfo.UserId;

                        _context.Entity<TrVenueReservation>().Update(filterVenueReservation);

                        foreach (var equipmentMapping in filterMappingEquipmentReservation)
                        {
                            var newIdHTrMappingEquipmentReservation = Guid.NewGuid().ToString();

                            var insertTrMappingEquipmentReservationHistory = new HTrMappingEquipmentReservation
                            {
                                IdHTrMappingEquipmentReservation = newIdHTrMappingEquipmentReservation,
                                IdMappingEquipmentReservation = equipmentMapping.Id,
                                ScheduleStartDate = equipmentMapping.ScheduleStartDate,
                                ScheduleEndDate = equipmentMapping.ScheduleEndDate,
                                IdUser = equipmentMapping.IdUser,
                                IdVenue = equipmentMapping.IdVenue,
                                VenueNameinEquipment = equipmentMapping.VenueNameinEquipment,
                                EventDescription = equipmentMapping.EventDescription,
                                IdVenueReservation = equipmentMapping.IdVenueReservation,
                                Notes = equipmentMapping.Notes,
                            };

                            _context.Entity<HTrMappingEquipmentReservation>().Add(insertTrMappingEquipmentReservationHistory);

                            foreach (var equipmentReservation in equipmentMapping.EquipmentReservations)
                            {
                                var insertTrEquipmentReservationHistory = new HTrEquipmentReservation
                                {
                                    IdHTrEquipmentReservation = Guid.NewGuid().ToString(),
                                    IdEquipmentReservation = equipmentReservation.Id,
                                    IdEquipment = equipmentReservation.IdEquipment,
                                    IdMappingEquipmentReservation = equipmentReservation.IdMappingEquipmentReservation,
                                    EquipmentBorrowingQty = equipmentReservation.EquipmentBorrowingQty,
                                    IdHTrMappingEquipmentReservation = newIdHTrMappingEquipmentReservation
                                };

                                _context.Entity<HTrEquipmentReservation>().Add(insertTrEquipmentReservationHistory);

                                equipmentReservation.IsActive = false;

                                _context.Entity<TrEquipmentReservation>().Update(equipmentReservation);
                            }

                            equipmentMapping.IsActive = false;

                            _context.Entity<TrMappingEquipmentReservation>().Update(equipmentMapping);
                        }

                        idBooking.Add(filterVenueReservation.Id);
                    }
                }
                else
                {
                    foreach (var items in request.IdVenueReservation)
                    {
                        var filterVenueReservation = getVenueReservation
                            .Where(a => a.Id == items)
                            .FirstOrDefault();

                        var filterMappingEquipmentReservation = getMappingEquipmentReservation
                            .Where(a => a.IdVenueReservation == items)
                            .ToList();

                        if (!BookingVenueAndEquipmentValidationHelper.ValidateDelete(new BookingVenueAndEquipmentValidationParams
                        {
                            Today = today,
                            ScheduleDate = filterVenueReservation.ScheduleDate,
                            StartTime = filterVenueReservation.StartTime,
                            MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                            MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                            AllSuperAccess = checkUserSpecialRole.AllSuperAccess,
                            CanOverride = checkUserSpecialRole.CanOverrideAnotherReservation,
                            IdLoggedUser = idLoggedUser,
                            CreatedBy = filterVenueReservation.UserIn,
                            CreatedFor = filterVenueReservation.IdUser,
                            ApprovalStatus = filterVenueReservation.Status,
                            IsOverlapping = filterVenueReservation.IsOverlapping
                        }))
                            throw new BadRequestException($"Cannot cancel {filterVenueReservation.EventDescription} book");

                        canceledEquipmentsForUser = filterMappingEquipmentReservation
                            .SelectMany(a => a.EquipmentReservations, (a, b) => new CanceledEquipmentForUser
                            {
                                EquipmentName = b.Equipment.EquipmentName,
                                EquipmentQty = b.EquipmentBorrowingQty
                            })
                            .ToList();

                        canceledEquipmentsForPIC = filterMappingEquipmentReservation
                            .SelectMany(a => a.EquipmentReservations, (a, b) => new CanceledEquipmentForPIC
                            {
                                EquipmentName = b.Equipment.EquipmentName,
                                EquipmentQty = b.EquipmentBorrowingQty
                            })
                            .ToList();

                        var insertTrVenueReservationHistory = new HTrVenueReservation
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdVenueReservation = items,
                            IdVenueMapping = filterVenueReservation.IdVenueMapping,
                            ScheduleDate = filterVenueReservation.ScheduleDate,
                            StartTime = filterVenueReservation.StartTime,
                            EndTime = filterVenueReservation.EndTime,
                            IdUser = filterVenueReservation.IdUser,
                            EventDescription = filterVenueReservation.EventDescription,
                            IdRepeatGroup = null,
                            URL = filterVenueReservation.URL ?? null,
                            FileName = filterVenueReservation.FileName ?? null,
                            FileType = filterVenueReservation.FileType ?? null,
                            FileSize = filterVenueReservation.FileSize,
                            CleanUpTime = filterVenueReservation.CleanUpTime ?? 0,
                            PreparationTime = filterVenueReservation.PreparationTime ?? 0,
                            IsOverlapping = filterVenueReservation.IsOverlapping,
                            Status = filterVenueReservation.Status,
                            RejectionReason = filterVenueReservation.RejectionReason ?? null,
                            IdUserAction = filterVenueReservation.IdUserAction ?? null,
                        };

                        _context.Entity<HTrVenueReservation>().Add(insertTrVenueReservationHistory);

                        filterVenueReservation.Status = 5;
                        filterVenueReservation.IdUserAction = AuthInfo.UserId;

                        _context.Entity<TrVenueReservation>().Update(filterVenueReservation);

                        foreach (var equipmentMapping in filterMappingEquipmentReservation)
                        {
                            var newIdHTrMappingEquipmentReservation = Guid.NewGuid().ToString();

                            var insertTrMappingEquipmentReservationHistory = new HTrMappingEquipmentReservation
                            {
                                IdHTrMappingEquipmentReservation = newIdHTrMappingEquipmentReservation,
                                IdMappingEquipmentReservation = equipmentMapping.Id,
                                ScheduleStartDate = equipmentMapping.ScheduleStartDate,
                                ScheduleEndDate = equipmentMapping.ScheduleEndDate,
                                IdUser = equipmentMapping.IdUser,
                                IdVenue = equipmentMapping.IdVenue,
                                VenueNameinEquipment = equipmentMapping.VenueNameinEquipment,
                                EventDescription = equipmentMapping.EventDescription,
                                IdVenueReservation = equipmentMapping.IdVenueReservation,
                                Notes = equipmentMapping.Notes,
                            };

                            _context.Entity<HTrMappingEquipmentReservation>().Add(insertTrMappingEquipmentReservationHistory);

                            foreach (var equipmentReservation in equipmentMapping.EquipmentReservations)
                            {
                                var insertTrEquipmentReservationHistory = new HTrEquipmentReservation
                                {
                                    IdHTrEquipmentReservation = Guid.NewGuid().ToString(),
                                    IdEquipmentReservation = equipmentReservation.Id,
                                    IdEquipment = equipmentReservation.IdEquipment,
                                    IdMappingEquipmentReservation = equipmentReservation.IdMappingEquipmentReservation,
                                    EquipmentBorrowingQty = equipmentReservation.EquipmentBorrowingQty,
                                    IdHTrMappingEquipmentReservation = newIdHTrMappingEquipmentReservation
                                };

                                _context.Entity<HTrEquipmentReservation>().Add(insertTrEquipmentReservationHistory);

                                equipmentReservation.IsActive = false;

                                _context.Entity<TrEquipmentReservation>().Update(equipmentReservation);
                            }

                            equipmentMapping.IsActive = false;

                            _context.Entity<TrMappingEquipmentReservation>().Update(equipmentMapping);
                        }

                        idBooking.Add(filterVenueReservation.Id);
                    }
                }

                await _sendEmailForUser.SendEmail(new SendEmailBulkVenueAndEquipmentReservationForUserRequest
                {
                    IdBooking = idBooking,
                    Action = "CANCELLED by " + getUserAction.DisplayName.Trim(),
                    ApprovalStatus = null,
                    Recurrence = new List<DateTime>(),
                    NotAvailableEquipments = new List<NotAvailableEquipmentForUser>(),
                    CanceledEquipments = canceledEquipmentsForUser
                });

                await _sendEmailForPIC.SendEmail(new SendEmailBulkVenueAndEquipmentReservationForPICRequest
                {
                    IdBooking = idBooking,
                    Action = "CANCELLED by " + getUserAction.DisplayName.Trim(),
                    Recurrence = new List<DateTime>(),
                    NotAvailableEquipments = new List<NotAvailableEquipmentForPIC>(),
                    CanceledEquipments = canceledEquipmentsForPIC
                });

                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();

                return Request.CreateApiResult2(code: HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
