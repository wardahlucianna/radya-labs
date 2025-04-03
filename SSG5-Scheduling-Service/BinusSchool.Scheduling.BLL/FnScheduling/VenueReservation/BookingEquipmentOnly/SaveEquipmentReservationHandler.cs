using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly.Helper
{
    public class SaveEquipmentReservationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly GetVenueReservationUserLoginSpecialRoleHandler _getVenueReservationUserLoginSpecialRoleHandler;
        private IDbContextTransaction _transaction;

        public SaveEquipmentReservationHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime,
            GetVenueReservationUserLoginSpecialRoleHandler getVenueReservationUserLoginSpecialRoleHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _getVenueReservationUserLoginSpecialRoleHandler = getVenueReservationUserLoginSpecialRoleHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveEquipmentReservationRequest, SaveEquipmentReservationValidator>();

            var res = await SaveEquipmentReservation(param);

            if (!res.isSuccess)
            {
                throw new Exception("Failed to save equipment reservation");
            }

            return Request.CreateApiResult2();
        }

        public async Task<(bool isSuccess, string errorMessage)> SaveEquipmentReservation(SaveEquipmentReservationRequest param)
        {
            var validator = new SaveEquipmentReservationValidator();
            var validationResult = validator.Validate(param);

            if (!validationResult.IsValid)
                return (false, "validation failed");

            var today = _dateTime.ServerTime;

            var dateToday = _dateTime.ServerTime;

            var DateMin = param.EquipmentReservationMapping.Min(x => x.ScheduleStartDate);
            var DateMax = param.EquipmentReservationMapping.Max(x => x.ScheduleEndDate);

            var listEquipmentNeeded = param.EquipmentReservationMapping.Select(x => x.ListEquipment)
                .SelectMany(x => x).Select(x => x.IdEquipment).ToList();

            var listStatus = Enum.GetValues(typeof(VenueApprovalStatus)).Cast<VenueApprovalStatus>()
                .Where(status => status == VenueApprovalStatus.Approved ||
                    status == VenueApprovalStatus.WaitingForApproval ||
                    status == VenueApprovalStatus.NoNeedApproval)
                .Select(status => (int)status)
                .ToList();

            var getEquipmentTransaction = _dbContext.Entity<TrMappingEquipmentReservation>()
                .Include(x => x.EquipmentReservations).ThenInclude(x => x.Equipment)
                .Include(x => x.VenueReservation)
                .Where(x => x.IdVenueReservation == null ? true : listStatus.Contains(x.VenueReservation.Status))
                .ToList();

            var getEquipment = _dbContext.Entity<MsEquipment>()
                .Where(x => listEquipmentNeeded.Contains(x.Id))
                .ToList();

            var getVenueRule = _dbContext.Entity<MsVenueReservationRule>()
                .Where(x => x.IdSchool == param.IdSchool)
                .FirstOrDefault();

            var checkUserSpecialRole = await _getVenueReservationUserLoginSpecialRoleHandler.GetVenueReservationUserLoginSpecialRole(new GetVenueReservationUserLoginSpecialRoleRequest
            {
                IdUser = param.IdUserLogin
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

            using (_transaction = await _dbContext.BeginTransactionAsync(CancellationToken, IsolationLevel.Serializable))
            {
                try
                {
                    foreach (var dataRequest in param.EquipmentReservationMapping)
                    {
                        if (dataRequest.IdMappingEquipmentReservation == null) // Create New
                        {
                            //Rule ga boleh back date
                            if (BookingVenueAndEquipmentValidationHelper.HasPassedScheduleDateTime(today, dataRequest.ScheduleStartDate.Date, dataRequest.ScheduleStartDate.TimeOfDay))
                            {
                                throw new Exception("End Time must be greater than Start Time and cannot be the same as Start Time");
                            }

                            //Check Rules Cutoff
                            if (checkUserSpecialRoleCheck.AllSuperAccess == false)
                            {
                                if (BookingVenueAndEquipmentValidationHelper.HasPassedRulesCutoff(today, dataRequest.ScheduleStartDate.Date, getVenueRule.MaxDayBookingVenue, getVenueRule.MaxTimeBookingVenue))
                                {
                                    throw new Exception("Booking schedule has passed cutoff rule");
                                }
                            }

                            //Create Mapping Equipment Reservation
                            var mappingEquipmentReservation = new TrMappingEquipmentReservation
                            {
                                Id = Guid.NewGuid().ToString(),
                                ScheduleStartDate = dataRequest.ScheduleStartDate,
                                ScheduleEndDate = dataRequest.ScheduleEndDate,
                                IdUser = dataRequest.IdUser,
                                IdVenue = dataRequest.IdVenue,
                                EventDescription = dataRequest.EventDescription,
                                IdVenueReservation = dataRequest.IdVenueReservation,
                                VenueNameinEquipment = dataRequest.VenueNameinEquipment,
                                Notes = dataRequest.Notes
                            };

                            var equipmentTransactionOnThatDate = getEquipmentTransaction
                                .Where(x =>
                                    (x.ScheduleStartDate.Subtract(TimeSpan.FromMinutes(x.VenueReservation?.PreparationTime ?? 0)) < dataRequest.ScheduleEndDate &&
                                        x.ScheduleEndDate.Add(TimeSpan.FromMinutes(x.VenueReservation?.CleanUpTime ?? 0)) > dataRequest.ScheduleStartDate)
                                )
                                .ToList();

                            foreach (var dataInsertEquipment in dataRequest.ListEquipment)
                            {
                                //Check current stock & Max Borrowing
                                var checkStock = await HasEnoughStock(getEquipment, equipmentTransactionOnThatDate, dataInsertEquipment.IdEquipment, dataInsertEquipment.EquipmentBorrowingQty);

                                if (!checkStock.status)
                                {
                                    throw new Exception(checkStock.errorMessage);
                                }

                                //Create Equipment Reservation
                                var equipmentReservation = new TrEquipmentReservation
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdMappingEquipmentReservation = mappingEquipmentReservation.Id,
                                    IdEquipment = dataInsertEquipment.IdEquipment,
                                    EquipmentBorrowingQty = dataInsertEquipment.EquipmentBorrowingQty
                                };

                                _dbContext.Entity<TrEquipmentReservation>().Add(equipmentReservation);
                            }

                            _dbContext.Entity<TrMappingEquipmentReservation>().Add(mappingEquipmentReservation);
                        }
                        else // Update
                        {
                            var editData = getEquipmentTransaction
                                .Where(x => x.Id == dataRequest.IdMappingEquipmentReservation)
                                .FirstOrDefault();

                            if (editData == null)
                            {
                                throw new Exception("Data not found");
                            }

                            if (!BookingVenueAndEquipmentValidationHelper.ValidateEdit(new BookingVenueAndEquipmentValidationParams
                            {
                                Today = today,
                                ScheduleDate = dataRequest.ScheduleStartDate.Date,
                                StartTime = dataRequest.ScheduleStartDate.TimeOfDay,
                                MaxDayBooking = getVenueRule.MaxDayBookingVenue,
                                MaxTimeBooking = getVenueRule.MaxTimeBookingVenue,
                                AllSuperAccess = checkUserSpecialRoleCheck.AllSuperAccess,
                                CanOverride = checkUserSpecialRoleCheck.CanOverrideAnotherReservation,
                                IdLoggedUser = dataRequest.IdUser,
                                CreatedBy = editData.UserIn,
                                CreatedFor = editData.IdUser,
                                ApprovalStatus = 0, // No need approval for booking equipment
                                IsOverlapping = false,
                                CheckApprovalStatus = false
                            }))
                            {
                                throw new Exception("Cannot Edit Booking Equipment");
                            }

                            var newIdHTrMappingEquipmentReservation = Guid.NewGuid().ToString();

                            //Create History
                            var mappingHistory = new HTrMappingEquipmentReservation
                            {
                                IdHTrMappingEquipmentReservation = newIdHTrMappingEquipmentReservation,
                                IdMappingEquipmentReservation = editData.Id,
                                ScheduleStartDate = editData.ScheduleStartDate,
                                ScheduleEndDate = editData.ScheduleEndDate,
                                IdUser = editData.IdUser,
                                IdVenue = editData.IdVenue,
                                VenueNameinEquipment = editData.VenueNameinEquipment,
                                EventDescription = editData.EventDescription,
                                IdVenueReservation = editData.IdVenueReservation,
                                Notes = editData.Notes,
                            };
                            _dbContext.Entity<HTrMappingEquipmentReservation>().Add(mappingHistory);

                            //Edit Data
                            editData.EventDescription = dataRequest.EventDescription;
                            editData.IdVenue = dataRequest.IdVenue;
                            editData.ScheduleStartDate = dataRequest.ScheduleStartDate;
                            editData.ScheduleEndDate = dataRequest.ScheduleEndDate;
                            editData.EventDescription = dataRequest.EventDescription;
                            editData.IdVenueReservation = dataRequest.IdVenueReservation;
                            editData.VenueNameinEquipment = dataRequest.VenueNameinEquipment;
                            editData.Notes = dataRequest.Notes;

                            _dbContext.Entity<TrMappingEquipmentReservation>().Update(editData);

                            //Update Equipment
                            var EditEquipmentData = editData.EquipmentReservations.ToList();

                            var equipmentUpdate = EditEquipmentData
                                .Where(x => dataRequest.ListEquipment.Select(y => y.IdEquipment).Contains(x.IdEquipment))
                                .ToList();

                            var equipmentDelete = EditEquipmentData
                                .Where(x => !dataRequest.ListEquipment.Select(y => y.IdEquipment).Contains(x.IdEquipment))
                                .ToList();

                            var equipmentInsert = dataRequest.ListEquipment
                                .Where(x => !EditEquipmentData.Any(y => y.IdEquipment == x.IdEquipment))
                                .ToList();

                            var equipmentTransactionOnThatDate = getEquipmentTransaction
                                .Where(x =>
                                    (x.ScheduleStartDate.Subtract(TimeSpan.FromMinutes(x.VenueReservation?.PreparationTime ?? 0)) < dataRequest.ScheduleEndDate &&
                                        x.ScheduleEndDate.Add(TimeSpan.FromMinutes(x.VenueReservation?.CleanUpTime ?? 0)) > dataRequest.ScheduleStartDate)
                                )
                                .Where(x => x.Id != editData.Id)
                                .ToList();

                            //Update Equipment that has different EquipmentBorrowingQty
                            foreach (var updateDataEquipment in equipmentUpdate)
                            {
                                var dataUpdate = dataRequest.ListEquipment
                                    .Where(x => x.IdEquipment == updateDataEquipment.IdEquipment)
                                    .FirstOrDefault();

                                //Check current stock & Max Borrowing
                                var checkStock = await HasEnoughStock(getEquipment, equipmentTransactionOnThatDate, dataUpdate.IdEquipment, dataUpdate.EquipmentBorrowingQty);

                                if (!checkStock.status)
                                {
                                    throw new Exception(checkStock.errorMessage);
                                }

                                if (updateDataEquipment.EquipmentBorrowingQty == dataUpdate.EquipmentBorrowingQty)
                                {
                                    continue;
                                }

                                var equipmentHistory = new HTrEquipmentReservation
                                {
                                    IdHTrEquipmentReservation = Guid.NewGuid().ToString(),
                                    IdEquipmentReservation = updateDataEquipment.Id,
                                    IdMappingEquipmentReservation = updateDataEquipment.IdMappingEquipmentReservation,
                                    IdEquipment = updateDataEquipment.IdEquipment,
                                    EquipmentBorrowingQty = updateDataEquipment.EquipmentBorrowingQty,
                                    IdHTrMappingEquipmentReservation = newIdHTrMappingEquipmentReservation
                                };

                                _dbContext.Entity<HTrEquipmentReservation>().Add(equipmentHistory);

                                updateDataEquipment.EquipmentBorrowingQty = dataUpdate.EquipmentBorrowingQty;
                                _dbContext.Entity<TrEquipmentReservation>().Update(updateDataEquipment);
                            }

                            //Delete Equipment
                            foreach (var deleteDataEquipment in equipmentDelete)
                            {
                                var equipmentHistory = new HTrEquipmentReservation
                                {
                                    IdHTrEquipmentReservation = Guid.NewGuid().ToString(),
                                    IdEquipmentReservation = deleteDataEquipment.Id,
                                    IdMappingEquipmentReservation = deleteDataEquipment.IdMappingEquipmentReservation,
                                    IdEquipment = deleteDataEquipment.IdEquipment,
                                    EquipmentBorrowingQty = deleteDataEquipment.EquipmentBorrowingQty,
                                    IdHTrMappingEquipmentReservation = newIdHTrMappingEquipmentReservation
                                };
                                _dbContext.Entity<HTrEquipmentReservation>().Add(equipmentHistory);

                                deleteDataEquipment.IsActive = false;
                                _dbContext.Entity<TrEquipmentReservation>().Update(deleteDataEquipment);
                            }

                            //Insert Equipment
                            foreach (var insertDataEquipment in equipmentInsert)
                            {
                                //Check current stock & Max Borrowing
                                var checkStock = await HasEnoughStock(getEquipment, equipmentTransactionOnThatDate, insertDataEquipment.IdEquipment, insertDataEquipment.EquipmentBorrowingQty);

                                if (!checkStock.status)
                                {
                                    throw new Exception(checkStock.errorMessage);
                                }

                                var equipmentReservation = new TrEquipmentReservation
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdMappingEquipmentReservation = editData.Id,
                                    IdEquipment = insertDataEquipment.IdEquipment,
                                    EquipmentBorrowingQty = insertDataEquipment.EquipmentBorrowingQty
                                };

                                _dbContext.Entity<TrEquipmentReservation>().Add(equipmentReservation);
                            }
                        }
                    }

                    await _dbContext.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);
                }
                catch (DbUpdateException ex)
                {
                    _transaction?.Rollback();
                    return (false, "An error occured. Please try again.");
                }
                catch (Exception ex)
                {
                    _transaction?.Rollback();
                    return (false, ex.Message);
                }
            }

            return (true, null);
        }

        private async Task<(bool status, string errorMessage)> HasEnoughStock(List<MsEquipment> equipments, List<TrMappingEquipmentReservation> mappingEquipmentReservations, string idEquipment, int equipmentBorrowingQty)
        {
            var availableStock = equipments.Where(x => x.Id == idEquipment).FirstOrDefault();
            var amountStockUsed = mappingEquipmentReservations.Select(x => x.EquipmentReservations).SelectMany(x => x)
                .Where(x => x.IdEquipment == idEquipment)
                .Select(x => x.EquipmentBorrowingQty).Sum();

            if (availableStock.MaxQtyBorrowing != null && (availableStock.MaxQtyBorrowing < equipmentBorrowingQty))
            {
                return (false, "Amount borrowing exceeding the maximum quantity borrowing");
            }

            if ((availableStock.TotalStockQty - amountStockUsed) < equipmentBorrowingQty)
            {
                return (false, "Amount borrowing exceeding the available stock");
            }

            return (true, null);
        }
    }
}
