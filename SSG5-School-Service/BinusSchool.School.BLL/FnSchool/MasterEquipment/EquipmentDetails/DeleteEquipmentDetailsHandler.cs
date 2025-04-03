using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Data.Model.School.FnSchool.MasterEquipment.EquipmentDetails;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.MasterEquipment.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NPOI.XSSF.UserModel;

namespace BinusSchool.School.FnSchool.MasterEquipment.EquipmentDetails
{
    public class DeleteEquipmentDetailsHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly IVenueReservation _venueReservationApi;
        public DeleteEquipmentDetailsHandler(ISchoolDbContext dbContext, IVenueReservation venueReservationApi
            )
        {
            _dbContext = dbContext;
            _venueReservationApi = venueReservationApi;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteEquipmentDetailsRequest, DeleteEquipmentDetailsValidator>();

            var getData = await _dbContext.Entity<MsEquipment>()
                .Include(x => x.EquipmentType)
                .Include(x => x.VenueEquipments)
                .Where(x => param.IdEquipment.Contains(x.Id))
                .ToListAsync(CancellationToken);

            var getCheckData = _venueReservationApi.GetListBookingEquipmentOnly(new GetListBookingEquipmentOnlyRequest
            {
                IdSchool = getData.Select(x => x.EquipmentType.IdSchool).FirstOrDefault(),
                GetAllData = true
            });

            var checkData = getCheckData.Result.Payload as List<GetListBookingEquipmentOnlyResult>;

            var isDataUsed = checkData.Select(x => x.ListEquipment).SelectMany(x => x)
                .Select(x => x.IdEquipment).Where(x => param.IdEquipment.Contains(x)).Any();


            if (isDataUsed)
            {
                throw new Exception("Cannot delete equipment because it is being used in booking equipment");
            }

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                foreach(var data in getData)
                {
                    if (data.VenueEquipments.Any())
                    {
                        throw new Exception("Cannot delete equipment because it is being used in venue equipment");
                    }


                    //Create History
                    var history = new HMsEquipment
                    {
                        IdHMsEquipment = Guid.NewGuid().ToString(),
                        IdEquipment = data.Id,
                        IdEquipmentType = data.IdEquipmentType,
                        EquipmentName = data.EquipmentName,
                        Description = data.Description,
                        TotalStockQty = data.TotalStockQty,
                        MaxQtyBorrowing = data.MaxQtyBorrowing,
                    };

                    _dbContext.Entity<HMsEquipment>().Add(history);

                    data.IsActive = false;
                    _dbContext.Entity<MsEquipment>().Update(data);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw ex;
            }
            finally
            {
                _transaction?.Dispose();
            }

            return Request.CreateApiResult2();
        }
    }
}
