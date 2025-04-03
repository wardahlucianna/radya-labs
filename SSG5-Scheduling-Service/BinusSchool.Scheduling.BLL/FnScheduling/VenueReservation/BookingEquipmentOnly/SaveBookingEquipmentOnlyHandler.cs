using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly.Helper;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class SaveBookingEquipmentOnlyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly SaveEquipmentReservationHandler _saveEquipmentReservationHandler;
        private readonly SendEmailForBookingEquipmentOnlyHandler _sendEmailForBookingEquipmentOnlyHandler;
        public SaveBookingEquipmentOnlyHandler(ISchedulingDbContext dbContext,
            SaveEquipmentReservationHandler saveEquipmentReservationHandler,
            SendEmailForBookingEquipmentOnlyHandler sendEmailForBookingEquipmentOnlyHandler)
        {
            _dbContext = dbContext;
            _saveEquipmentReservationHandler = saveEquipmentReservationHandler;
            _sendEmailForBookingEquipmentOnlyHandler = sendEmailForBookingEquipmentOnlyHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveBookingEquipmentOnlyRequest, SaveBookingEquipmentOnlyValidator>();

            var listSaveParam = new SaveEquipmentReservationRequest();

            listSaveParam.IdSchool = param.IdSchool;
            listSaveParam.EquipmentReservationMapping = new List<SaveEquipmentReservationRequest_Mapping>();

            var userLoginId = AuthInfo.UserId;
            listSaveParam.IdUserLogin = userLoginId;

            var saveParam = new SaveEquipmentReservationRequest_Mapping
            {
                IdMappingEquipmentReservation = param.IdMappingEquipmentReservation,
                ScheduleStartDate = param.Date.Add(param.StartTime),
                ScheduleEndDate = param.Date.Add(param.EndTime),
                IdUser = param.IdUser,
                IdVenue = param.IdVenue,
                EventDescription = param.EventDescription,
                VenueNameinEquipment = param.VenueNameinEquipment,
                Notes = param.Notes,
                ListEquipment = param.ListEquipment.Select(x => new SaveEquipmentReservationRequest_Equipment
                {
                    IdEquipment = x.IdEquipment,
                    EquipmentBorrowingQty = x.EquipmentBorrowingQty
                }).ToList()
            };

            listSaveParam.EquipmentReservationMapping.Add(saveParam);

            var res = await _saveEquipmentReservationHandler.SaveEquipmentReservation(listSaveParam);

            if (!res.isSuccess)
            {
                throw new Exception(res.errorMessage);
            }


            var getUser = await _dbContext.Entity<MsUser>()
                .Where(x => x.Id == userLoginId || x.Id == param.IdUser)
                .ToListAsync(CancellationToken);

            var getUserLogin = getUser
                .Where(x => x.Id == userLoginId)
                .FirstOrDefault();

            var getUserRequest = getUser
                .Where(x => x.Id == param.IdUser)
                .FirstOrDefault();

            var buildingVenue = "-";
            if (param.IdVenue != null)
            {
                buildingVenue = await _dbContext.Entity<MsVenue>()
                   .Include(x => x.Building)
                   .Where(x => x.Id == param.IdVenue)
                   .Select(x => x.Building.Code + " - " + x.Description)
                   .FirstOrDefaultAsync(CancellationToken);
            }
            else
            {
                buildingVenue = param.VenueNameinEquipment;
            }

            var schoolName = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == param.IdSchool)
                .Select(x => x.Description)
                .FirstOrDefaultAsync(CancellationToken);

            var getListEquipment = await _dbContext.Entity<MsEquipment>()
                .Include(x => x.EquipmentType).ThenInclude(x => x.ReservationOwner).ThenInclude(x => x.ReservationOwnerEmails)
                .Where(x => param.ListEquipment.Select(y => y.IdEquipment).Contains(x.Id))
                .ToListAsync(CancellationToken);

            var listEquipmentSendEmail = param.ListEquipment
                .Select(x => new SendEmailForBookingEquipmentOnlyRequest_Equipment
                {
                    EquipmentName = getListEquipment.Where(y => y.Id == x.IdEquipment).Select(y => y.EquipmentName).FirstOrDefault(),
                    BorrowingQty = x.EquipmentBorrowingQty.ToString()
                }).ToList();

            var emailRequest = new SendEmailForBookingEquipmentOnlyRequest
            {
                RequesterName = getUserRequest.DisplayName,
                Action = param.IdMappingEquipmentReservation == null ? "Created" : "Updated",
                SubjectAction = param.IdMappingEquipmentReservation == null ? "Successfully Created" : "Updated",
                UserInputted = (getUserLogin.DisplayName + " - " + getUserLogin.Id + " (" + getUserLogin.Email + ")"),
                Requester = (getUserRequest.DisplayName + " - " + getUserRequest.Id + " (" + getUserRequest.Email + ")"),
                BuildingVenue = buildingVenue,
                EventDate = param.Date.ToString("dd MMMM yyyy"),
                EventTime = param.StartTime.ToString(@"hh\:mm") + " - " + param.EndTime.ToString(@"hh\:mm"),
                EventDescription = param.EventDescription,
                Notes = param.Notes ?? "-",
                SchoolName = schoolName,
                EquipmentList = listEquipmentSendEmail,
                IdSchool = param.IdSchool,
                SendToName = getUserRequest.DisplayName,
                SendToEmail = getUserRequest.Email,
                
            };

            res = await _sendEmailForBookingEquipmentOnlyHandler.SendEmailForBookingEquipmentOnly_ToRequester(emailRequest);

            //if (!res.isSuccess)
            //{
            //    throw new Exception("Failed to send email to requester");
            //}

            var getEquipment = getListEquipment
                .GroupBy(x =>  x.EquipmentType)
                .Select(x => new
                {
                    x.Key,
                    Equipments = x.Select(y => new
                    {
                        y.EquipmentName,
                        y.Id
                    })
                })
                .ToList();

            foreach(var itemEquipment in getEquipment)
            {
                var listEmail = itemEquipment.Key.ReservationOwner?.ReservationOwnerEmails
                    .Select(x => new SendEmailForBookingEquipmentOnlyRequest_PICOwnerEmail
                    {
                        Email = x.OwnerEmail,
                        IsBCC = x.IsOwnerEmailBCC,
                        IsCC = x.IsOwnerEmailCC,
                        IsTo = x.IsOwnerEmailTo,
                        Name = x.ReservationOwner.OwnerName
                    }).ToList();

                emailRequest.SendToPIC = listEmail;
                emailRequest.EquipmentType = itemEquipment.Key.EquipmentTypeName;

                var equipments = itemEquipment.Equipments
                    .Where(x => param.ListEquipment.Select(y => y.IdEquipment).Contains(x.Id))
                    .Select(x => new SendEmailForBookingEquipmentOnlyRequest_Equipment
                    {
                        EquipmentName = x.EquipmentName,
                        BorrowingQty = param.ListEquipment.Where(y => y.IdEquipment == x.Id).Select(x => x.EquipmentBorrowingQty).FirstOrDefault().ToString(),
                    }).ToList();

                emailRequest.EquipmentList = equipments;

                if (!listEmail.Any())
                {
                    continue;
                }

                res = await _sendEmailForBookingEquipmentOnlyHandler.SendEmailForBookingEquipmentOnly_ToPICOwner(emailRequest);

                //if (!res.isSuccess)
                //{
                //    throw new Exception("Failed to send email to PIC Owner");
                //}

            }

            return Request.CreateApiResult2();
            
        }
    }
}
