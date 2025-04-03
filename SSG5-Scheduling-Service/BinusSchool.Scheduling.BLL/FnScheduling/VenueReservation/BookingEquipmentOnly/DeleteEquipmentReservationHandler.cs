using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.VenueReservation;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly.Validator;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NPOI.OpenXml4Net.OPC.Internal;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class DeleteEquipmentReservationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly IMachineDateTime _dateTime;
        private readonly GetVenueReservationUserLoginSpecialRoleHandler _getVenueReservationUserLoginSpecialRoleHandler;
        public DeleteEquipmentReservationHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime,
            GetVenueReservationUserLoginSpecialRoleHandler getVenueReservationUserLoginSpecialRoleHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _getVenueReservationUserLoginSpecialRoleHandler = getVenueReservationUserLoginSpecialRoleHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteEquipmentReservationRequest, DeleteEquipmentReservationValidator>();

            var res = await DeleteEquipmentReservation(param);

            if (!res.isSuccess)
            {
                throw new Exception(res.errorMessage);
            }

            return Request.CreateApiResult2();
        }

        public async Task<(bool isSuccess, string errorMessage)> DeleteEquipmentReservation(DeleteEquipmentReservationRequest param)
        {
            var validator = new DeleteEquipmentReservationValidator();
            var validationResult = validator.Validate(param);

            if (!validationResult.IsValid)
            {
                return (false, "validation failed");
            }

            var today = _dateTime.ServerTime;

            var listIds = param.EquipmentReservationRequestMappings.SelectMany(x => x.IdMappingEquipmentReservation).ToList();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getDeleteData = await _dbContext.Entity<TrMappingEquipmentReservation>()
                    .Include(x => x.EquipmentReservations)
                    .Where(x => listIds.Contains(x.Id))
                    .ToListAsync(CancellationToken);

                if (!getDeleteData.Any())
                {
                    return (false, "Data not found");
                }

                var getVenueRule = await _dbContext.Entity<MsVenueReservationRule>()
                    .Where(x => x.IdSchool == param.IdSchool)
                    .FirstOrDefaultAsync(CancellationToken);

                foreach (var deleteItem in param.EquipmentReservationRequestMappings)
                {
                    var deleteData = getDeleteData.Where(x => deleteItem.IdMappingEquipmentReservation.Contains(x.Id)).ToList();

                    var checkUserSpecialRole = await _getVenueReservationUserLoginSpecialRoleHandler.GetVenueReservationUserLoginSpecialRole(new GetVenueReservationUserLoginSpecialRoleRequest
                    {
                        IdUser = deleteItem.IdUser
                    });

                    var checkUserSpecialRoleCheck = new GetVenueReservationUserLoginSpecialRoleResponse();
                    if (checkUserSpecialRole == null)
                    {
                        checkUserSpecialRoleCheck.AllSuperAccess = false;
                        checkUserSpecialRoleCheck.CanOverrideAnotherReservation = false;
                    }
                    else
                    {
                        checkUserSpecialRoleCheck = checkUserSpecialRole;
                    }

                    foreach (var deleteTransaction in deleteData)
                    {
                        if (!BookingVenueAndEquipmentValidationHelper.ValidateDelete(new BookingVenueAndEquipmentValidationParams
                        {
                            Today = today,
                            ScheduleDate = deleteTransaction.ScheduleStartDate.Date,
                            StartTime = deleteTransaction.ScheduleStartDate.TimeOfDay,
                            MaxDayBooking = getVenueRule.MaxDayBookingVenue,
                            MaxTimeBooking = getVenueRule.MaxTimeBookingVenue,
                            AllSuperAccess = checkUserSpecialRoleCheck.AllSuperAccess,
                            CanOverride = checkUserSpecialRoleCheck.CanOverrideAnotherReservation,
                            IdLoggedUser = deleteItem.IdUser,
                            CreatedBy = deleteTransaction.UserIn,
                            CreatedFor = deleteTransaction.IdUser,
                            ApprovalStatus = 0,
                            IsOverlapping = false,
                            CheckApprovalStatus = false
                        }))
                        {
                            throw new Exception("Cannot Delete Booking Equipment");
                        };

                        var newIdHTrMappingEquipmentReservation = Guid.NewGuid().ToString();

                        var historyMappingData = new HTrMappingEquipmentReservation
                        {
                            IdHTrMappingEquipmentReservation = newIdHTrMappingEquipmentReservation,
                            IdMappingEquipmentReservation = deleteTransaction.Id,
                            ScheduleStartDate = deleteTransaction.ScheduleStartDate,
                            ScheduleEndDate = deleteTransaction.ScheduleEndDate,
                            IdUser = deleteTransaction.IdUser,
                            IdVenue = deleteTransaction.IdVenue,
                            VenueNameinEquipment = deleteTransaction.VenueNameinEquipment,
                            EventDescription = deleteTransaction.EventDescription,
                            IdVenueReservation = deleteTransaction.IdVenueReservation,
                            Notes = deleteTransaction.Notes,
                        };
                        _dbContext.Entity<HTrMappingEquipmentReservation>().Add(historyMappingData);

                        foreach (var deleteEquipment in deleteTransaction.EquipmentReservations)
                        {
                            var historyEquipmentData = new HTrEquipmentReservation
                            {
                                IdHTrEquipmentReservation = Guid.NewGuid().ToString(),
                                IdEquipmentReservation = deleteEquipment.Id,
                                IdEquipment = deleteEquipment.IdEquipment,
                                IdMappingEquipmentReservation = deleteEquipment.IdMappingEquipmentReservation,
                                EquipmentBorrowingQty = deleteEquipment.EquipmentBorrowingQty,
                                IdHTrMappingEquipmentReservation = newIdHTrMappingEquipmentReservation
                            };
                            _dbContext.Entity<HTrEquipmentReservation>().Add(historyEquipmentData);

                            deleteEquipment.IsActive = false;
                            _dbContext.Entity<TrEquipmentReservation>().Update(deleteEquipment);
                        }

                        deleteTransaction.IsActive = false;
                        _dbContext.Entity<TrMappingEquipmentReservation>().Update(deleteTransaction);
                    }

                    await _dbContext.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);
                }
            }
            catch(Exception ex)
            {
                _transaction?.Rollback();
                return (false, ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }

            return (true, null);
        }
    }
}
