using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class DeleteBookingEquipmentOnlyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly DeleteEquipmentReservationHandler _deleteEquipmentReservationHandler;
        private readonly SendEmailForBookingEquipmentOnlyHandler _sendEmailForBookingEquipmentOnlyHandler;
        public DeleteBookingEquipmentOnlyHandler(ISchedulingDbContext dbContext,
            DeleteEquipmentReservationHandler deleteEquipmentReservationHandler,
            SendEmailForBookingEquipmentOnlyHandler sendEmailForBookingEquipmentOnlyHandler)
        {
            _dbContext = dbContext;
            _deleteEquipmentReservationHandler = deleteEquipmentReservationHandler;
            _sendEmailForBookingEquipmentOnlyHandler = sendEmailForBookingEquipmentOnlyHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteBookingEquipmentOnlyRequest, DeleteBookingEquipmentOnlyValidator>();

            var userid = AuthInfo.UserId;

            var listDeleteParam = new DeleteEquipmentReservationRequest();

            listDeleteParam.EquipmentReservationRequestMappings = new List<DeleteEquipmentReservationRequest_Mapping>();
            listDeleteParam.IdSchool = param.IdSchool;

            var deleteParam = new DeleteEquipmentReservationRequest_Mapping
            {
                IdMappingEquipmentReservation = param.IdMappingEquipmentReservations,
                IdUser = userid,
            };

            listDeleteParam.EquipmentReservationRequestMappings.Add(deleteParam);

            var getListTransaction = await _dbContext.Entity<TrMappingEquipmentReservation>()
                .Include(x => x.EquipmentReservations).ThenInclude(x => x.Equipment).ThenInclude(x => x.EquipmentType).ThenInclude(x => x.ReservationOwner).ThenInclude(x => x.ReservationOwnerEmails)
                .Where(x => param.IdMappingEquipmentReservations.Contains(x.Id))
                .ToListAsync(CancellationToken);

            var getListUser = await _dbContext.Entity<MsUser>()
                .Where(x => getListTransaction.Select(y => y.IdUser).Contains(x.Id) || x.Id == userid)
                .ToListAsync(CancellationToken);

            var getListVenue = await _dbContext.Entity<MsVenue>()
                .Include(x => x.Building)
                .Where(x => getListTransaction.Select(y => y.IdVenue).Contains(x.Id))
                .ToListAsync(CancellationToken);

            var schoolName = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == param.IdSchool)
                .Select(x => x.Description)
                .FirstOrDefaultAsync(CancellationToken);

            var res = await _deleteEquipmentReservationHandler.DeleteEquipmentReservation(listDeleteParam);

            if (!res.isSuccess)
            {
                throw new Exception(res.errorMessage);
            }

            //create email
            foreach (var emailContent in getListTransaction)
            {
                var buildingVenue = "-";
                if (emailContent.IdVenue != null)
                {
                    buildingVenue = getListVenue
                       .Where(x => x.Id == emailContent.IdVenue)
                       .Select(x => x.Building.Code + " - " + x.Description)
                       .FirstOrDefault();
                }
                else
                {
                    buildingVenue = emailContent.VenueNameinEquipment;
                }

                var emailRequest = new SendEmailForBookingEquipmentOnlyRequest
                {
                    RequesterName = getListUser.Where(x => x.Id == emailContent.IdUser).Select(x => x.DisplayName).FirstOrDefault(),
                    Action = "Cancelled",
                    SubjectAction = "Has Been Cancelled",
                    UserInputted = getListUser.Where(x => x.Id == userid).Select(x => x.DisplayName + " - " + x.Id + " (" + x.Email + ")").FirstOrDefault(),
                    Requester = getListUser.Where(x => x.Id == emailContent.IdUser).Select(x => x.DisplayName + " - " + x.Id + " (" + x.Email + ")").FirstOrDefault(),
                    BuildingVenue = buildingVenue,
                    EventDate = emailContent.ScheduleStartDate.ToString("dd MMMM yyyy"),
                    EventTime = emailContent.ScheduleStartDate.ToString(@"hh\:mm") + " - " + emailContent.ScheduleEndDate.ToString(@"hh\:mm"),
                    EventDescription = emailContent.EventDescription,
                    Notes = emailContent.Notes ?? "-",
                    SchoolName = schoolName,
                    EquipmentList = emailContent.EquipmentReservations.Select(x => new SendEmailForBookingEquipmentOnlyRequest_Equipment
                    {
                        EquipmentName = x.Equipment.EquipmentName,
                        BorrowingQty = x.EquipmentBorrowingQty.ToString()
                    }).ToList(),
                    IdSchool = param.IdSchool,
                    SendToEmail = getListUser.Where(x => x.Id == emailContent.IdUser).Select(x => x.Email).FirstOrDefault(),
                    SendToName = getListUser.Where(x => x.Id == emailContent.IdUser).Select(x => x.DisplayName).FirstOrDefault(),
                };

                res = await _sendEmailForBookingEquipmentOnlyHandler.SendEmailForBookingEquipmentOnly_ToRequester(emailRequest);

                //if (!res.isSuccess)
                //{
                //    throw new Exception("Failed to send email to requester");
                //}

                var listEquipmentType = emailContent.EquipmentReservations
                    .GroupBy(x => x.Equipment.EquipmentType)
                    .Select(x => new
                    {
                        x.Key,
                        Equipments = x.Select(y => new
                        {
                            y.Equipment.EquipmentName,
                            y.Equipment.Id
                        })
                    }).ToList();

                foreach (var equipmentType in listEquipmentType)
                {
                    var listEmail = equipmentType.Key.ReservationOwner.ReservationOwnerEmails
                        .Select(x => new SendEmailForBookingEquipmentOnlyRequest_PICOwnerEmail
                        {
                            Email = x.OwnerEmail,
                            IsTo = x.IsOwnerEmailTo,
                            IsCC = x.IsOwnerEmailCC,
                            IsBCC = x.IsOwnerEmailBCC
                        }).ToList();

                    var emailRequestPIC = new SendEmailForBookingEquipmentOnlyRequest
                    {
                        EquipmentType = equipmentType.Key.EquipmentTypeName,
                        Action = "Cancelled",
                        SubjectAction = "Has Been Cancelled",
                        UserInputted = getListUser.Where(x => x.Id == userid).Select(x => x.DisplayName + " - " + x.Id + " (" + x.Email + ")").FirstOrDefault(),
                        Requester = getListUser.Where(x => x.Id == emailContent.IdUser).Select(x => x.DisplayName + " - " + x.Id + " (" + x.Email + ")").FirstOrDefault(),
                        BuildingVenue = buildingVenue,
                        EventDate = emailContent.ScheduleStartDate.ToString("dd MMMM yyyy"),
                        EventTime = emailContent.ScheduleStartDate.ToString(@"hh\:mm") + " - " + emailContent.ScheduleEndDate.ToString(@"hh\:mm"),
                        EventDescription = emailContent.EventDescription,
                        Notes = emailContent.Notes ?? "-",
                        IdSchool = param.IdSchool,
                        SchoolName = schoolName,
                    };

                    emailRequestPIC.SendToPIC = listEmail;

                    var equipments = equipmentType.Equipments
                        .Where(x => emailContent.EquipmentReservations.Select(y => y.IdEquipment).Contains(x.Id))
                        .Select(x => new SendEmailForBookingEquipmentOnlyRequest_Equipment
                        {
                            EquipmentName = x.EquipmentName,
                            BorrowingQty = emailContent.EquipmentReservations.Where(y => y.IdEquipment == x.Id).Select(y => y.EquipmentBorrowingQty).FirstOrDefault().ToString(),
                        }).ToList();

                    emailRequestPIC.EquipmentList = equipments;

                    res = await _sendEmailForBookingEquipmentOnlyHandler.SendEmailForBookingEquipmentOnly_ToPICOwner(emailRequestPIC);

                    //if (!res.isSuccess)
                    //{
                    //    throw new Exception("Failed to send email to PIC Owner");
                    //}

                }
            }

            return Request.CreateApiResult2();

        }
    }
}
