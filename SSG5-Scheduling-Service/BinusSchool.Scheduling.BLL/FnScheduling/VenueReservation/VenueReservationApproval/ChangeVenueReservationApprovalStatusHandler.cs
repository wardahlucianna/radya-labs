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
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.SendEmail;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.SendEmail;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.Helper;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.SendEmail;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval
{
    public class ChangeVenueReservationApprovalStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly SendEmailBulkVenueReservationApprovalHandler _sendEmail;
        private readonly SendEmailBulkVenueAndEquipmentReservationForPIC _sendEmailForPIC;
        private IDbContextTransaction _transaction;

        public ChangeVenueReservationApprovalStatusHandler(ISchedulingDbContext context, SendEmailBulkVenueReservationApprovalHandler sendEmail, SendEmailBulkVenueAndEquipmentReservationForPIC sendEmailForPIC)
        {
            _context = context;
            _sendEmail = sendEmail;
            _sendEmailForPIC = sendEmailForPIC;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<List<ChangeVenueReservationApprovalStatusRequest>, ChangeVenueReservationApprovalStatusValidator>();

            try
            {
                _transaction = await _context.BeginTransactionAsync();

                var idBooking = new List<string>();
                var canceledEquipmentsForPIC = new List<CanceledEquipmentForPIC>();

                foreach (var item in request)
                {
                    var getVenueReservation = await _context.Entity<TrVenueReservation>()
                    .Include(a => a.VenueMapping.Venue)
                    .Where(a => a.Id == item.IdBooking)
                    .FirstOrDefaultAsync(CancellationToken);

                    var getApproverUser = await _context.Entity<MsVenueMappingApproval>()
                        .Where(a => a.IdBinusian == item.IdUser
                            && a.IdVenueMapping == getVenueReservation.IdVenueMapping)
                        .ToListAsync(CancellationToken);

                    var checkUser = getApproverUser.Count();

                    if (item.IdUser != "00000000-0000-0000-0000-000000000000")
                    {
                        if (checkUser == 0) throw new BadRequestException("This user is not the Approver");
                    }

                    if (getVenueReservation.Status != 3) throw new BadRequestException($"This venue reservation already {VenueReservationApprovalStatusHelper.ApprovalStatus(getVenueReservation.Status)}");

                    var getMappingEquipmentReservation = await _context.Entity<TrMappingEquipmentReservation>()
                        .Include(a => a.EquipmentReservations)
                            .ThenInclude(b => b.Equipment.EquipmentType)
                        .Where(a => a.IdVenueReservation == item.IdBooking)
                        .ToListAsync(CancellationToken);

                    var insertTrVenueReservationHistory = new HTrVenueReservation
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdVenueMapping = getVenueReservation.IdVenueMapping,
                        IdVenueReservation = getVenueReservation.Id,
                        ScheduleDate = getVenueReservation.ScheduleDate,
                        StartTime = getVenueReservation.StartTime,
                        EndTime = getVenueReservation.EndTime,
                        IdUser = getVenueReservation.IdUser,
                        EventDescription = getVenueReservation.EventDescription,
                        IdRepeatGroup = null,
                        URL = getVenueReservation.URL ?? null,
                        FileName = getVenueReservation.FileName ?? null,
                        FileType = getVenueReservation.FileType ?? null,
                        FileSize = getVenueReservation.FileSize,
                        CleanUpTime = getVenueReservation.CleanUpTime ?? 0,
                        PreparationTime = getVenueReservation.PreparationTime ?? 0,
                        IsOverlapping = getVenueReservation.IsOverlapping,
                        Status = getVenueReservation.Status,
                        RejectionReason = getVenueReservation.RejectionReason ?? null,
                        IdUserAction = getVenueReservation.IdUserAction ?? null,
                    };

                    _context.Entity<HTrVenueReservation>().Add(insertTrVenueReservationHistory);

                    getVenueReservation.Status = (int)item.ApprovalStatus;
                    getVenueReservation.IdUserAction = item.IdUser;
                    getVenueReservation.RejectionReason = item.RejectionReason ?? null;

                    _context.Entity<TrVenueReservation>().Update(getVenueReservation);

                    if (item.ApprovalStatus == VenueApprovalStatus.Rejected)
                    {
                        canceledEquipmentsForPIC = getMappingEquipmentReservation
                            .SelectMany(a => a.EquipmentReservations, (a, b) => new CanceledEquipmentForPIC
                            {
                                EquipmentName = b.Equipment.EquipmentName,
                                EquipmentQty = b.EquipmentBorrowingQty
                            })
                            .ToList();

                        foreach (var equipmentMapping in getMappingEquipmentReservation)
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
                    }

                    idBooking.Add(getVenueReservation.Id);
                }

                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();

                await _sendEmail.SendEmailBulkVenueReservationApproval(new SendEmailBulkVenueReservationApprovalRequest
                {
                    IdBooking = idBooking
                });

                if (request.Select(a => a.ApprovalStatus).Any(a => a == VenueApprovalStatus.Approved))
                {
                    await _sendEmailForPIC.SendEmail(new SendEmailBulkVenueAndEquipmentReservationForPICRequest
                    {
                        IdBooking = idBooking,
                        Action = "RESERVED",
                        Recurrence = new List<DateTime>(),
                        NotAvailableEquipments = new List<NotAvailableEquipmentForPIC>(),
                        CanceledEquipments = canceledEquipmentsForPIC
                    });
                }

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();

                throw new Exception(ex.Message.ToString());
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
