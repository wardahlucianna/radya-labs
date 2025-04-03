using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentDetails;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXml4Net.OPC.Internal;
using NPOI.XSSF.UserModel;
namespace BinusSchool.School.FnSchool.MasterEquipment.EquipmentDetails
{
    public class GetListEquipmentDetailsHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IVenueReservation _venueReservation;
        public GetListEquipmentDetailsHandler(ISchoolDbContext dbContext, IVenueReservation venueReservation
            )
        {
            _dbContext = dbContext;
            _venueReservation = venueReservation;
        }   

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListEquipmentDetailsRequest>(nameof(GetListEquipmentDetailsRequest.IdSchool));

            var getEquipmentTransaction = await _venueReservation.GetListBookingEquipmentOnly(new GetListBookingEquipmentOnlyRequest
            {
                IdSchool = param.IdSchool,
                GetAllData = true
            });

            var getEquipmentTransactionData = getEquipmentTransaction.Payload as List<GetListBookingEquipmentOnlyResult>;

            var checkData = getEquipmentTransactionData.Select(x => x.ListEquipment).SelectMany(x => x)
                .Select(x => x.IdEquipment).ToList();

            var equipmentData = await _dbContext.Entity<MsEquipment>()
                .Include(x => x.EquipmentType)
                .Where(x => x.EquipmentType.IdSchool == param.IdSchool)
                .Where(x => x.IdEquipmentType == (param.IdEquipmentType ?? x.IdEquipmentType))
                .Select(x => new
                {
                    Id = x.Id,
                    EquipmentType = x.EquipmentType,
                    EquipmentName = x.EquipmentName,
                    TotalStockQty = x.TotalStockQty,
                    MaxQtyBorrowing = x.MaxQtyBorrowing,
                    Description = x.Description,
                    VenueEquipments = x.VenueEquipments
                })
                .ToListAsync(CancellationToken);

            // Perform the CanDelete logic in memory
            var data = equipmentData.Select(x => new GetListEquipmentDetailsResult
            {
                IdEquipment = x.Id,
                EquipmentType = new NameValueVm
                {
                    Id = x.EquipmentType.Id,
                    Name = x.EquipmentType.EquipmentTypeName
                },
                EquipmentName = x.EquipmentName,
                TotalStockQty = x.TotalStockQty,
                MaxQtyBorrowing = x.MaxQtyBorrowing,
                EquipmentDescription = x.Description,
                CanDelete =
                !checkData.Where(y => y == x.Id).Any() &&
                !x.VenueEquipments.Any()
            }).ToList();



            return Request.CreateApiResult2(data as object);
        }
    }
}
